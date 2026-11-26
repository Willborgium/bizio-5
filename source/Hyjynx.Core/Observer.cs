namespace Hyjynx.Core;

public sealed class Observer<T> : IUpdateable
    where T : struct
{
    private readonly Func<T> _subject;
    private T _value;

    public event EventHandler<ValueChangedEventArgs<T>>? ValueChanged;

    public Observer(Func<T> subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        _subject = subject;
        _value = subject();
    }

    public T Value => _value;

    public void Update()
    {
        var current = _subject();
        if (EqualityComparer<T>.Default.Equals(current, _value))
        {
            return;
        }

        var previous = _value;
        _value = current;
        ValueChanged?.Invoke(this, new ValueChangedEventArgs<T>(previous, current));
    }
}
