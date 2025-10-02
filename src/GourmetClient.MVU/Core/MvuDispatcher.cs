using System.Collections.Concurrent;
using GourmetClient.MVU.Messages;
using GourmetClient.MVU.Models;
using GourmetClient.MVU.Update;

namespace GourmetClient.MVU.Core;

/// <summary>
///   Central MVU dispatcher that manages state updates and command execution
/// </summary>
public class MvuDispatcher : IDisposable {
  private readonly ConcurrentQueue<Msg> _messageQueue = new();
  private readonly object _stateLock = new();
  private readonly CancellationTokenSource _cancellationTokenSource = new();
  private readonly Task _processingTask;

  private AppState _currentState;
  private Action<AppState>? _stateChangedCallback;
  private bool _disposed;

  public MvuDispatcher(AppState initialState) {
    _currentState = initialState;
    _processingTask = Task.Run(ProcessMessages, _cancellationTokenSource.Token);
  }

  public AppState CurrentState {
    get {
      lock (_stateLock) {
        return _currentState;
      }
    }
  }

  public void SetStateChangedCallback(Action<AppState> callback) {
    _stateChangedCallback = callback;
  }

  public void Dispatch(Msg message) {
    if (_disposed) return;

    _messageQueue.Enqueue(message);
  }

  private async Task ProcessMessages() {
    while (!_cancellationTokenSource.Token.IsCancellationRequested) {
      if (_messageQueue.TryDequeue(out var message)) {
        await ProcessMessage(message);
      }
      else {
        // Wait a bit if no messages to process
        await Task.Delay(1, _cancellationTokenSource.Token);
      }
    }
  }

  private Task ProcessMessage(Msg message) {
    AppState newState;
    Cmd<Msg> cmd;

    // Update state synchronously
    lock (_stateLock) {
      (newState, cmd) = AppUpdate.UpdateState(message, _currentState);
      _currentState = newState;
    }

    // Notify UI of state change
    _stateChangedCallback?.Invoke(newState);

    // Execute commands asynchronously
    if (cmd is not Cmd<Msg>.None) {
      _ = Task.Run(async () => {
        try {
          await ExecuteCommand(cmd);
        }
        catch (Exception ex) {
          Dispatch(new ErrorOccurred($"Command execution failed: {ex.Message}"));
        }
      }, _cancellationTokenSource.Token);
    }

    return Task.CompletedTask;
  }

  private async Task ExecuteCommand(Cmd<Msg> cmd) {
    switch (cmd) {
      case Cmd<Msg>.None:
        break;

      case Cmd<Msg>.OfFunc funcCmd:
        var result = await funcCmd.AsyncFunc();
        Dispatch(result);
        break;

      case Cmd<Msg>.OfTask taskCmd:
        var taskResult = await taskCmd.Task;
        Dispatch(taskResult);
        break;

      case Cmd<Msg>.Batch batchCmd:
        var tasks = batchCmd.Commands.Select(ExecuteCommand);
        await Task.WhenAll(tasks);
        break;
    }
  }

  public void Dispose() {
    if (_disposed) return;

    _disposed = true;
    _cancellationTokenSource.Cancel();

    try {
      _processingTask.Wait(TimeSpan.FromSeconds(1));
    }
    catch (Exception) {
      // Ignore timeout or cancellation exceptions during disposal
    }

    _cancellationTokenSource.Dispose();
  }
}