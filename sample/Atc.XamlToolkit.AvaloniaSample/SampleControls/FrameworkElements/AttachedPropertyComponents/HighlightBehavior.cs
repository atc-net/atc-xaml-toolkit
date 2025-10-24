// ReSharper disable InvertIf
namespace Atc.XamlToolkit.AvaloniaSample.SampleControls.FrameworkElements.AttachedPropertyComponents;

/// <summary>
/// Simple attached property example using field-level attribute with source generator.
/// </summary>
public partial class HighlightBehavior : AvaloniaObject
{
    [AttachedProperty]
    private static bool isHighlightEnabled;

    [AttachedProperty]
    private static IBrush? originalBackground;

    [AttachedProperty]
    private static double originalOpacity;

    static HighlightBehavior()
    {
        IsHighlightEnabledProperty.Changed.AddClassHandler<Control>(OnIsHighlightEnabledChanged);
    }

    private static void OnIsHighlightEnabledChanged(
        AvaloniaObject d,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not Control element)
        {
            return;
        }

        element.PointerEntered -= OnPointerEntered;
        element.PointerExited -= OnPointerExited;

        if (e.NewValue is not true)
        {
            return;
        }

        element.PointerEntered += OnPointerEntered;
        element.PointerExited += OnPointerExited;
    }

    private static void OnPointerEntered(
        object? sender,
        global::Avalonia.Input.PointerEventArgs e)
    {
        if (sender is not Control element)
        {
            return;
        }

        // Store original values
        if (element is global::Avalonia.Controls.Primitives.TemplatedControl templatedControl)
        {
            SetOriginalBackground(element, templatedControl.Background);
            SetOriginalOpacity(element, element.Opacity);

            // Apply highlight effect
            templatedControl.Background = new SolidColorBrush(Colors.LightBlue);
            element.Opacity = 0.7;
        }
    }

    private static void OnPointerExited(
        object? sender,
        global::Avalonia.Input.PointerEventArgs e)
    {
        if (sender is not Control element)
        {
            return;
        }

        // Restore original values
        if (element is global::Avalonia.Controls.Primitives.TemplatedControl templatedControl)
        {
            var orgBackground = GetOriginalBackground(element);
            if (orgBackground is not null)
            {
                templatedControl.Background = orgBackground;
            }

            var orgOpacity = GetOriginalOpacity(element);
            element.Opacity = orgOpacity > 0 ? orgOpacity : 1.0;
        }
    }
}