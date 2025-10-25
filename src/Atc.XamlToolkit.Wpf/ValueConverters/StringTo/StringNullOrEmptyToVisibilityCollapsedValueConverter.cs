// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

[ValueConversion(typeof(string), typeof(Visibility))]
public sealed class StringNullOrEmptyToVisibilityCollapsedValueConverter :
    ValueConverterBase<string?, Visibility>,
    System.Windows.Data.IValueConverter
{
    public static readonly StringNullOrEmptyToVisibilityCollapsedValueConverter Instance = new();

    public override Visibility Convert(
        string? value,
        object? parameter,
        CultureInfo culture)
        => value is null || string.IsNullOrEmpty(value)
            ? Visibility.Collapsed
            : Visibility.Visible;

    public override string? ConvertBack(
        Visibility value,
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