namespace Hyjynx.Core;

public sealed class ValueChangedEventArgs<TValue> : EventArgs
{
    public TValue Previous { get; }
    public TValue Current { get; }

    public ValueChangedEventArgs(TValue previous, TValue current)
    {
        Previous = previous;
        Current = current;
    }
}
