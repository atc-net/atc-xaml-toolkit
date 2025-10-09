// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

public sealed class ToLowerValueConverter :
    ValueConverterBase<string, string>,
    Microsoft.UI.Xaml.Data.IValueConverter
{
    public static readonly ToLowerValueConverter Instance = new();

    public override string Convert(
        string value,
        object? parameter,
        CultureInfo culture)
        => string.IsNullOrEmpty(value)
            ? value
            : value.ToLower(culture);

    public override string ConvertBack(
        string value,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");

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