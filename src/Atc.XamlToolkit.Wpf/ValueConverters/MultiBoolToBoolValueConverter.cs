namespace Atc.XamlToolkit.ValueConverters;

[ValueConversion(typeof(bool[]), typeof(bool))]
public sealed class MultiBoolToBoolValueConverter :
    MultiValueConverterBase<bool, bool>,
    System.Windows.Data.IMultiValueConverter
{
    public static readonly MultiBoolToBoolValueConverter Instance = new();

    public override bool Convert(
        bool[] values,
        object? parameter,
        CultureInfo culture)
        => values.All(b => b);

    public override object[] ConvertBack(
        bool value,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");

    object? System.Windows.Data.IMultiValueConverter.Convert(
        object[]? values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => ((Data.Converters.IMultiValueConverter)this).Convert(
            values,
            targetType,
            parameter,
            culture);

    object?[] System.Windows.Data.IMultiValueConverter.ConvertBack(
        object? value,
        Type[] targetTypes,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");
}