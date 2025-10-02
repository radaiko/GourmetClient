using Avalonia;
using Avalonia.FuncUI.Hosts;
using Avalonia.Threading;
using GourmetClient.MVU.Core;
using GourmetClient.MVU.Models;
using GourmetClient.MVU.Views;

namespace GourmetClient.MVU;

public class MainWindow : HostWindow {
  private readonly MvuDispatcher _dispatcher;

  public MainWindow() {
    Title = "Gourmet Client";
    Width = 800;
    Height = 900;
    MinWidth = 590;

    this.AttachDevTools();

    // Initialize MVU dispatcher with initial state
    _dispatcher = new MvuDispatcher(AppState.Initial);
    _dispatcher.SetStateChangedCallback(OnStateChanged);

    // Initialize the view with current state
    UpdateView(_dispatcher.CurrentState);
  }

  private void OnStateChanged(AppState newState) {
    // Update UI on the UI thread
    Dispatcher.UIThread.Post(() => UpdateView(newState));
  }

  private void UpdateView(AppState state) {
    Content = MainView.Create(state, _dispatcher.Dispatch);
  }

  protected override void OnClosed(EventArgs e) {
    _dispatcher?.Dispose();
    base.OnClosed(e);
  }
}