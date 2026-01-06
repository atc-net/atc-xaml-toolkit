namespace Atc.XamlToolkit.Data.Converters;

/// <summary>
/// Converts a binding value.
/// </summary>
public interface IMultiValueConverter
{
    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="values">The values to convert.</param>
    /// <param name="targetType">The type of the target.</param>
    /// <param name="parameter">A user-defined parameter.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The converted value.</returns>
    object? Convert(
        object?[]? values,
        Type targetType,
        object? parameter,
        CultureInfo culture);

    /// <summary>
    /// Converts a value.
    /// </summary>
    /// <param name="value">The value to convert.</param>
    /// <param name="targetTypes">The types of the target.</param>
    /// <param name="parameter">A user-defined parameter.</param>
    /// <param name="culture">The culture to use.</param>
    /// <returns>The converted values.</returns>
    object?[]? ConvertBack(
        object? value,
        Type[] targetTypes,
        object? parameter,
        CultureInfo culture);
}