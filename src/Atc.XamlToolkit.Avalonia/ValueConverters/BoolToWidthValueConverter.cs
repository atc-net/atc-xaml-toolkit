namespace Atc.XamlToolkit.ValueConverters;

public sealed class BoolToWidthValueConverter :
    ValueConverterBase<bool, double>,
    Avalonia.Data.Converters.IValueConverter
{
    public static readonly BoolToWidthValueConverter Instance = new();

    public override double Convert(
        bool value,
        object? parameter,
        CultureInfo culture)
    {
        if (!value)
        {
            return 0;
        }

        if (parameter is null || parameter.ToString()?.Equals("Auto", StringComparison.OrdinalIgnoreCase) == true)
        {
            return double.NaN;
        }

        var s = parameter.ToString();
        return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0;
    }

    public override bool ConvertBack(
        double value,
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

    object Avalonia.Data.Converters.IValueConverter.ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");
}