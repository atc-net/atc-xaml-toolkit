namespace Atc.XamlToolkit.WpfSample.SampleControls.FrameworkElements.AttachedPropertyComponents;

/// <summary>
/// Simple attached property example using class-level attribute.
/// </summary>
[AttachedProperty<string>(
    "Watermark",
    DefaultValue = "",
    PropertyChangedCallback = nameof(OnWatermarkChanged))]
public static partial class WatermarkBehavior
{
    private static void OnWatermarkChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBox textBox)
        {
            return;
        }

        textBox.Loaded -= OnTextBoxLoaded;
        textBox.GotFocus -= OnTextBoxGotFocus;
        textBox.LostFocus -= OnTextBoxLostFocus;
        textBox.TextChanged -= OnTextBoxTextChanged;

        if (string.IsNullOrEmpty(e.NewValue as string))
        {
            return;
        }

        textBox.Loaded += OnTextBoxLoaded;
        textBox.GotFocus += OnTextBoxGotFocus;
        textBox.LostFocus += OnTextBoxLostFocus;
        textBox.TextChanged += OnTextBoxTextChanged;

        UpdateWatermark(textBox);
    }

    private static void OnTextBoxLoaded(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            UpdateWatermark(textBox);
        }
    }

    private static void OnTextBoxGotFocus(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is not TextBox textBox || !string.IsNullOrEmpty(textBox.Tag as string))
        {
            return;
        }

        textBox.Text = string.Empty;
        textBox.Foreground = SystemColors.WindowTextBrush;
    }

    private static void OnTextBoxLostFocus(
        object sender,
        RoutedEventArgs e)
    {
        if (sender is TextBox textBox)
        {
            UpdateWatermark(textBox);
        }
    }

    private static void OnTextBoxTextChanged(
        object sender,
        TextChangedEventArgs e)
    {
        if (sender is TextBox { IsFocused: true } textBox)
        {
            // Mark that the user has entered text
            textBox.Tag = "UserText";
        }
    }

    private static void UpdateWatermark(
        TextBox textBox)
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
            textBox.Foreground = SystemColors.WindowTextBrush;
        }
    }
}