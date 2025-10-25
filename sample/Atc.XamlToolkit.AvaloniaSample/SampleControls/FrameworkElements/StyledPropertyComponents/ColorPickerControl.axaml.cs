namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.FrameworkElements.StyledPropertyComponents;

/// <summary>
/// Example custom control using field-level [StyledProperty] attributes.
/// Demonstrates how styled properties are generated from private fields in Avalonia.
/// </summary>
public partial class ColorPickerControl : UserControl
{
    public ColorPickerControl()
    {
        InitializeComponent();
    }

    [StyledProperty(DefaultValue = "Colors.Blue", PropertyChangedCallback = nameof(OnColorChanged))]
    private Color selectedColor;

    [StyledProperty(PropertyChangedCallback = nameof(OnColorComponentChanged))]
    private byte red;

    [StyledProperty(PropertyChangedCallback = nameof(OnColorComponentChanged))]
    private byte green;

    [StyledProperty(PropertyChangedCallback = nameof(OnColorComponentChanged))]
    private byte blue;

    private static void OnColorChanged(
        AvaloniaObject d,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not ColorPickerControl control ||
            e.NewValue is null)
        {
            return;
        }

        var color = (Color)e.NewValue;
        control.Red = color.R;
        control.Green = color.G;
        control.Blue = color.B;
    }

    private static void OnColorComponentChanged(
        AvaloniaObject d,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not ColorPickerControl control)
        {
            return;
        }

        var newColor = Color.FromRgb(control.Red, control.Green, control.Blue);
        if (control.SelectedColor != newColor)
        {
            control.SelectedColor = newColor;
        }
    }
}
