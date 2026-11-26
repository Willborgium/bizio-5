namespace Hyjynx.Core;

public readonly record struct DoubleRange
{
    public DoubleRange(double minimum, double maximum)
    {
        if (double.IsNaN(minimum) || double.IsNaN(maximum) || maximum < minimum)
            throw new ArgumentException("Maximum must be greater than or equal to Minimum and neither bound may be NaN.", nameof(maximum));

        Minimum = minimum;
        Maximum = maximum;
    }

    public double Minimum { get; }
    public double Maximum { get; }

    public void Validate()
    {
        if (double.IsNaN(Minimum) || double.IsNaN(Maximum) || Maximum < Minimum)
            throw new ArgumentException("Maximum must be greater than or equal to Minimum and neither bound may be NaN.");
    }

    public bool Contains(double value)
        => !double.IsNaN(value) && value >= Minimum && value <= Maximum;
}
