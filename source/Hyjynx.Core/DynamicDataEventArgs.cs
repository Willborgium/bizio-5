namespace Hyjynx.Core;

public sealed class DynamicDataEventArgs<T> : EventArgs, IDataEventArgs<T>
{
    private readonly Func<T> _dataGetter;

    public T Data => _dataGetter();

    public DynamicDataEventArgs(Func<T> dataGetter)
    {
        ArgumentNullException.ThrowIfNull(dataGetter);
        _dataGetter = dataGetter;
    }
}
