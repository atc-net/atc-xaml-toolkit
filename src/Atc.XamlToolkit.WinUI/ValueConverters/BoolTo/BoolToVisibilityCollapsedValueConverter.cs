// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

public sealed class BoolToVisibilityCollapsedValueConverter :
    ValueConverterBase<bool, Visibility>,
    Microsoft.UI.Xaml.Data.IValueConverter
{
    public static readonly BoolToVisibilityCollapsedValueConverter Instance = new();

    public override Visibility Convert(
        bool value,
        object? parameter,
        CultureInfo culture)
        => value
            ? Visibility.Collapsed
            : Visibility.Visible;

    public override bool ConvertBack(
        Visibility value,
        object? parameter,
        CultureInfo culture)
        => value == Visibility.Collapsed;

    object? Microsoft.UI.Xaml.Data.IValueConverter.Convert(
        object? value,
        Type targetType,
        object? parameter,
        string language)
        => ((IValueConverter)this).Convert(
            value,
            targetType,
            parameter,
            CultureInfo.CurrentCulture);

    object? Microsoft.UI.Xaml.Data.IValueConverter.ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        string language)
        => ((IValueConverter)this).ConvertBack(
            value,
            targetType,
            parameter,
            CultureInfo.CurrentCulture);
}