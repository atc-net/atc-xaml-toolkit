namespace Atc.XamlToolkit.WpfSample.SampleControls.FrameworkElements.RoutedEventComponents;

/// <summary>
/// Custom routed event args containing value change details.
/// </summary>
/// <param name="routedEvent">The routed event identifier.</param>
/// <param name="oldValue">The value before the change.</param>
/// <param name="newValue">The value after the change.</param>
/// <param name="delta">The difference between the new and old value.</param>
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