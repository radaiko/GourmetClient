using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

namespace GC.iOS;

[Register(nameof(AppDelegate))]
public class AppDelegate : MauiUIApplicationDelegate {
  protected override MauiApp CreateMauiApp() {
    // Remove borders from Entry controls to have ios26 style
    Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("SetupEntry", (handler, view) => {
      if (view is Microsoft.Maui.Controls.Entry entry) {
        handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
      }
    });
    return MauiProgram.CreateMauiApp();
  }
}