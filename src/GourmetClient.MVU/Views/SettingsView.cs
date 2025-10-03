using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;
using GourmetClient.MVU.Utils;

namespace GourmetClient.MVU.Views;

/// <summary>
/// Main settings view that routes to platform-specific implementations
/// </summary>
public static class SettingsView
{
    public static Control Create(AppState state, Action<Msg> dispatch)
    {
        // Use iOS-specific layout on iOS devices
        if (PlatformDetector.IsIOS)
        {
            return SettingsViewIOS.Create(state, dispatch);
        }

        // Use desktop layout for all other platforms
        return SettingsViewDesktop.Create(state, dispatch);
    }
}

/// <summary>
/// Shared utilities and styling for settings views across all platforms
/// </summary>
public static class SettingsViewShared
{
    public static SolidColorBrush GetTextBrush() =>
        new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
            ? Colors.White
            : Colors.Black);

    public static SolidColorBrush GetMinimalistBackgroundBrush() =>
        new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
            ? Color.Parse("#0d1117")
            : Color.Parse("#ffffff"));

    public static SolidColorBrush GetMinimalistBorderBrush() =>
        new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
            ? Color.Parse("#21262d")
            : Color.Parse("#d0d7de"));

    public static SolidColorBrush GetAccentBrush() =>
        new(Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark
            ? Color.Parse("#0969da")
            : Color.Parse("#0366d6"));

    public static int GetThemeIndex(string theme) => theme switch
    {
        "Hell" => 1,
        "Dunkel" => 2,
        _ => 0 // "System" or default
    };

    public static Control CreateMinimalistHeader()
    {
        var headerPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 8,
            Margin = new Thickness(0, 0, 0, 16)
        };

        var titleText = new TextBlock
        {
            Text = "Einstellungen",
            FontSize = 24,
            FontWeight = FontWeight.Light,
            Foreground = GetTextBrush()
        };
        headerPanel.Children.Add(titleText);

        var subtitleText = new TextBlock
        {
            Text = "Konfiguration der Anmeldedaten und Anwendungseinstellungen",
            FontSize = 14,
            FontWeight = FontWeight.Light,
            Foreground = GetTextBrush(),
            Opacity = 0.7
        };
        headerPanel.Children.Add(subtitleText);

        return headerPanel;
    }

    public static StackPanel CreateMinimalistSection(string title)
    {
        var section = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 20
        };

        var titleText = new TextBlock
        {
            Text = title,
            FontSize = 18,
            FontWeight = FontWeight.Normal,
            Foreground = GetTextBrush(),
            Margin = new Thickness(0, 0, 0, 4)
        };
        section.Children.Add(titleText);

        return section;
    }
}