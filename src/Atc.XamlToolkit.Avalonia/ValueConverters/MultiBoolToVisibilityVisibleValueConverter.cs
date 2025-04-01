namespace Atc.XamlToolkit.ValueConverters;

public sealed class MultiBoolToVisibilityVisibleValueConverter :
    MultiValueConverterBase<bool, bool>,
    Avalonia.Data.Converters.IMultiValueConverter
{
    public static readonly MultiBoolToVisibilityVisibleValueConverter Instance = new();

    public override bool Convert(
        bool[] values,
        object? parameter,
        CultureInfo culture)
        => values.All(b => b);

    public override object?[] ConvertBack(
        bool value,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");

    object? Avalonia.Data.Converters.IMultiValueConverter.Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => ((IMultiValueConverter)this).Convert(
            values.ToArray(),
            targetType,
            parameter,
            culture);
}