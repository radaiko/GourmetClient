using System.Windows;
using GourmetClient.ViewModels;

namespace GourmetClient.Views;

public partial class SettingsView : InitializableView
{
    public SettingsView()
    {
        InitializeComponent();

        DataContext = new SettingsViewModel();
    }

    private void LoginPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (IsLoaded)
        {
            ((SettingsViewModel)DataContext).LoginPassword = LoginPasswordBox.Password;
        }
    }

    private void VentopayPasswordBox_OnPasswordChangedPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        if (IsLoaded)
        {
            ((SettingsViewModel)DataContext).VentopayPassword = VentopayPasswordBox.Password;
        }
    }
}