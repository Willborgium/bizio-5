using System.Globalization;
using System.Windows.Data;

namespace Hyjynx.Wpf.Converters;

public sealed class NullToBooleanConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var result = value is null;
        return (ValueConverterHelpers.IsInverted(parameter) || Invert) ? !result : result;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
