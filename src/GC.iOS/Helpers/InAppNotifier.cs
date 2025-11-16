using System;
using System.Linq;
using UIKit;
using CoreGraphics;

namespace GC.iOS.Helpers;

/// <summary>
/// Simple in-app notification banner shown at the top of the app window.
/// Non-modal and dismisses itself after a short delay.
/// </summary>
public static class InAppNotifier
{
    private const int BannerTag = 0xB4A0; // unique tag to find/remove existing banner

    /// <summary>
    /// Show an informational banner.
    /// </summary>
    public static void ShowInfo(string message) => Show(message, isError: false);

    /// <summary>
    /// Show an error banner.
    /// </summary>
    public static void ShowError(string message) => Show(message, isError: true);

    /// <summary>
    /// Show a banner with the provided message.
    /// This is safe to call from any thread.
    /// </summary>
    public static void Show(string message, bool isError)
    {
        UIApplication.SharedApplication.BeginInvokeOnMainThread(() =>
        {
            try
            {
                var window = UIApplication.SharedApplication.ConnectedScenes
                                .OfType<UIWindowScene>()
                                .SelectMany(s => s.Windows)
                                .LastOrDefault(w => w.IsKeyWindow);

                // Fallback to KeyWindow (older APIs)
#pragma warning disable CS0618
                if (window == null) window = UIApplication.SharedApplication.KeyWindow;
#pragma warning restore CS0618

                if (window == null) return;

                // Remove any existing banner
                var existing = window.ViewWithTag(BannerTag);
                existing?.RemoveFromSuperview();

                var topInset = window.SafeAreaInsets.Top;
                var bannerHeight = Math.Max(48, 44) + (nfloat)topInset;
                var banner = new UIView(new CGRect(0, -bannerHeight, window.Frame.Width, bannerHeight))
                {
                    Tag = BannerTag,
                    BackgroundColor = isError ? UIColor.SystemRed : UIColor.SystemGreen,
                    Alpha = 0.0f
                };

                var label = new UILabel(new CGRect(12, (nfloat)topInset, banner.Frame.Width - 24, bannerHeight - (nfloat)topInset))
                {
                    Text = message,
                    TextColor = UIColor.White,
                    Lines = 2,
                    LineBreakMode = UILineBreakMode.WordWrap,
                    Font = UIFont.SystemFontOfSize(14),
                    TextAlignment = UITextAlignment.Center,
                    BackgroundColor = UIColor.Clear
                };

                banner.AddSubview(label);
                window.AddSubview(banner);

                // Animate in
                UIView.Animate(0.35, () =>
                {
                    banner.Frame = new CGRect(0, 0, window.Frame.Width, bannerHeight);
                    banner.Alpha = 1.0f;
                });

                // Dismiss after delay
                var delay = isError ? 4.0 : 2.5;
                NSTimer.CreateScheduledTimer(delay, _ =>
                {
                    UIView.Animate(0.25, () =>
                    {
                        banner.Frame = new CGRect(0, -bannerHeight, window.Frame.Width, bannerHeight);
                        banner.Alpha = 0.0f;
                    }, () => banner.RemoveFromSuperview());
                });
            }
            catch (Exception ex)
            {
                // Best effort; don't crash the app if the notifier fails
                GC.Common.Logger.Error($"InAppNotifier failed: {ex}");
            }
        });
    }
}

