using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace Hyjynx.Wpf.Converters;

public sealed class BooleanToVisibilityConverter : IValueConverter
{
    public bool Invert { get; set; }

    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => (ValueConverterHelpers.IsInverted(parameter) || Invert)
            ? (value is true ? Visibility.Collapsed : Visibility.Visible)
            : (value is true ? Visibility.Visible : Visibility.Collapsed);

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is Visibility.Visible
            ? !(ValueConverterHelpers.IsInverted(parameter) || Invert)
            : (ValueConverterHelpers.IsInverted(parameter) || Invert);
}
