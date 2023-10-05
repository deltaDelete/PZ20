using System;
using System.Windows.Input;

namespace PZ20.ViewModels; 

public class Command<T> : ICommand {
    private readonly Action<T?> _action;
    private readonly Func<T?, bool>? _canExecute;

    public Command(Action<T?> action, Func<T?, bool>? canExecute = null) {
        _action = action;
        _canExecute = canExecute;
    }
    public bool CanExecute(object? parameter) {
        return _canExecute?.Invoke((T?)parameter) ?? true;
    }

    public void Execute(object? parameter) {
        _action.Invoke((T?)parameter);
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged(object? sender, EventArgs e) {
        CanExecuteChanged?.Invoke(sender, e);
    }
    
    public void RaiseCanExecuteChanged() {
        CanExecuteChanged?.Invoke(null, EventArgs.Empty);
    }
}

public class Command : ICommand {
    private readonly Action _action;
    private readonly Func<bool>? _canExecute;

    public Command(Action action, Func<bool>? canExecute = null) {
        _action = action;
        _canExecute = canExecute;
    }
    public bool CanExecute(object? parameter) {
        return _canExecute?.Invoke() ?? true;
    }

    public void Execute(object? parameter) {
        _action.Invoke();
    }

    public event EventHandler? CanExecuteChanged;

    public void RaiseCanExecuteChanged(object? sender, EventArgs e) {
        CanExecuteChanged?.Invoke(sender, e);
    }
    
    public void RaiseCanExecuteChanged() {
        CanExecuteChanged?.Invoke(null, EventArgs.Empty);
    }
}