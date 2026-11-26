namespace Hyjynx.Wpf.Converters;

internal static class ValueConverterHelpers
{
    public static bool IsInverted(object? parameter)
        => parameter is bool inverted && inverted
            || bool.TryParse(parameter?.ToString(), out var parsed) && parsed;
}
