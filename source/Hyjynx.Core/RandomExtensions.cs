namespace Hyjynx.Core;

public static class RandomExtensions
{
    public static T Random<T>(this Random random)
        where T : struct, Enum
    {
        ArgumentNullException.ThrowIfNull(random);
        var values = Enum.GetValues<T>();
        return values[random.Next(values.Length)];
    }

    public static T Random<T>(this IEnumerable<T> collection)
    {
        ArgumentNullException.ThrowIfNull(collection);
        var values = collection as IReadOnlyList<T> ?? collection.ToArray();
        if (values.Count == 0)
        {
            throw new ArgumentException("Collection cannot be empty.", nameof(collection));
        }

        return values[System.Random.Shared.Next(values.Count)];
    }

    public static T Random<T>(this IEnumerable<T> collection, Func<T, bool> condition)
    {
        ArgumentNullException.ThrowIfNull(collection);
        ArgumentNullException.ThrowIfNull(condition);
        var values = collection.Where(condition).ToArray();
        if (values.Length == 0)
        {
            throw new ArgumentException("Collection has no values matching the condition.", nameof(collection));
        }

        return values[System.Random.Shared.Next(values.Length)];
    }

    public static int Random(this IntRange range)
    {
        range.Validate();
        var span = (long)range.Maximum - range.Minimum + 1;
        if (span <= int.MaxValue)
        {
            return range.Minimum + System.Random.Shared.Next((int)span);
        }

        var offset = System.Random.Shared.NextInt64(span);
        return checked((int)(range.Minimum + offset));
    }

    public static float Random(this FloatRange range)
    {
        range.Validate();
        return range.Minimum + (float)(System.Random.Shared.NextDouble() * (range.Maximum - range.Minimum));
    }

    public static double Random(this DoubleRange range)
    {
        range.Validate();
        return range.Minimum + (System.Random.Shared.NextDouble() * (range.Maximum - range.Minimum));
    }
}
