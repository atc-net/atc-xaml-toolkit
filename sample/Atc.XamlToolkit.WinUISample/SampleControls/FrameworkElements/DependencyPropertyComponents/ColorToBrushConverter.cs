namespace Atc.XamlToolkit.WinUISample.SampleControls.FrameworkElements.DependencyPropertyComponents;

public class ColorToBrushConverter : Microsoft.UI.Xaml.Data.IValueConverter
{
    public object Convert(
        object? value,
        Type targetType,
        object? parameter,
        string language)
    {
        if (value is Windows.UI.Color color)
        {
            return new Microsoft.UI.Xaml.Media.SolidColorBrush(color);
        }

        return new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
    }

    public object ConvertBack(
        object? value,
        Type targetType,
        object? parameter,
        string language)
    {
        if (value is Microsoft.UI.Xaml.Media.SolidColorBrush brush)
        {
            return brush.Color;
        }

        return Microsoft.UI.Colors.Transparent;
    }
}
