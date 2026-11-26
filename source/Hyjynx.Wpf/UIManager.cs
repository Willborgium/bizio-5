using System.Windows.Threading;

namespace Hyjynx.Wpf;

public sealed class UIManager
{
    private readonly Dispatcher? _dispatcher;
    private readonly SynchronizationContext? _context;

    private UIManager() { }

    public bool HasUIContext => _dispatcher is not null || _context is not null;

    public bool CheckAccess()
        => _dispatcher?.CheckAccess()
        ?? (_context is not null && SynchronizationContext.Current == _context);

    public UIManager(Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        _dispatcher = dispatcher;
        _context = null;
    }

    public UIManager(SynchronizationContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        _context = context;
    }

    public static UIManager CaptureCurrent()
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is not null)
        {
            return new UIManager(dispatcher);
        }

        var context = SynchronizationContext.Current;
        return context is null ? new UIManager() : new UIManager(context);
    }

    public static Task InvokeAsync(Action action, Dispatcher? dispatcher = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        dispatcher ??= System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is not null)
        {
            if (dispatcher.CheckAccess())
                return InvokeOnCurrentThread(action);

            return dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken).Task;
        }

        var context = SynchronizationContext.Current;
        if (context is null)
        {
            throw new InvalidOperationException("No UI dispatcher or synchronization context is available.");
        }

        return PostAsync(context, action, cancellationToken);
    }

    public static Task InvokeAsync(Func<Task> action, Dispatcher? dispatcher = null, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        dispatcher ??= System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is not null)
        {
            if (dispatcher.CheckAccess())
                return InvokeAsyncOnCurrentThread(action);

            return dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken).Task.Unwrap();
        }

        var context = SynchronizationContext.Current;
        if (context is null)
        {
            throw new InvalidOperationException("No UI dispatcher or synchronization context is available.");
        }

        return PostAsync(context, action, cancellationToken);
    }

    public static Task<T> InvokeAsync<T>(
        Func<T> action,
        Dispatcher? dispatcher = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<T>(cancellationToken);

        dispatcher ??= System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is not null)
        {
            if (dispatcher.CheckAccess())
                return InvokeOnCurrentThread(action);

            return dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken).Task;
        }

        var context = SynchronizationContext.Current;
        if (context is null)
            throw new InvalidOperationException("No UI dispatcher or synchronization context is available.");

        return PostAsync(context, action, cancellationToken);
    }

    public static Task ExecuteAsync(Action action, Dispatcher? dispatcher = null, CancellationToken cancellationToken = default)
        => InvokeAsync(action, dispatcher, cancellationToken);

    public static Task ExecuteAsync(Func<Task> action, Dispatcher? dispatcher = null, CancellationToken cancellationToken = default)
        => InvokeAsync(action, dispatcher, cancellationToken);

    public static bool TryInvokeOnUIThread(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        try
        {
            _ = InvokeAsync(action);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public Task InvokeAsync(Action action, CancellationToken cancellationToken = default)
        => InvokeOnCapturedContext(action, cancellationToken);

    public Task InvokeAsync(Func<Task> action, CancellationToken cancellationToken = default)
        => InvokeOnCapturedContext(action, cancellationToken);

    public Task<T> InvokeAsync<T>(Func<T> action, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled<T>(cancellationToken);

        if (_dispatcher is not null)
        {
            if (_dispatcher.CheckAccess())
                return InvokeOnCurrentThread(action);

            return _dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken).Task;
        }
        if (_context is null)
        {
            throw new InvalidOperationException("No UI dispatcher or synchronization context is available.");
        }

        return PostAsync(_context, action, cancellationToken);
    }

    private Task InvokeOnCapturedContext(Action action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        if (_dispatcher is not null)
        {
            if (_dispatcher.CheckAccess())
                return InvokeOnCurrentThread(action);

            return _dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken).Task;
        }
        if (_context is null)
        {
            throw new InvalidOperationException("No UI dispatcher or synchronization context is available.");
        }

        return PostAsync(_context, action, cancellationToken);
    }

    private Task InvokeOnCapturedContext(Func<Task> action, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (cancellationToken.IsCancellationRequested)
            return Task.FromCanceled(cancellationToken);

        if (_dispatcher is not null)
        {
            if (_dispatcher.CheckAccess())
                return InvokeAsyncOnCurrentThread(action);

            return _dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken).Task.Unwrap();
        }
        if (_context is null)
        {
            throw new InvalidOperationException("No UI dispatcher or synchronization context is available.");
        }

        return PostAsync(_context, action, cancellationToken);
    }

    private static Task InvokeOnCurrentThread(Action action)
    {
        try
        {
            action();
            return Task.CompletedTask;
        }
        catch (Exception exception)
        {
            return Task.FromException(exception);
        }
    }

    private static async Task InvokeAsyncOnCurrentThread(Func<Task> action)
    {
        await action().ConfigureAwait(true);
    }

    private static Task<T> InvokeOnCurrentThread<T>(Func<T> action)
    {
        try
        {
            return Task.FromResult(action());
        }
        catch (Exception exception)
        {
            return Task.FromException<T>(exception);
        }
    }

    private static Task PostAsync(SynchronizationContext context, Action action, CancellationToken cancellationToken)
    {
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (cancellationToken.IsCancellationRequested)
        {
            source.SetCanceled(cancellationToken);
            return source.Task;
        }

        context.Post(static state =>
        {
            var work = (Tuple<Action, TaskCompletionSource>)state!;
            try
            {
                work.Item1();
                work.Item2.TrySetResult();
            }
            catch (Exception exception)
            {
                work.Item2.TrySetException(exception);
            }
        }, Tuple.Create(action, source));
        return source.Task;
    }

    private static Task PostAsync(SynchronizationContext context, Func<Task> action, CancellationToken cancellationToken)
    {
        var source = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        if (cancellationToken.IsCancellationRequested)
        {
            source.SetCanceled(cancellationToken);
            return source.Task;
        }

        context.Post(static async state =>
        {
            var work = (Tuple<Func<Task>, TaskCompletionSource>)state!;
            try
            {
                await work.Item1().ConfigureAwait(true);
                work.Item2.TrySetResult();
            }
            catch (Exception exception)
            {
                work.Item2.TrySetException(exception);
            }
        }, Tuple.Create(action, source));
        return source.Task;
    }

    private static Task<T> PostAsync<T>(
        SynchronizationContext context,
        Func<T> action,
        CancellationToken cancellationToken)
    {
        var source = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (cancellationToken.IsCancellationRequested)
        {
            source.SetCanceled(cancellationToken);
            return source.Task;
        }

        context.Post(static state =>
        {
            var work = (Tuple<Func<T>, TaskCompletionSource<T>>)state!;
            try
            {
                work.Item2.TrySetResult(work.Item1());
            }
            catch (Exception exception)
            {
                work.Item2.TrySetException(exception);
            }
        }, Tuple.Create(action, source));
        return source.Task;
    }
}
