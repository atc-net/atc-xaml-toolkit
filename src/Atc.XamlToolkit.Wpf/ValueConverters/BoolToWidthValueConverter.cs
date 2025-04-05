namespace Atc.XamlToolkit.ValueConverters;

[ValueConversion(typeof(bool), typeof(double))]
public sealed class BoolToWidthValueConverter :
    ValueConverterBase<bool, double>,
    System.Windows.Data.IValueConverter
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
        if (s is null)
        {
            return 0;
        }

        var lengthConverter = new LengthConverter();
        var converted = lengthConverter.ConvertFromString(s);

        return converted is not null &&
               double.TryParse(converted.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0;
    }

    public override bool ConvertBack(
        double value,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");

    object? System.Windows.Data.IValueConverter.Convert(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => ((Data.Converters.IValueConverter)this).Convert(
            value,
            targetType,
            parameter,
            culture);

    object System.Windows.Data.IValueConverter.ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        CultureInfo culture)
        => throw new NotSupportedException("This is a OneWay converter.");
}