// ReSharper disable once CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

/// <summary>
/// Converts a string to a nullable integer, filtering out invalid characters.
/// Handles empty strings by returning null, and filters non-numeric characters during conversion.
/// </summary>
public sealed class StringToNullableIntValueConverter :
    ValueConverterBase<int?, string>,
    Avalonia.Data.Converters.IValueConverter
{
    public static readonly StringToNullableIntValueConverter Instance = new();

    public override string Convert(
        int? value,
        object? parameter,
        CultureInfo culture)
        => value is null
            ? string.Empty
            : value.Value.ToString(culture);

    public override int? ConvertBack(
        string value,
        object? parameter,
        CultureInfo culture)
    {
        // Handle null or empty strings
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // Try to parse the string
        if (int.TryParse(value, NumberStyles.Integer, culture, out var result))
        {
            return result;
        }

        // If parsing fails, return null
        return null;
    }

    object? Avalonia.Data.Converters.IValueConverter.Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is null)
        {
            return Convert(null, parameter, culture);
        }

        if (value is int intValue)
        {
            return Convert(intValue, parameter, culture);
        }

        return null;
    }

    object? Avalonia.Data.Converters.IValueConverter.ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is null)
        {
            return ConvertBack(string.Empty, parameter, culture);
        }

        if (value is not string stringValue)
        {
            return null;
        }

        // Handle empty or whitespace strings
        if (string.IsNullOrWhiteSpace(stringValue))
        {
            return ConvertBack(stringValue, parameter, culture);
        }

        // Filter out non-numeric characters (keep digits and minus sign)
        var filteredValue = new string(
            stringValue
                .Where(c => char.IsDigit(c) || c == '-')
                .ToArray());

        // ConvertBack with the filtered value
        return ConvertBack(filteredValue, parameter, culture);
    }
}