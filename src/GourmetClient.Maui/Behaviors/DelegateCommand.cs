using System;
using System.Windows.Input;

namespace GourmetClient.Maui.Behaviors;

public class DelegateCommand : ICommand
{
    private readonly Action _executeAction;
    private readonly Func<bool>? _canExecuteFunction;

    public DelegateCommand(Action executeAction, Func<bool>? canExecuteFunction = null)
    {
        _executeAction = executeAction ?? throw new ArgumentNullException(nameof(executeAction));
        _canExecuteFunction = canExecuteFunction;
    }

    public event EventHandler? CanExecuteChanged
    {
        // TODO: In MAUI, we might need a different approach for CanExecuteChanged
        // For now, keeping it simple
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecuteFunction?.Invoke() ?? true;
    }

    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            _executeAction();
        }
    }
}