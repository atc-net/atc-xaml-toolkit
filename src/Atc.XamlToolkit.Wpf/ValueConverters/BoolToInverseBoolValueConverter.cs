namespace Atc.XamlToolkit.ValueConverters;

[ValueConversion(typeof(bool), typeof(bool))]
public sealed class BoolToInverseBoolValueConverter :
    ValueConverterBase<bool, bool>,
    System.Windows.Data.IValueConverter
{
    public static readonly BoolToInverseBoolValueConverter Instance = new();

    public override bool Convert(
        bool value,
        object? parameter,
        CultureInfo culture)
        => !value;

    public override bool ConvertBack(
        bool value,
        object? parameter,
        CultureInfo culture)
        => !value;

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