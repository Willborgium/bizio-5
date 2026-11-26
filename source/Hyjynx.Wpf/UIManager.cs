using System.Windows.Threading;

namespace Hyjynx.Wpf;

public sealed class UIManager
{
    private readonly Dispatcher? _dispatcher;
    private readonly SynchronizationContext? _context;

    private UIManager()
    {
    }

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
        dispatcher ??= System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is not null)
        {
            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken).Task;
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
        dispatcher ??= System.Windows.Application.Current?.Dispatcher;
        if (dispatcher is not null)
        {
            return cancellationToken.IsCancellationRequested
                ? Task.FromCanceled(cancellationToken)
                : dispatcher.InvokeAsync(action, DispatcherPriority.Normal, cancellationToken).Task.Unwrap();
        }

        var context = SynchronizationContext.Current;
        if (context is null)
        {
            throw new InvalidOperationException("No UI dispatcher or synchronization context is available.");
        }

        return PostAsync(context, action, cancellationToken);
    }

    public Task InvokeAsync(Action action)
        => InvokeOnCapturedContext(action);

    public Task InvokeAsync(Func<Task> action)
        => InvokeOnCapturedContext(action);

    public Task<T> InvokeAsync<T>(Func<T> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (_dispatcher is not null)
        {
            return _dispatcher.InvokeAsync(action, DispatcherPriority.Normal).Task;
        }
        if (_context is null)
        {
            throw new InvalidOperationException("No UI dispatcher or synchronization context is available.");
        }

        return PostAsync(_context, action);
    }

    private Task InvokeOnCapturedContext(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (_dispatcher is not null)
        {
            return _dispatcher.InvokeAsync(action, DispatcherPriority.Normal).Task;
        }
        if (_context is null)
        {
            throw new InvalidOperationException("No UI dispatcher or synchronization context is available.");
        }

        return PostAsync(_context, action, CancellationToken.None);
    }

    private Task InvokeOnCapturedContext(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        if (_dispatcher is not null)
        {
            return _dispatcher.InvokeAsync(action, DispatcherPriority.Normal).Task.Unwrap();
        }
        if (_context is null)
        {
            throw new InvalidOperationException("No UI dispatcher or synchronization context is available.");
        }

        return PostAsync(_context, action, CancellationToken.None);
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

    private static Task<T> PostAsync<T>(SynchronizationContext context, Func<T> action)
    {
        var source = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
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
