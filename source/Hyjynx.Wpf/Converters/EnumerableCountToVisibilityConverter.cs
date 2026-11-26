using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hyjynx.Wpf.Converters;

public sealed class EnumerableCountToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not IEnumerable collection)
            return Visibility.Collapsed;

        var count = 0;
        var enumerator = collection.GetEnumerator();
        while (enumerator.MoveNext())
            count++;

        if (enumerator is IDisposable disposable)
            disposable.Dispose();

        var (comparison, threshold) = ParseParameter(parameter);
        var visible = comparison switch
        {
            ComparisonType.EqualTo => count == threshold,
            ComparisonType.GreaterThan => count > threshold,
            ComparisonType.GreaterThanOrEqualTo => count >= threshold,
            ComparisonType.LessThan => count < threshold,
            ComparisonType.LessThanOrEqualTo => count <= threshold,
            ComparisonType.NotEqualTo => count != threshold,
            _ => false
        };

        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    private static (ComparisonType Comparison, int Threshold) ParseParameter(object? parameter)
    {
        var text = parameter?.ToString()?.Trim();
        if (string.IsNullOrEmpty(text))
            return (ComparisonType.GreaterThan, 0);

        var comparison = ComparisonType.GreaterThan;
        foreach (var prefix in new[] { "ge", "le", "ne", "gt", "lt", "eq", "=", ">", "<" })
        {
            if (text.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
            {
                comparison = prefix switch
                {
                    "ge" => ComparisonType.GreaterThanOrEqualTo,
                    "le" => ComparisonType.LessThanOrEqualTo,
                    "ne" => ComparisonType.NotEqualTo,
                    "gt" or ">" => ComparisonType.GreaterThan,
                    "lt" or "<" => ComparisonType.LessThan,
                    "eq" or "=" => ComparisonType.EqualTo,
                    _ => comparison
                };
                text = text[prefix.Length..].Trim();
                break;
            }
        }

        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var threshold)
            ? (comparison, threshold)
            : (comparison, 0);
    }

    private enum ComparisonType
    {
        EqualTo,
        GreaterThan,
        GreaterThanOrEqualTo,
        LessThan,
        LessThanOrEqualTo,
        NotEqualTo
    }
}
