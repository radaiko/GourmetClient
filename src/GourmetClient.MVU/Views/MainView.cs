using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;

namespace GourmetClient.MVU.Views;

public static class MainView {
  public static Control Create(AppState state, Action<Msg> dispatch) {
    var mainPanel = new DockPanel();

    // Top menu bar
    var menuBar = new StackPanel {
      Orientation = Orientation.Horizontal
    };
    DockPanel.SetDock(menuBar, Dock.Top);

    var menusButton = new Button { Content = "Menus" };
    menusButton.Click += (_, _) => dispatch(new LoadMenus());
    menuBar.Children.Add(menusButton);

    var billingButton = new Button { Content = "Billing" };
    billingButton.Click += (_, _) => dispatch(new ToggleBilling());
    menuBar.Children.Add(billingButton);

    var settingsButton = new Button { Content = "Settings" };
    settingsButton.Click += (_, _) => dispatch(new ToggleSettings());
    menuBar.Children.Add(settingsButton);

    var aboutButton = new Button { Content = "About" };
    aboutButton.Click += (_, _) => dispatch(new ToggleAbout());
    menuBar.Children.Add(aboutButton);

    mainPanel.Children.Add(menuBar);

    // Main content area
    var contentPanel = new StackPanel();

    // Error display
    if (!string.IsNullOrEmpty(state.ErrorMessage)) {
      var errorPanel = new StackPanel { Orientation = Orientation.Horizontal };

      var errorText = new TextBlock {
        Text = state.ErrorMessage,
        Foreground = Brushes.Red
      };
      errorPanel.Children.Add(errorText);

      var clearButton = new Button { Content = "Clear" };
      clearButton.Click += (_, _) => dispatch(new ClearError());
      errorPanel.Children.Add(clearButton);

      contentPanel.Children.Add(errorPanel);
    }

    // Loading indicator or main content
    if (state.IsLoading) {
      var loadingText = new TextBlock {
        Text = "Loading...",
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center
      };
      contentPanel.Children.Add(loadingText);
    }
    else {
      var welcomeText = new TextBlock {
        Text = "Gourmet Client - MVU Implementation",
        FontSize = 16,
        HorizontalAlignment = HorizontalAlignment.Center
      };
      contentPanel.Children.Add(welcomeText);
    }

    mainPanel.Children.Add(contentPanel);

    return mainPanel;
  }
}