namespace Atc.XamlToolkit.WinUISample.SampleControls.FrameworkElements.DependencyPropertyComponents;

/// <summary>
/// Example custom control using field-level [DependencyProperty] attributes.
/// Demonstrates how dependency properties are inferred from private fields.
/// </summary>
public sealed partial class ColorPickerControl
{
    public ColorPickerControl()
    {
        InitializeComponent();
    }

    [DependencyProperty(PropertyChangedCallback = nameof(OnColorChanged))]
    private Windows.UI.Color selectedColor;

    [DependencyProperty(PropertyChangedCallback = nameof(OnColorComponentChanged))]
    private byte red;

    [DependencyProperty(PropertyChangedCallback = nameof(OnColorComponentChanged))]
    private byte green;

    [DependencyProperty(PropertyChangedCallback = nameof(OnColorComponentChanged))]
    private byte blue;

    private static void OnColorChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorPickerControl control)
        {
            return;
        }

        var color = (Windows.UI.Color)e.NewValue;
        control.Red = color.R;
        control.Green = color.G;
        control.Blue = color.B;
    }

    private static void OnColorComponentChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not ColorPickerControl control)
        {
            return;
        }

        var newColor = Windows.UI.Color.FromArgb(255, control.Red, control.Green, control.Blue);
        if (control.SelectedColor != newColor)
        {
            control.SelectedColor = newColor;
        }
    }
}