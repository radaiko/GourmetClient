using Avalonia.FuncUI.Hosts;
using Avalonia.Threading;
using GourmetClient.MVU.Core;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;
using GourmetClient.MVU.Views;
using Avalonia;

namespace GourmetClient.MVU;

public class MainViewHostControl : HostControl {
  private readonly MvuDispatcher _dispatcher;

  public MainViewHostControl() {
    // Initialize MVU dispatcher with initial state
    _dispatcher = new MvuDispatcher(AppState.Initial);
    _dispatcher.SetStateChangedCallback(OnStateChanged);

    // Initialize the view with current state
    UpdateView(_dispatcher.CurrentState);

    // Load settings at startup
    _dispatcher.Dispatch(new InitializeApp());
  }

  private void OnStateChanged(AppState newState) {
    // Update UI on the UI thread
    Dispatcher.UIThread.Post(() => UpdateView(newState));
  }

  private void UpdateView(AppState state) {
    Content = MainView.Create(state, _dispatcher.Dispatch);
  }

  protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
  {
    _dispatcher?.Dispose();
    base.OnDetachedFromVisualTree(e);
  }
}
