namespace Hyjynx.Core;

public readonly record struct FloatRange
{
    public FloatRange(float minimum, float maximum)
    {
        if (float.IsNaN(minimum) || float.IsNaN(maximum) || maximum < minimum)
            throw new ArgumentException("Maximum must be greater than or equal to Minimum and neither bound may be NaN.", nameof(maximum));

        Minimum = minimum;
        Maximum = maximum;
    }

    public float Minimum { get; }
    public float Maximum { get; }

    public void Validate()
    {
        if (float.IsNaN(Minimum) || float.IsNaN(Maximum) || Maximum < Minimum)
            throw new ArgumentException("Maximum must be greater than or equal to Minimum and neither bound may be NaN.");
    }

    public bool Contains(float value)
        => !float.IsNaN(value) && value >= Minimum && value <= Maximum;
}
