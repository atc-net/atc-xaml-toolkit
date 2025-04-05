namespace Atc.XamlToolkit.ValueConverters;

[ValueConversion(typeof(bool[]), typeof(Visibility))]
public sealed class MultiBoolToVisibilityVisibleValueConverter :
    MultiValueConverterBase<bool, Visibility>, System.Windows.Data.IMultiValueConverter
{
    public static readonly MultiBoolToVisibilityVisibleValueConverter Instance = new();

    public override Visibility Convert(
        bool[] values,
        object? parameter,
        CultureInfo culture)
        => values.All(b => b)
            ? Visibility.Visible
            : Visibility.Collapsed;

    public override object?[] ConvertBack(
        Visibility value,
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