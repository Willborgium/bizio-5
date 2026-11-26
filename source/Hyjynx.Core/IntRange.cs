namespace Hyjynx.Core;

public readonly record struct IntRange
{
    public IntRange(int minimum, int maximum)
    {
        if (maximum < minimum)
            throw new ArgumentException("Maximum must be greater than or equal to Minimum.", nameof(maximum));

        Minimum = minimum;
        Maximum = maximum;
    }

    public int Minimum { get; }
    public int Maximum { get; }

    public void Validate()
    {
        if (Maximum < Minimum)
            throw new ArgumentException("Maximum must be greater than or equal to Minimum.", nameof(Maximum));
    }

    public bool Contains(int value) => value >= Minimum && value <= Maximum;
    public int Length => checked((long)Maximum - Minimum + 1 <= int.MaxValue
        ? (int)((long)Maximum - Minimum + 1)
        : throw new OverflowException("The range length exceeds Int32."));
}
