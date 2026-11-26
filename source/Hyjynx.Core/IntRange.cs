namespace Hyjynx.Core;

public readonly record struct IntRange(int Minimum, int Maximum)
{
    public void Validate()
    {
        if (Maximum < Minimum)
        {
            throw new ArgumentException("Maximum must be greater than or equal to Minimum.", nameof(Maximum));
        }
    }
}
