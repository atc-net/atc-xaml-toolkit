// ReSharper disable ReturnTypeCanBeNotNullable
// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

[ValueConversion(typeof(object), typeof(Visibility))]
public sealed class NullToVisibilityCollapsedValueConverter :
    ValueConverterBase<object?, Visibility>,
    System.Windows.Data.IValueConverter
{
    public static readonly NullToVisibilityCollapsedValueConverter Instance = new();

    public override Visibility Convert(
        object? value,
        object? parameter,
        CultureInfo culture)
        => value is null
            ? Visibility.Collapsed
            : Visibility.Visible;

    public override object? ConvertBack(
        Visibility value,
        object? parameter,
        CultureInfo culture)
        => value == Visibility.Collapsed
            ? null
            : new object();

    object? System.Windows.Data.IValueConverter.Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => Convert(
            value,
            parameter,
            culture);

    object? System.Windows.Data.IValueConverter.ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
    {
        if (value is not Visibility visibility)
        {
            throw new UnexpectedTypeException(value?.GetType() ?? typeof(object), typeof(Visibility));
        }

        return ConvertBack(
            visibility,
            parameter,
            culture);
    }
}