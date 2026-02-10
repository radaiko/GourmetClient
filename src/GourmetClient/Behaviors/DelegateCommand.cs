using System;
using System.Windows.Input;

namespace GourmetClient.Behaviors;

public class DelegateCommand : ICommand
{
    private readonly Action _executeMethod;
    private readonly Func<bool> _canExecuteMethod;

    public DelegateCommand(Action executeMethod) : this(executeMethod, () => true)
    {
    }

    public DelegateCommand(Action executeMethod, Func<bool> canExecuteMethod)
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
        return _canExecuteMethod();
    }

    public void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            _executeMethod();
        }
    }
}