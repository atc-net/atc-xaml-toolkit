// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.ValueConverters;

public sealed class BoolToWidthValueConverter :
    ValueConverterBase<bool, double>,
    Microsoft.UI.Xaml.Data.IValueConverter
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

        if (parameter is null ||
            parameter.ToString()?.Equals("Auto", StringComparison.OrdinalIgnoreCase) == true)
        {
            return double.NaN;
        }

        var s = parameter.ToString();
        if (s is null)
        {
            return 0;
        }

        // In WinUI, we simply try to parse the value directly
        return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
            ? result
            : 0;
    }

    public override bool ConvertBack(
        double value,
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