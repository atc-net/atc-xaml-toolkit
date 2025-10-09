// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

public sealed class StringNullOrEmptyToBoolValueConverter :
    ValueConverterBase<string, bool>,
    Microsoft.UI.Xaml.Data.IValueConverter
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