namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.FrameworkElements.StyledPropertyComponents;

public class ColorToBrushConverter : Avalonia.Data.Converters.IValueConverter
{
    public object? Convert(
        object? value,
        Type targetType,
        object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if (value is Color color)
        {
            return new SolidColorBrush(color);
        }

        return Brushes.Transparent;
    }

    public object? ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        System.Globalization.CultureInfo culture)
    {
        if (value is SolidColorBrush brush)
        {
            return brush.Color;
        }

        return Colors.Transparent;
    }
}
