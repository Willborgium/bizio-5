namespace Hyjynx.Core;

public sealed class ValueChangingEventArgs<TValue>(TValue current, TValue proposed) : EventArgs
{
    public TValue Current { get; } = current;
    public TValue Proposed { get; } = proposed;
    public bool Cancel { get; set; }
}
