// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

public sealed class BoolToInverseBoolValueConverter :
    ValueConverterBase<bool, bool>,
    Microsoft.UI.Xaml.Data.IValueConverter
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