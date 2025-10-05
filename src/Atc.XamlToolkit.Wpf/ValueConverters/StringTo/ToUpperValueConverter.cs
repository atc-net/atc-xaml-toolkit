// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

[ValueConversion(typeof(string), typeof(string))]
public sealed class ToUpperValueConverter :
    ValueConverterBase<string, string>,
    System.Windows.Data.IValueConverter
{
    public static readonly ToUpperValueConverter Instance = new();

    public override string Convert(
        string value,
        object? parameter,
        CultureInfo culture)
        => string.IsNullOrEmpty(value)
            ? value
            : value.ToUpper(culture);

    public override string ConvertBack(
        string value,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");

    object? System.Windows.Data.IValueConverter.Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => ((Data.Converters.IValueConverter)this).Convert(
            value,
            targetType,
            parameter,
            culture);

    object? System.Windows.Data.IValueConverter.ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => ((Data.Converters.IValueConverter)this).ConvertBack(
            value,
            targetType,
            parameter,
            culture);
}