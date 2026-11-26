namespace Hyjynx.Core;

public readonly record struct DoubleRange(double Minimum, double Maximum)
{
    public void Validate()
    {
        if (double.IsNaN(Minimum) || double.IsNaN(Maximum) || Maximum < Minimum)
        {
            throw new ArgumentException("Maximum must be greater than or equal to Minimum and neither bound may be NaN.");
        }
    }
}
