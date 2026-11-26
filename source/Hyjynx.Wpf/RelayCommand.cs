using System.Windows.Input;
using System.Windows.Threading;

namespace Hyjynx.Wpf;

public sealed class RelayCommand : ICommand
{
    private readonly Action<object?> _execute;
    private readonly Predicate<object?> _canExecute;
    private readonly Dispatcher? _dispatcher;

    public RelayCommand(Action execute)
        : this(_ => RequireExecute(execute)(), _ => true)
    {
        ArgumentNullException.ThrowIfNull(execute);
    }

    public RelayCommand(Action<object?> execute)
        : this(execute, _ => true)
    {
    }

    public RelayCommand(Action execute, Func<bool> canExecute)
        : this(_ => RequireExecute(execute)(), _ => RequireCanExecute(canExecute)())
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);
    }

    public RelayCommand(Action<object?> execute, Predicate<object?> canExecute)
        : this(execute, canExecute, null)
    {
    }

    public RelayCommand(Action execute, Func<bool> canExecute, Dispatcher dispatcher)
        : this(_ => RequireExecute(execute)(), _ => RequireCanExecute(canExecute)(), dispatcher)
    {
    }

    public RelayCommand(Action<object?> execute, Predicate<object?> canExecute, Dispatcher? dispatcher)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);
        _execute = execute;
        _canExecute = canExecute;
        _dispatcher = dispatcher;
    }

    public event EventHandler? CanExecuteChanged
    {
        add
        {
            if (value is not null)
            {
                CommandManager.RequerySuggested += value;
                _canExecuteChanged += value;
            }
        }
        remove
        {
            if (value is not null)
            {
                CommandManager.RequerySuggested -= value;
                _canExecuteChanged -= value;
            }
        }
    }

    private event EventHandler? _canExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute(parameter);

    private static Action RequireExecute(Action execute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        return execute;
    }

    private static Func<bool> RequireCanExecute(Func<bool> canExecute)
    {
        ArgumentNullException.ThrowIfNull(canExecute);
        return canExecute;
    }

    public void Execute(object? parameter) => _execute(parameter);

    public void RaiseCanExecuteChanged()
    {
        var dispatcher = _dispatcher ?? ApplicationDispatcher();
        if (dispatcher is null)
        {
            if (SynchronizationContext.Current is not null)
            {
                SynchronizationContext.Current.Post(static state =>
                {
                    var relay = (RelayCommand)state!;
                    relay._canExecuteChanged?.Invoke(relay, EventArgs.Empty);
                }, this);
            }
            else
            {
                _canExecuteChanged?.Invoke(this, EventArgs.Empty);
            }

            return;
        }

        if (dispatcher.CheckAccess())
        {
            _canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            dispatcher.Invoke(() => _canExecuteChanged?.Invoke(this, EventArgs.Empty), DispatcherPriority.Normal);
        }
    }

    private static Dispatcher? ApplicationDispatcher()
        => System.Windows.Application.Current?.Dispatcher;
}

public sealed class RelayCommand<T> : ICommand
{
    private readonly Action<T?> _execute;
    private readonly Predicate<T?> _canExecute;
    private readonly Dispatcher? _dispatcher;

    public RelayCommand(Action<T?> execute)
        : this(execute, _ => true)
    {
    }

    public RelayCommand(Action<T?> execute, Predicate<T?> canExecute)
        : this(execute, canExecute, null)
    {
    }

    public RelayCommand(Action<T?> execute, Predicate<T?> canExecute, Dispatcher? dispatcher)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);
        _execute = execute;
        _canExecute = canExecute;
        _dispatcher = dispatcher;
    }

    public event EventHandler? CanExecuteChanged
    {
        add
        {
            if (value is not null)
            {
                CommandManager.RequerySuggested += value;
                _canExecuteChanged += value;
            }
        }
        remove
        {
            if (value is not null)
            {
                CommandManager.RequerySuggested -= value;
                _canExecuteChanged -= value;
            }
        }
    }

    private event EventHandler? _canExecuteChanged;

    public bool CanExecute(object? parameter)
        => _canExecute(parameter is null ? default : (T?)parameter);

    public void Execute(object? parameter)
        => _execute(parameter is null ? default : (T?)parameter);

    public void RaiseCanExecuteChanged()
    {
        var dispatcher = _dispatcher ?? System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            _canExecuteChanged?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (dispatcher.CheckAccess())
        {
            _canExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            dispatcher.Invoke(() => _canExecuteChanged?.Invoke(this, EventArgs.Empty), DispatcherPriority.Normal);
        }
    }
}
