namespace Atc.XamlToolkit.ValueConverters;

public sealed class BoolToInverseBoolValueConverter :
    ValueConverterBase<bool, bool>,
    Avalonia.Data.Converters.IValueConverter
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