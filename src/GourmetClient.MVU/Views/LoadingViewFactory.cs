using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace GourmetClient.MVU.Views;

/// <summary>
/// Factory for creating a lightweight animated loading view to avoid duplication
/// across different feature views (Menu, Billing, etc.).
/// </summary>
public static class LoadingViewFactory
{
    private const string SpinnerGlyph = "↻"; // unified spinner glyph

    public static Control Create(
        string message,
        double spinnerFontSize = 32,
        double textFontSize = 14,
        double spacing = 16,
        Thickness? margin = null,
        Thickness? textMargin = null,
        Color? spinnerColor = null,
        IBrush? textBrush = null,
        FontWeight? textFontWeight = null,
        double textOpacity = 1.0,
        double rotationStepDegrees = 6,   // 6 deg @16ms -> 360 in ~1s
        double timerIntervalMs = 16
    )
    {
        var panel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = spacing,
            Margin = margin ?? new Thickness()
        };

        var spinner = new TextBlock
        {
            Text = SpinnerGlyph,
            FontSize = spinnerFontSize,
            Foreground = new SolidColorBrush(spinnerColor ?? Color.Parse("#007ACC")),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            RenderTransformOrigin = RelativePoint.Center
        };

        var rotateTransform = new RotateTransform();
        spinner.RenderTransform = rotateTransform;

        var timer = new System.Timers.Timer(timerIntervalMs);
        double angle = 0;
        timer.Elapsed += (_, _) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                angle += rotationStepDegrees;
                if (angle >= 360) angle = 0;
                rotateTransform.Angle = angle;
            });
        };

        // Start / stop with visual tree to avoid orphan timers
        spinner.AttachedToVisualTree += (_, _) => timer.Start();
        spinner.DetachedFromVisualTree += (_, _) => timer.Stop();

        panel.Children.Add(spinner);

        var resolvedTextBrush = textBrush ?? new SolidColorBrush(
            Application.Current?.ActualThemeVariant == Avalonia.Styling.ThemeVariant.Dark ? Colors.White : Colors.Black);

        var messageBlock = new TextBlock
        {
            Text = message,
            FontSize = textFontSize,
            Foreground = resolvedTextBrush,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = textMargin ?? new Thickness(),
            FontWeight = textFontWeight ?? FontWeight.Normal,
            Opacity = textOpacity,
            TextAlignment = TextAlignment.Center
        };
        panel.Children.Add(messageBlock);

        return panel;
    }
}
