namespace Hyjynx.Core;

public readonly record struct FloatRange(float Minimum, float Maximum)
{
    public void Validate()
    {
        if (float.IsNaN(Minimum) || float.IsNaN(Maximum) || Maximum < Minimum)
        {
            throw new ArgumentException("Maximum must be greater than or equal to Minimum and neither bound may be NaN.");
        }
    }
}
