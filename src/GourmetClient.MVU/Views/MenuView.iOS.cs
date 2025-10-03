using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

/// <summary>
/// iOS-optimized menu view with vertical layout for narrow screens
/// </summary>
public static class MenuViewIOS
{
    public static Control Create(AppState state, Action<Msg> dispatch)
    {
        if (state.IsLoading)
        {
            return MenuViewShared.CreateLoadingView();
        }

        if (state.MenuDays == null || state.MenuDays.Count == 0)
        {
            if (state.Settings != null && 
                !string.IsNullOrEmpty(state.Settings.Username) && 
                !string.IsNullOrEmpty(state.Settings.Password))
            {
                dispatch(new LoadMenus());
                return MenuViewShared.CreateLoadingView();
            }
            
            return MenuViewShared.CreateWelcomeView(state, dispatch);
        }

        return CreateMobileMenuView(state, dispatch);
    }

    private static Control CreateMobileMenuView(AppState state, Action<Msg> dispatch)
    {
        var scrollViewer = new ScrollViewer
        {
            HorizontalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = Avalonia.Controls.Primitives.ScrollBarVisibility.Auto,
            Padding = new Thickness(20, 20),
            Background = MenuViewShared.GetBackgroundBrush()
        };

        var mainPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 32
        };

        // Create menu days with minimalist cards, vertical layout for mobile
        foreach (var menuDay in state.MenuDays!)
        {
            var dayCard = CreateMinimalistMenuDayCard(menuDay, dispatch);
            mainPanel.Children.Add(dayCard);
        }

        scrollViewer.Content = mainPanel;
        return scrollViewer;
    }

    private static Control CreateMinimalistMenuDayCard(GourmetMenuDayViewModel menuDay, Action<Msg> dispatch)
    {
        var dayPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16,
            Margin = new Thickness(0, 0, 0, 40)
        };

        // Simple date header
        var dateHeader = new TextBlock
        {
            Text = menuDay.Date.ToString("dddd, dd. MMMM yyyy"),
            FontWeight = FontWeight.Normal,
            FontSize = 18,
            Foreground = MenuViewShared.GetTextBrush(),
            Margin = new Thickness(0, 0, 0, 16)
        };
        dayPanel.Children.Add(dateHeader);

        // Vertical menu layout for mobile (instead of horizontal for desktop)
        var menusPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 16
        };

        // Menu cards - same style as desktop but full width
        foreach (var menu in menuDay.Menus.Take(4))
        {
            if (menu != null)
            {
                var menuCard = MenuViewShared.CreateMinimalistMenuCard(menu, dispatch, null);
                menusPanel.Children.Add(menuCard);
            }
        }

        dayPanel.Children.Add(menusPanel);
        return dayPanel;
    }
}


