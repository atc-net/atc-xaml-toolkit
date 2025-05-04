// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

[ValueConversion(typeof(bool), typeof(Visibility))]
public sealed class BoolToVisibilityVisibleValueConverter :
    ValueConverterBase<bool, Visibility>,
    System.Windows.Data.IValueConverter
{
    public static readonly BoolToVisibilityVisibleValueConverter Instance = new();

    public override Visibility Convert(
        bool value,
        object? parameter,
        CultureInfo culture)
        => value
            ? Visibility.Visible
            : Visibility.Collapsed;

    public override bool ConvertBack(
        Visibility value,
        object? parameter,
        CultureInfo culture)
        => value == Visibility.Visible;

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