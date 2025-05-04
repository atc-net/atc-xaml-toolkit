// ReSharper disable once CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

public sealed class StringNullOrEmptyToVisibilityVisibleValueConverter :
    ValueConverterBase<string, bool>,
    Avalonia.Data.Converters.IValueConverter
{
    public static readonly StringNullOrEmptyToVisibilityVisibleValueConverter Instance = new();

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