namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.FrameworkElements.AttachedPropertyComponents;

/// <summary>
/// Simple attached property example using class-level attribute with source generator.
/// </summary>
[AttachedProperty<string>("Watermark", DefaultValue = "")]
public partial class WatermarkBehavior : AvaloniaObject
{
    static WatermarkBehavior()
    {
        WatermarkProperty.Changed.AddClassHandler<TextBox>(OnWatermarkChanged);
    }

    private static void OnWatermarkChanged(
        AvaloniaObject d,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
        {
            return;
        }

        textBox.Loaded -= OnTextBoxLoaded;
        textBox.GotFocus -= OnTextBoxGotFocus;
        textBox.LostFocus -= OnTextBoxLostFocus;
        textBox.PropertyChanged -= OnTextBoxTextChanged;

        if (string.IsNullOrEmpty(e.NewValue as string))
        {
            return;
        }

        textBox.Loaded += OnTextBoxLoaded;
        textBox.GotFocus += OnTextBoxGotFocus;
        textBox.LostFocus += OnTextBoxLostFocus;
        textBox.PropertyChanged += OnTextBoxTextChanged;

        UpdateWatermark(textBox);
    }

    private static void OnTextBoxLoaded(
        object? sender,
        global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            UpdateWatermark(textBox);
        }
    }

    private static void OnTextBoxGotFocus(
        object? sender,
        global::Avalonia.Input.GotFocusEventArgs e)
    {
        if (sender is not TextBox textBox || !string.IsNullOrEmpty(textBox.Tag as string))
        {
            return;
        }

        textBox.Text = string.Empty;
        textBox.Foreground = new SolidColorBrush(Colors.Black);
    }

    private static void OnTextBoxLostFocus(
        object? sender,
        global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            UpdateWatermark(textBox);
        }
    }

    private static void OnTextBoxTextChanged(
        object? sender,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is TextBox textBox && e.Property.Name == nameof(TextBox.Text) && textBox.IsFocused)
        {
            // Mark that the user has entered text
            textBox.Tag = "UserText";
        }
    }

    private static void UpdateWatermark(TextBox textBox)
    {
        var watermark = GetWatermark(textBox);

        if (string.IsNullOrEmpty(watermark))
        {
            return;
        }

        if (string.IsNullOrEmpty(textBox.Text) && !textBox.IsFocused)
        {
            textBox.Text = watermark;
            textBox.Foreground = new SolidColorBrush(Colors.Gray);
            textBox.Tag = null; // Clear user text flag
        }
        else if (textBox.Text == watermark && string.IsNullOrEmpty(textBox.Tag as string))
        {
            textBox.Foreground = new SolidColorBrush(Colors.Gray);
        }
        else
        {
            textBox.Foreground = new SolidColorBrush(Colors.Black);
        }
    }
}