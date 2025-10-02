using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GourmetClient.Maui.Behaviors;

public class AsyncDelegateCommand<T> : ICommand
{
    private readonly Func<T?, Task> _executeFunction;
    private readonly Func<T?, bool>? _canExecuteFunction;
    private bool _isExecuting;

    public AsyncDelegateCommand(Func<T?, Task> executeFunction, Func<T?, bool>? canExecuteFunction = null)
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
        return !_isExecuting && (_canExecuteFunction?.Invoke((T?)parameter) ?? true);
    }

    public async void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            try
            {
                _isExecuting = true;
                await _executeFunction((T?)parameter);
            }
            finally
            {
                _isExecuting = false;
            }
        }
    }
}