using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GourmetClient.Maui.Behaviors;

public class AsyncDelegateCommand : ICommand
{
    private readonly Func<Task> _executeFunction;
    private readonly Func<bool>? _canExecuteFunction;
    private bool _isExecuting;

    public AsyncDelegateCommand(Func<Task> executeFunction, Func<bool>? canExecuteFunction = null)
    {
        _executeFunction = executeFunction ?? throw new ArgumentNullException(nameof(executeFunction));
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
        return !_isExecuting && (_canExecuteFunction?.Invoke() ?? true);
    }

    public async void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            try
            {
                _isExecuting = true;
                await _executeFunction();
            }
            finally
            {
                _isExecuting = false;
            }
        }
    }
}