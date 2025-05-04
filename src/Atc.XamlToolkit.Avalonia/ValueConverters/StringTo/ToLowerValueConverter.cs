// ReSharper disable once CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

public sealed class ToLowerValueConverter :
    ValueConverterBase<string, string>,
    Avalonia.Data.Converters.IValueConverter
{
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

    object? Avalonia.Data.Converters.IValueConverter.Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => ((IValueConverter)this).Convert(
            value,
            targetType,
            parameter,
            culture);

    object? Avalonia.Data.Converters.IValueConverter.ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => ((IValueConverter)this).ConvertBack(
            value,
            targetType,
            parameter,
            culture);
}