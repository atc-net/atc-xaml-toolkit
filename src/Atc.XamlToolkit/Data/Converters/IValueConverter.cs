namespace Atc.XamlToolkit.Data.Converters;

/// <summary>
/// Converts a binding value.
/// </summary>
public interface IValueConverter
{
    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The type of the target.</param>
    /// <param name="parameter">A user-defined parameter.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The converted value.</returns>
    object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture);

    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetType">The type of the target.</param>
    /// <param name="parameter">A user-defined parameter.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The converted value.</returns>
    object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture);
}