using System.Threading.Tasks;
using System.Windows.Input;
using GourmetClient.Behaviors;
using GourmetClient.Settings;
using GourmetClient.Utils;

namespace GourmetClient.ViewModels;

public class SettingsViewModel : ViewModelBase
{
    private readonly GourmetSettingsService _settingsService;

    private string _loginUsername;

    private string _loginPassword;

    private string _ventopayUsername;

    private string _ventopayPassword;

    private bool _checkForUpdates;

    public SettingsViewModel()
    {
        _settingsService = InstanceProvider.SettingsService;

        _loginUsername = string.Empty;
        _loginPassword = string.Empty;
        _ventopayUsername = string.Empty;
        _ventopayPassword = string.Empty;

        SaveSettingsCommand = new AsyncDelegateCommand(SaveSettings);
    }

    public ICommand SaveSettingsCommand { get; }

    public string LoginUsername
    {
        get => _loginUsername;
        set
        {
            if (_loginUsername != value)
            {
                _loginUsername = value;
                OnPropertyChanged();
            }
        }
    }

    public string LoginPassword
    {
        private get => _loginPassword;
        set
        {
            _loginPassword = value;
            OnPropertyChanged();
        }
    }

    public string VentopayUsername
    {
        get => _ventopayUsername;
        set
        {
            if (_ventopayUsername != value)
            {
                _ventopayUsername = value;
                OnPropertyChanged();
            }
        }
    }

    public string VentopayPassword
    {
        private get => _ventopayPassword;
        set
        {
            _ventopayPassword = value;
            OnPropertyChanged();
        }
    }

    public bool CheckForUpdates
    {
        get => _checkForUpdates;
        set
        {
            if (_checkForUpdates != value)
            {
                _checkForUpdates = value;
                OnPropertyChanged();
            }
        }
    }

    public override void Initialize()
    {
        var userSettings = _settingsService.GetCurrentUserSettings();
        var updateSettings = _settingsService.GetCurrentUpdateSettings();

        LoginUsername = userSettings.GourmetLoginUsername;
        LoginPassword = userSettings.GourmetLoginPassword;
        VentopayUsername = userSettings.VentopayUsername;
        VentopayPassword = userSettings.VentopayPassword;

        CheckForUpdates = updateSettings.CheckForUpdates;
    }

    private Task SaveSettings()
    {
        var userSettings = _settingsService.GetCurrentUserSettings();
        var updateSettings = _settingsService.GetCurrentUpdateSettings();

        userSettings.GourmetLoginUsername = LoginUsername;
        userSettings.GourmetLoginPassword = LoginPassword;
        userSettings.VentopayUsername = VentopayUsername;
        userSettings.VentopayPassword = VentopayPassword;

        _settingsService.SaveUserSettings(userSettings);

        if (updateSettings.CheckForUpdates != CheckForUpdates)
        {
            updateSettings.CheckForUpdates = CheckForUpdates;
            _settingsService.SaveUpdateSettings(updateSettings);
        }

        return Task.CompletedTask;
    }
}