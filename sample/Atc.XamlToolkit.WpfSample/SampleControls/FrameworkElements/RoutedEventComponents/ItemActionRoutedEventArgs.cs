namespace Atc.XamlToolkit.WpfSample.SampleControls.FrameworkElements.RoutedEventComponents;

/// <summary>
/// Custom routed event args containing action details.
/// </summary>
/// <param name="routedEvent">The routed event identifier.</param>
/// <param name="actionType">The type of action performed.</param>
/// <param name="timestamp">The time the action occurred.</param>
public sealed class ItemActionRoutedEventArgs(
    RoutedEvent routedEvent,
    string actionType,
    DateTime timestamp)
    : RoutedEventArgs(routedEvent)
{
    public string ActionType { get; } = actionType;

    public DateTime Timestamp { get; } = timestamp;
}