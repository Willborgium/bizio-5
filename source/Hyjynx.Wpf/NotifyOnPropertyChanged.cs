using System.Collections.Concurrent;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hyjynx.Wpf;

public abstract class NotifyOnPropertyChanged : INotifyPropertyChanged
{
    private readonly ConcurrentDictionary<string, PropertyChangedEventArgs> _propertyChangedArgs = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, object?> _fields = new(StringComparer.Ordinal);

    protected NotifyOnPropertyChanged()
        : this(UIManager.CaptureCurrent())
    {
    }

    protected NotifyOnPropertyChanged(UIManager uiManager)
    {
        ArgumentNullException.ThrowIfNull(uiManager);
        UIManager = uiManager;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected UIManager UIManager { get; }

    protected T GetField<T>(T? defaultValue = default, [CallerMemberName] string? propertyName = null)
    {
        var name = propertyName ?? string.Empty;
        if (!_fields.TryGetValue(name, out var value))
            return defaultValue!;

        return value is T typedValue ? typedValue : defaultValue!;
    }

    protected bool SetField<T>(T value, bool notify = true, [CallerMemberName] string? propertyName = null)
    {
        var name = propertyName ?? string.Empty;
        while (true)
        {
            if (!_fields.TryGetValue(name, out var oldValue))
            {
                if (_fields.TryAdd(name, value))
                {
                    if (notify)
                        OnPropertyChanged(name);
                    return true;
                }
                continue;
            }

            var oldTyped = oldValue is T typedOldValue ? typedOldValue : default;
            if (EqualityComparer<T>.Default.Equals(oldTyped, value))
                return false;

            if (_fields.TryUpdate(name, value, oldValue))
            {
                if (notify)
                    OnPropertyChanged(name);
                return true;
            }
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler is null)
            return;

        var name = propertyName ?? string.Empty;
        var args = _propertyChangedArgs.GetOrAdd(
            name,
            static propertyName => new PropertyChangedEventArgs(propertyName));
        _ = UIManager.InvokeAsync(
            action: () => handler(this, args),
            cancellationToken: CancellationToken.None);
    }

}
