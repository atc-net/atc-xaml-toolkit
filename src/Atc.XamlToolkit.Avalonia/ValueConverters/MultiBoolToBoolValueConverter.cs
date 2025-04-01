namespace Atc.XamlToolkit.ValueConverters;

public sealed class MultiBoolToBoolValueConverter :
    MultiValueConverterBase<bool, bool>,
    Avalonia.Data.Converters.IMultiValueConverter
{
    public static readonly MultiBoolToBoolValueConverter Instance = new();

    public override bool Convert(
        bool[] values,
        object? parameter,
        CultureInfo culture)
        => values.All(b => b);

    public object? Convert(
        IList<object?> values,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => ((IMultiValueConverter)this).Convert(
            values.ToArray(),
            targetType,
            parameter,
            culture);

    public override object[] ConvertBack(
        bool value,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");
}