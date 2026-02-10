using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GourmetClient.Behaviors;

public class AsyncDelegateCommand<T> : ICommand
{
    private readonly Func<T?, Task> _executeMethod;
    private readonly Func<T?, bool> _canExecuteMethod;
    private bool _executing;

    public AsyncDelegateCommand(Func<T?, Task> executeMethod) : this(executeMethod, _ => true)
    {
    }

    public AsyncDelegateCommand(Func<T?, Task> executeMethod, Func<T?, bool> canExecuteMethod)
    {
        _executeMethod = executeMethod;
        _canExecuteMethod = canExecuteMethod;
    }

    public event EventHandler? CanExecuteChanged
    {
        add => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter)
    {
        return _canExecuteMethod((T?)parameter);
    }

    public async void Execute(object? parameter)
    {
        if (_executing || !CanExecute(parameter))
        {
            return;
        }

        try
        {
            _executing = true;
            await _executeMethod((T?)parameter);
        }
        finally
        {
            _executing = false;
        }
    }
}