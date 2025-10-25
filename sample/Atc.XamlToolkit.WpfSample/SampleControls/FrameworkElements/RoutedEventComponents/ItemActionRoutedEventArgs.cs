namespace Atc.XamlToolkit.WpfSample.SampleControls.FrameworkElements.RoutedEventComponents;

/// <summary>
/// Custom routed event args containing action details.
/// </summary>
public sealed class ItemActionRoutedEventArgs(
    RoutedEvent routedEvent,
    string actionType,
    DateTime timestamp)
    : RoutedEventArgs(routedEvent)
{
    public string ActionType { get; } = actionType;

    public DateTime Timestamp { get; } = timestamp;
}