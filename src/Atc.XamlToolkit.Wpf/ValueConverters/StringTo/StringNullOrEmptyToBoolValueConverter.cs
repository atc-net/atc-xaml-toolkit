// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

[ValueConversion(typeof(string), typeof(bool))]
public sealed class StringNullOrEmptyToBoolValueConverter :
    ValueConverterBase<string, bool>,
    System.Windows.Data.IValueConverter
{
    public static readonly StringNullOrEmptyToBoolValueConverter Instance = new();

    public override bool Convert(
        string value,
        object? parameter,
        CultureInfo culture)
        => value is null ||
           string.IsNullOrEmpty(value);

    public override string ConvertBack(
        bool value,
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