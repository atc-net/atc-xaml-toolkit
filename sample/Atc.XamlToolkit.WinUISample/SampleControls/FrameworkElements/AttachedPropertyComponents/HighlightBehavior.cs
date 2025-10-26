namespace Atc.XamlToolkit.WinUISample.SampleControls.FrameworkElements.AttachedPropertyComponents;

/// <summary>
/// Simple attached property example using field-level attribute.
/// </summary>
public static partial class HighlightBehavior
{
    [AttachedProperty(PropertyChangedCallback = nameof(OnIsHighlightEnabledChanged))]
    private static bool isHighlightEnabled;

    [AttachedProperty(DefaultValue = "new SolidColorBrush(Colors.AliceBlue)")]
    private static Brush? originalBackground;

    [AttachedProperty]
    private static double originalOpacity;

    private static void OnIsHighlightEnabledChanged(
        DependencyObject d,
        DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement element)
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
        object sender,
        PointerRoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        // Store original values
        var orgBackground = GetBackgroundBrush(element);
        SetOriginalBackground(element, orgBackground);
        SetOriginalOpacity(element, element.Opacity);

        switch (element)
        {
            // Apply highlight effect based on element type
            case Border or Panel:
                // For Border/Panel, change background directly
                SetBackgroundBrush(element, new SolidColorBrush(Colors.LightBlue));
                break;
            case Control:
                // For controls like Button, reduce opacity to create visible effect
                element.Opacity = 0.3;
                break;
        }
    }

    private static void OnPointerExited(
        object sender,
        PointerRoutedEventArgs e)
    {
        if (sender is not FrameworkElement element)
        {
            return;
        }

        // Restore original values
        var orgBackground = GetOriginalBackground(element);
        if (element is Border or Panel && orgBackground is not null)
        {
            SetBackgroundBrush(element, orgBackground);
        }

        // Restore original opacity
        var orgOpacity = GetOriginalOpacity(element);
        element.Opacity = orgOpacity > 0
            ? orgOpacity
            : 1.0; // Default if not set
    }

    private static Brush GetBackgroundBrush(
        FrameworkElement element)
        => element switch
        {
            Control control => control.Background,
            Panel panel => panel.Background,
            Border border => border.Background,
            _ => new SolidColorBrush(Colors.Transparent),
        };

    private static void SetBackgroundBrush(
        FrameworkElement element,
        Brush brush)
    {
        switch (element)
        {
            case Control control:
                control.Background = brush;
                break;
            case Panel panel:
                panel.Background = brush;
                break;
            case Border border:
                border.Background = brush;
                break;
        }
    }
}