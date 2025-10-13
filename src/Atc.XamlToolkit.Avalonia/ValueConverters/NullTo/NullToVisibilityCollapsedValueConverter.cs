// ReSharper disable ReturnTypeCanBeNotNullable
// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

public sealed class NullToVisibilityCollapsedValueConverter :
    ValueConverterBase<object?, bool>,
    Avalonia.Data.Converters.IValueConverter
{
    public static readonly NullToVisibilityCollapsedValueConverter Instance = new();

    public override bool Convert(
        object? value,
        object? parameter,
        CultureInfo culture)
        => value is not null;

    public override object? ConvertBack(
        bool value,
        object? parameter,
        CultureInfo culture)
        => value
            ? new object()
            : null;

    object? Avalonia.Data.Converters.IValueConverter.Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => Convert(
            value,
            parameter,
            culture);

    object? Avalonia.Data.Converters.IValueConverter.ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not bool boolValue)
        {
            throw new UnexpectedTypeException(value?.GetType() ?? typeof(object), typeof(bool));
        }

        return ConvertBack(
            boolValue,
            parameter,
            culture);
    }
}