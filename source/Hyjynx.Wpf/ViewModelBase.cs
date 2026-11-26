using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Hyjynx.Wpf;

public abstract class ViewModelBase : INotifyPropertyChanged
{
    private readonly UIManager _uiManager;
    private bool _isBusy;

    protected ViewModelBase()
    {
        _uiManager = UIManager.CaptureCurrent();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

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

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler is null)
        {
            return;
        }

        var args = new PropertyChangedEventArgs(propertyName);
        _ = _uiManager.InvokeAsync(() => handler(this, args));
    }

    protected async Task RunAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);
        await _uiManager.InvokeAsync(() => IsBusy = true).ConfigureAwait(false);
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            await operation().ConfigureAwait(false);
        }
        finally
        {
            await _uiManager.InvokeAsync(() => IsBusy = false).ConfigureAwait(false);
        }
    }
}
