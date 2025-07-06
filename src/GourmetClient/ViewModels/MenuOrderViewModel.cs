namespace GourmetClient.ViewModels
{
    using Behaviors;
    using GourmetClient.Model;
    using GourmetClient.Network;
    using GourmetClient.Settings;
    using GourmetClient.Utils;
    using Notifications;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows.Input;

    public class MenuOrderViewModel : ViewModelBase
    {
        private readonly GourmetCacheService _cacheService;

        private readonly GourmetSettingsService _settingsService;

        private readonly NotificationService _notificationService;

        private bool _showWelcomeMessage;

        private IReadOnlyList<GourmetMenuDayViewModel> _menuDays;

        private bool _isMenuUpdating;

        private string _nameOfUser;

        private DateTime _lastMenuUpdate;

        private bool _isSettingsPopupOpened;

        public MenuOrderViewModel()
        {
            _cacheService = InstanceProvider.GourmetCacheService;
            _settingsService = InstanceProvider.SettingsService;
            _notificationService = InstanceProvider.NotificationService;

            _menuDays = [];
            _nameOfUser = string.Empty;

            UpdateMenuCommand = new AsyncDelegateCommand(ForceUpdateMenu, () => !IsMenuUpdating);
            ExecuteSelectedOrderCommand = new AsyncDelegateCommand(ExecuteSelectedOrder, () => !IsMenuUpdating);
            ToggleMenuOrderedMarkCommand = new AsyncDelegateCommand<GourmetMenuViewModel>(ToggleMenuOrderedMark, CanToggleMenuOrderedMark);
        }

        public ICommand UpdateMenuCommand { get; }

        public ICommand ExecuteSelectedOrderCommand { get; }

        public ICommand ToggleMenuOrderedMarkCommand { get; }

        public bool ShowWelcomeMessage
        {
            get => _showWelcomeMessage;

            private set
            {
                if (_showWelcomeMessage != value)
                {
                    _showWelcomeMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public IReadOnlyList<GourmetMenuDayViewModel> MenuDays
        {
            get => _menuDays;

            private set
            {
                _menuDays = value;
                OnPropertyChanged();
            }
        }

        public bool IsMenuUpdating
        {
            get => _isMenuUpdating;

            private set
            {
                if (_isMenuUpdating != value)
                {
                    _isMenuUpdating = value;
                    OnPropertyChanged();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string NameOfUser
        {
            get => _nameOfUser;

            private set
            {
                if (_nameOfUser != value)
                {
                    _nameOfUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime LastMenuUpdate
        {
            get => _lastMenuUpdate;

            private set
            {
                if (_lastMenuUpdate != value)
                {
                    _lastMenuUpdate = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSettingsPopupOpened
        {
            get => _isSettingsPopupOpened;

            set
            {
                if (_isSettingsPopupOpened != value)
                {
                    _isSettingsPopupOpened = value;
                    OnPropertyChanged();
                }
            }
        }

        public override async void Initialize()
        {
            _settingsService.SettingsSaved += SettingsServiceOnSettingsSaved;

            var userSettings = _settingsService.GetCurrentUserSettings();

            if (!string.IsNullOrEmpty(userSettings.GourmetLoginUsername))
            {
                IsMenuUpdating = true;

                try
                {
                    await UpdateMenu();
                }
                finally
                {
                    IsMenuUpdating = false;
                }
            }
            else
            {
                ShowWelcomeMessage = true;
            }
        }

        private async Task ForceUpdateMenu()
        {
            IsMenuUpdating = true;

            try
            {
                _cacheService.InvalidateCache();
                await UpdateMenu();
            }
            finally
            {
                IsMenuUpdating = false;
            }
        }

        private async Task UpdateMenu()
        {
            GourmetCache cache = await _cacheService.GetCache();

            LastMenuUpdate = cache.Timestamp;
            NameOfUser = cache.UserInformation.NameOfUser;

            var dayViewModels = new List<GourmetMenuDayViewModel>();

            foreach (var dayGroup in cache.Menus.GroupBy(menu => menu.Day))
            {
                DateTime day = dayGroup.Key;
                var menuViewModels = new List<GourmetMenuViewModel>();

                foreach (var menu in dayGroup.OrderBy(menu => menu.MenuName))
                {
                    var menuViewModel = new GourmetMenuViewModel(menu);
                    var orderedMenu = cache.OrderedMenus.FirstOrDefault(orderedMenu => orderedMenu.MatchesMenu(menu));

                    if (orderedMenu != null)
                    {
                        menuViewModel.IsOrdered = true;
                        menuViewModel.IsOrderApproved = orderedMenu.IsOrderApproved;
                        menuViewModel.MenuState = GourmetMenuState.Ordered;

                        // TODO: Check if this can be found out somehow
                        menuViewModel.IsOrderCancelable = true;
                    }
                    else if (!menu.IsAvailable)
                    {
                        menuViewModel.MenuState = GourmetMenuState.NotAvailable;
                    }

                    menuViewModels.Add(menuViewModel);
                }

                dayViewModels.Add(new GourmetMenuDayViewModel(day, menuViewModels));
            }

            NotifyAboutConflictingOrderedMenus(cache.OrderedMenus);
            MenuDays = dayViewModels.OrderBy(viewModel => viewModel.Date).ToArray();
        }

        private void NotifyAboutConflictingOrderedMenus(IReadOnlyCollection<GourmetOrderedMenu> orderedMenus)
        {
            IEnumerable<GourmetOrderedMenu> duplicateOrderedMenus = orderedMenus
                .GroupBy(orderedMenu => orderedMenu)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            IEnumerable<DateTime> daysWithMultipleOrderedMenus = orderedMenus
                .GroupBy(orderedMenu => orderedMenu.Day)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key);

            foreach (var duplicateOrderedMenu in duplicateOrderedMenus)
            {
                _notificationService.Send(new Notification(NotificationType.Warning, $"Das Menü '{duplicateOrderedMenu.MenuName}' am {duplicateOrderedMenu.Day:dd.MM.yyyy} ist mehrfach bestellt"));
            }

            foreach (var dayWithMultipleOrderedMenus in daysWithMultipleOrderedMenus)
            {
                _notificationService.Send(new Notification(NotificationType.Warning, $"Am {dayWithMultipleOrderedMenus:dd.MM.yyyy} sind mehrere Menüs bestellt"));
            }
        }

        private async Task ExecuteSelectedOrder()
        {
            IsMenuUpdating = true;

            try
            {
                _cacheService.InvalidateCache();
                var currentData = await _cacheService.GetCache();

                var errorDays = new List<DateTime>();
                var menusToOrder = new List<GourmetMenu>();
                var menusToCancel = new List<GourmetOrderedMenu>();

                foreach (var dayViewModel in _menuDays)
                {
                    var menuToOrder = dayViewModel.Menus.SingleOrDefault(menu => menu.MenuState == GourmetMenuState.MarkedForOrder);

                    if (menuToOrder != null)
                    {
                        var menuModel = menuToOrder.GetModel();
                        var actualMenu = currentData.Menus.FirstOrDefault(menu => menu.Equals(menuModel));

                        if (actualMenu is { IsAvailable: true })
                        {
                            menusToOrder.Add(menuToOrder.GetModel());
                        }
                        else
                        {
                            errorDays.Add(dayViewModel.Date);
                            _notificationService.Send(new Notification(NotificationType.Error, $"{menuToOrder.MenuName} für den {dayViewModel.Date:dd.MM.yyyy} ist nicht mehr verfügbar"));
                        }
                    }
                }

                foreach (var dayViewModel in _menuDays.Where(day => !errorDays.Contains(day.Date)))
                {
                    foreach (var menuViewModel in dayViewModel.Menus)
                    {
                        if (menuViewModel.MenuState == GourmetMenuState.MarkedForCancel)
                        {
                            var menuModel = menuViewModel.GetModel();
                            var matchingOrderedMenus = currentData.OrderedMenus.Where(orderedMenu => orderedMenu.MatchesMenu(menuModel));
                            var menusToCancelCount = menusToCancel.Count;

                            // Cancel all orders in case the menu has been ordered multiple times
                            foreach (var actualOrderedMenu in matchingOrderedMenus)
                            {
                                // TODO: Apply IsOrderCancelable if this information is available
                                // if (!actualOrderedMenu.Meal.IsOrderCancelable)
                                //{
                                //   // Assume that, in case of multiple orders of the same menu, if one of the order can't be cancelled, then none of the orders can be cancelled
                                //   break;
                                //}

                                menusToCancel.Add(actualOrderedMenu);
                            }

                            if (menusToCancelCount == menusToCancel.Count)
                            {
                                // Nothing was added
                                _notificationService.Send(new Notification(NotificationType.Error, $"{menuViewModel.MenuName} für den {dayViewModel.Date:dd.MM.yyyy} kann nicht storniert werden"));
                            }
                        }
                    }
                }

                GourmetUpdateOrderResult updateOrderResult = await _cacheService.UpdateOrderedMenu(currentData.UserInformation, menusToOrder, menusToCancel);
                NotifyAboutFailedOrders(updateOrderResult);

                await UpdateMenu();
            }
            catch (Exception exception)
            {
                _notificationService.Send(new ExceptionNotification("Das Ausführen der Bestellung ist fehlgeschlagen", exception));
            }
            finally
            {
                IsMenuUpdating = false;
            }
        }

        private void NotifyAboutFailedOrders(GourmetUpdateOrderResult updateOrderResult)
        {
            foreach (FailedMenuToOrderInformation information in updateOrderResult.FailedMenusToOrder)
            {
                _notificationService.Send(
                    new Notification(
                        NotificationType.Warning,
                        $"Das Menü '{information.Menu.MenuName}' am {information.Menu.Day:dd.MM.yyyy} konnte nicht bestellt werden. Ursache: {information.Message}"));
            }
        }

        private bool CanToggleMenuOrderedMark(GourmetMenuViewModel? menuViewModel)
        {
            if (menuViewModel == null)
            {
                return false;
            }

            if (!menuViewModel.IsAvailable)
            {
                if (menuViewModel.IsOrdered && menuViewModel.IsOrderCancelable)
                {
                    // Menu can no longer be ordered, but it is ordered and the order can be canceled
                    return true;
                }

                // Menu can no longer be ordered
                return false;
            }

            if (menuViewModel.IsOrdered && !menuViewModel.IsOrderCancelable)
            {
                // Menu is ordered and the order cannot be canceled
                return false;
            }

            return true;
        }

        private Task ToggleMenuOrderedMark(GourmetMenuViewModel? menuViewModel)
        {
            if (menuViewModel == null)
            {
                return Task.CompletedTask;
            }

            if (menuViewModel.MenuState == GourmetMenuState.Ordered)
            {
                menuViewModel.MenuState = GourmetMenuState.MarkedForCancel;
            }
            else if (menuViewModel.MenuState == GourmetMenuState.MarkedForOrder)
            {
                menuViewModel.MenuState = GourmetMenuState.None;

                var orderedMenu = GetDayViewModel(menuViewModel).Menus.FirstOrDefault(menu => menu.IsOrdered);

                if (orderedMenu != null)
                {
                    orderedMenu.MenuState = GourmetMenuState.Ordered;
                }
            }
            else
            {
                var dayViewModel = GetDayViewModel(menuViewModel);

                foreach (var menuOfDay in GetMenusWhereOrderCanBeChanged(dayViewModel))
                {
                    if (menuOfDay == menuViewModel)
                    {
                        menuOfDay.MenuState = menuOfDay.IsOrdered ? GourmetMenuState.Ordered : GourmetMenuState.MarkedForOrder;
                    }
                    else
                    {
                        menuOfDay.MenuState = menuOfDay.IsOrdered ? GourmetMenuState.MarkedForCancel : GourmetMenuState.None;
                    }
                }
            }

            return Task.CompletedTask;
        }

        private IEnumerable<GourmetMenuViewModel> GetMenusWhereOrderCanBeChanged(GourmetMenuDayViewModel dayViewModel)
        {
            return dayViewModel.Menus.Where(menu => menu.MenuState != GourmetMenuState.NotAvailable && (!menu.IsOrdered || menu.IsOrderCancelable));
        }

        private GourmetMenuDayViewModel GetDayViewModel(GourmetMenuViewModel menuViewModel)
        {
            return _menuDays.First(day => day.Menus.Contains(menuViewModel));
        }

        private async void SettingsServiceOnSettingsSaved(object? sender, EventArgs e)
        {
            IsSettingsPopupOpened = false;
            ShowWelcomeMessage = false;

            await ForceUpdateMenu();
        }
    }
}
