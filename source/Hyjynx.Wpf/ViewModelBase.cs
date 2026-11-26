using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace Hyjynx.Wpf;

public abstract class ViewModelBase : NotifyOnPropertyChanged
{
    private readonly ConcurrentDictionary<Action<object?>, ConcurrentDictionary<Predicate<object?>, ICommand>> _commands = new();
    private int _busyCount;
    private bool _isBusy;
    private readonly bool _canLoadAsync;

    protected ViewModelBase()
        : this(true)
    {
    }

    protected ViewModelBase(bool canLoadAsync)
    {
        _canLoadAsync = canLoadAsync;
    }

    public ICommand LoadViewModelCommand
        => _canLoadAsync
            ? GetAsyncCommand(LoadViewModel, isBusy: true)
            : GetCommand(LoadViewModel);

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    protected virtual void LoadViewModel(object? state) { }

    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected bool SetProperty<T>(
        ref T field,
        T value,
        Action? changed,
        [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref field, value, propertyName))
            return false;

        changed?.Invoke();
        return true;
    }

    protected ICommand GetCommand(Action<object?> execute, Predicate<object?> canExecute)
    {
        ArgumentNullException.ThrowIfNull(execute);
        ArgumentNullException.ThrowIfNull(canExecute);
        var predicates = _commands.GetOrAdd(execute, static _ =>
            new ConcurrentDictionary<Predicate<object?>, ICommand>());
        return predicates.GetOrAdd(canExecute, static (predicate, state) =>
            new RelayCommand(state.Execute, predicate), new CommandState(execute));
    }

    protected ICommand GetCommand(Action execute, Predicate<object?> canExecute)
        => GetCommand(_ => execute(), canExecute);

    protected ICommand GetCommand(Action execute, Func<bool> canExecute)
        => GetCommand(_ => execute(), _ => canExecute());

    protected ICommand GetCommand(Action<object?> execute)
        => GetCommand(execute, static _ => true);

    protected ICommand GetCommand(Action execute)
        => GetCommand(_ => execute(), static _ => true);

    protected static ICommand GetCommand<TParameter>(Action<TParameter?> execute)
        => GetCommand(new RelayCommand<TParameter>(execute));

    protected static ICommand GetCommand<TParameter>(Action<TParameter?> execute, Predicate<TParameter?> canExecute)
        => GetCommand(new RelayCommand<TParameter>(execute, canExecute));

    private static ICommand GetCommand(ICommand command) => command;

    protected ICommand GetAsyncCommand(
        Action<object?> execute,
        Predicate<object?> canExecute,
        bool isBusy = false)
        => GetCommand(
            async parameter => await ExecuteAsync(parameter, execute, isBusy).ConfigureAwait(false),
            canExecute);

    protected ICommand GetAsyncCommand(Action execute, Predicate<object?> canExecute, bool isBusy = false)
        => GetAsyncCommand(_ => execute(), canExecute, isBusy);

    protected ICommand GetAsyncCommand(Action execute, Func<bool> canExecute, bool isBusy = false)
        => GetAsyncCommand(_ => execute(), _ => canExecute(), isBusy);

    protected ICommand GetAsyncCommand(Action<object?> execute, bool isBusy = false)
        => GetAsyncCommand(execute, static _ => true, isBusy);

    protected ICommand GetAsyncCommand(Action execute, bool isBusy = false)
        => GetAsyncCommand(_ => execute(), static _ => true, isBusy);

    private async Task ExecuteAsync(object? parameter, Action<object?> execute, bool isBusy)
    {
        if (!isBusy)
        {
            await Task.Run(() => execute(parameter)).ConfigureAwait(false);
            return;
        }

        await RunAsync(() => execute(parameter)).ConfigureAwait(false);
    }

    protected async Task RunAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await EnterBusyAsync().ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await operation().ConfigureAwait(false);
        }
        finally
        {
            await ExitBusyAsync().ConfigureAwait(false);
        }
    }

    protected Task RunAsync(Action operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return RunAsync(
            async () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                await Task.Run(operation, cancellationToken).ConfigureAwait(false);
            },
            cancellationToken);
    }

    protected async Task<T> RunAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await EnterBusyAsync().ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await operation().ConfigureAwait(false);
        }
        finally
        {
            await ExitBusyAsync().ConfigureAwait(false);
        }
    }

    protected async Task<T> RunAsync<T>(Func<T> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await EnterBusyAsync().ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await Task.Run(operation, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await ExitBusyAsync().ConfigureAwait(false);
        }
    }

    private async Task EnterBusyAsync()
    {
        if (Interlocked.Increment(ref _busyCount) == 1)
        {
            try
            {
                await UIManager.InvokeAsync(
                    () => IsBusy = true,
                    CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                Interlocked.Decrement(ref _busyCount);
                throw;
            }
        }
    }

    private async Task ExitBusyAsync()
    {
        if (Interlocked.Decrement(ref _busyCount) == 0)
        {
            await UIManager.InvokeAsync(
                () => IsBusy = false,
                CancellationToken.None).ConfigureAwait(false);
        }
    }

    private sealed class CommandState(Action<object?> execute)
    {
        public Action<object?> Execute { get; } = execute;
    }
}
