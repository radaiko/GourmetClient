using System;
using System.ComponentModel;
using System.Windows;
using GourmetClient.Settings;
using GourmetClient.Utils;

namespace GourmetClient;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        SourceInitialized += OnSourceInitialized;
        Closing += OnClosing;
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        WindowSettings? windowSettings = InstanceProvider.SettingsService.GetCurrentWindowSettings();

        if (windowSettings is not null)
        {
            Top = windowSettings.WindowPositionTop;
            Left = windowSettings.WindowPositionLeft;
            Width = windowSettings.WindowWidth;
            Height = windowSettings.WindowHeight;

            if ((Top + Height) > (SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight))
            {
                Top = SystemParameters.VirtualScreenTop + SystemParameters.VirtualScreenHeight - Height;
            }

            if (Top < SystemParameters.VirtualScreenTop)
            {
                Top = SystemParameters.VirtualScreenTop;
            }

            if ((Left + Width) > (SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth))
            {
                Left = SystemParameters.VirtualScreenLeft + SystemParameters.VirtualScreenWidth - Width;
            }

            if (Left < SystemParameters.VirtualScreenLeft)
            {
                Left = SystemParameters.VirtualScreenLeft;
            }
        }
    }

    private void OnClosing(object? sender, CancelEventArgs e)
    {
        Closing -= OnClosing;

        var windowSettings = new WindowSettings((int)Top, (int)Left, (int)Width, (int)Height);

        if (!windowSettings.Equals(InstanceProvider.SettingsService.GetCurrentWindowSettings()))
        {
            InstanceProvider.SettingsService.SaveWindowSettings(windowSettings);
        }
    }
}