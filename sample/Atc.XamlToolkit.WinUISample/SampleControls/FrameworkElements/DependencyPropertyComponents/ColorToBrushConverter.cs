namespace Atc.XamlToolkit.WinUISample.SampleControls.FrameworkElements.DependencyPropertyComponents;

public sealed class ColorToBrushConverter : Microsoft.UI.Xaml.Data.IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        string language)
    {
        if (value is Color color)
        {
            return new SolidColorBrush(color);
        }

        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        string language)
    {
        if (value is SolidColorBrush brush)
        {
            return brush.Color;
        }

        return Colors.Transparent;
    }
}