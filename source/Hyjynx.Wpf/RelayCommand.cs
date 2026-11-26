using System.Windows.Input;
using System.Windows.Threading;

namespace Hyjynx.Wpf;

public sealed class RelayCommand : ICommand
{
    private readonly Action? _executeParameterless;
    private readonly Action<object?>? _executeParameterized;
    private readonly Func<Task>? _executeAsyncParameterless;
    private readonly Func<object?, Task>? _executeAsyncParameterized;
    private readonly Func<bool>? _canExecuteParameterless;
    private readonly Predicate<object?>? _canExecuteParameterized;
    private readonly Dispatcher? _dispatcher;

    public RelayCommand(Action execute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        _executeParameterless = execute;
        _canExecuteParameterless = static () => true;
    }

    public RelayCommand(Action<object?> execute)
        : this(execute, static _ => true)
    {
    }

    public RelayCommand(Action execute, Func<bool> canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);
        _executeParameterless = execute;
        _canExecuteParameterless = canExecute;
    }

    public RelayCommand(Action<object?> execute, Predicate<object?> canExecute)
        : this(execute, canExecute, null)
    {
    }

    public RelayCommand(Func<Task> execute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        _executeAsyncParameterless = execute;
        _canExecuteParameterless = static () => true;
    }

    public RelayCommand(Func<Task> execute, Func<bool> canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);
        _executeAsyncParameterless = execute;
        _canExecuteParameterless = canExecute;
    }

    public RelayCommand(Func<object?, Task> execute)
        : this(execute, static _ => true)
    {
    }

    public RelayCommand(Func<object?, Task> execute, Predicate<object?> canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);
        _executeAsyncParameterized = execute;
        _canExecuteParameterized = canExecute;
    }

    public RelayCommand(Action execute, Func<bool> canExecute, Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);
        ArgumentNullException.ThrowIfNull(dispatcher);
        _executeParameterless = execute;
        _canExecuteParameterless = canExecute;
        _dispatcher = dispatcher;
    }

    public RelayCommand(Action<object?> execute, Predicate<object?> canExecute, Dispatcher? dispatcher)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);
        _executeParameterized = execute;
        _canExecuteParameterized = canExecute;
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
        => _canExecuteParameterless?.Invoke()
        ?? _canExecuteParameterized?.Invoke(parameter)
        ?? true;

    public void Execute(object? parameter)
    {
        if (_executeParameterless is not null)
            _executeParameterless();
        else if (_executeAsyncParameterless is not null)
            _ = ExecuteAsyncParameterless();
        else if (_executeAsyncParameterized is not null)
            _ = ExecuteAsyncParameterized(parameter);
        else
            _executeParameterized!(parameter);
    }

    public Task ExecuteAsync(object? parameter = null)
    {
        if (_executeParameterless is not null)
        {
            _executeParameterless();
            return Task.CompletedTask;
        }
        if (_executeAsyncParameterless is not null)
            return _executeAsyncParameterless();
        if (_executeAsyncParameterized is not null)
            return _executeAsyncParameterized(parameter);

        _executeParameterized!(parameter);
        return Task.CompletedTask;
    }

    private async Task ExecuteAsyncParameterless()
        => await _executeAsyncParameterless!().ConfigureAwait(true);

    private async Task ExecuteAsyncParameterized(object? parameter)
        => await _executeAsyncParameterized!(parameter).ConfigureAwait(true);

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
    private readonly Action<T?>? _execute;
    private readonly Func<T?, Task>? _executeAsync;
    private readonly Predicate<T?> _canExecute;
    private readonly Dispatcher? _dispatcher;

    public RelayCommand(Action<T?> execute)
        : this(execute, static _ => true)
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

    public RelayCommand(Func<T?, Task> execute)
        : this(execute, static _ => true)
    {
    }

    public RelayCommand(Func<T?, Task> execute, Predicate<T?> canExecute)
        : this(execute, canExecute, null)
    {
    }

    public RelayCommand(Func<T?, Task> execute, Predicate<T?> canExecute, Dispatcher? dispatcher)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);
        _executeAsync = execute;
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
        => _canExecute(ConvertParameter(parameter));

    public void Execute(object? parameter)
    {
        var value = ConvertParameter(parameter);
        if (_executeAsync is not null)
            _ = ExecuteAsync(value);
        else
            _execute!(value);
    }

    public Task ExecuteAsync(T? parameter = default)
        => _executeAsync is not null
            ? _executeAsync(parameter)
            : InvokeSync(parameter);

    private Task InvokeSync(T? parameter)
    {
        _execute!(parameter);
        return Task.CompletedTask;
    }

    private static T? ConvertParameter(object? parameter)
        => parameter switch
        {
            null => default,
            T value => value,
            _ => throw new ArgumentException(
                $"Expected a parameter of type '{typeof(T).FullName}' or null.",
                nameof(parameter))
        };

    public void RaiseCanExecuteChanged()
    {
        var dispatcher = _dispatcher ?? System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is null)
        {
            if (SynchronizationContext.Current is not null)
            {
                SynchronizationContext.Current.Post(static state =>
                {
                    var relay = (RelayCommand<T>)state!;
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
}
