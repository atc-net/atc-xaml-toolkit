namespace Atc.XamlToolkit.WpfSample.SampleControls.FrameworkElements.RoutedEventComponents;

/// <summary>
/// Custom routed event args containing value change details.
/// </summary>
public sealed class ValueChangedRoutedEventArgs(
    RoutedEvent routedEvent,
    int oldValue,
    int newValue,
    int delta)
    : RoutedEventArgs(routedEvent)
{
    public int OldValue { get; } = oldValue;

    public int NewValue { get; } = newValue;

    public int Delta { get; } = delta;
}