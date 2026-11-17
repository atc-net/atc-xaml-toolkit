// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

[ValueConversion(typeof(bool), typeof(double))]
public sealed class BoolToWidthValueConverter :
    ValueConverterBase<bool, double>,
    System.Windows.Data.IValueConverter
{
    public static readonly BoolToWidthValueConverter Instance = new();

    private static readonly LengthConverter LengthConverter = new();

    public override double Convert(
        bool value,
        object? parameter,
        CultureInfo culture)
    {
        if (!value)
        {
            return 0;
        }

        if (parameter is null ||
            parameter.ToString()?.Equals("Auto", StringComparison.OrdinalIgnoreCase) == true)
        {
            return double.NaN;
        }

        // The parameter is written in XAML, so it is read with the invariant culture - not the current one
        var s = System.Convert.ToString(parameter, CultureInfo.InvariantCulture);
        if (s is null)
        {
            return 0;
        }

        return LengthConverter.ConvertFromInvariantString(s) is double result
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