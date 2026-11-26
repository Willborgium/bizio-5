namespace Hyjynx.Core;

public sealed class EventingValue<TValue>
{
    private TValue _value;

    public EventingValue(TValue initialValue = default!)
    {
        _value = initialValue;
    }

    public TValue Value
    {
        get => _value;
        set
        {
            if (EqualityComparer<TValue>.Default.Equals(_value, value))
                return;

            var changing = new ValueChangingEventArgs<TValue>(_value, value);
            ValueChanging?.Invoke(this, changing);
            if (changing.Cancel)
                return;

            var previous = _value;
            _value = value;
            ValueChanged?.Invoke(this, new ValueChangedEventArgs<TValue>(previous, value));
        }
    }

    public event EventHandler<ValueChangingEventArgs<TValue>>? ValueChanging;
    public event EventHandler<ValueChangedEventArgs<TValue>>? ValueChanged;

    public static implicit operator TValue(EventingValue<TValue> value)
        => value.Value;

    public override string? ToString()
        => _value?.ToString();
}
