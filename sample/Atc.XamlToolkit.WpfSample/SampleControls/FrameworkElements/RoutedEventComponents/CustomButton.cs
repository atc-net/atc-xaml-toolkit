namespace Atc.XamlToolkit.WpfSample.SampleControls.FrameworkElements.RoutedEventComponents;

/// <summary>
/// Custom button control demonstrating source-generated routed events.
/// </summary>
public partial class CustomButton : Button
{
    public CustomButton()
    {
        Click += OnClick;
    }

    /// <summary>
    /// Simple routed event with default Bubble strategy.
    /// </summary>
    [RoutedEvent]
    private static readonly RoutedEvent itemClicked;

    /// <summary>
    /// Routed event with Tunnel strategy (preview event).
    /// </summary>
    [RoutedEvent(RoutingStrategy.Tunnel)]
    private static readonly RoutedEvent previewItemClicked;

    /// <summary>
    /// Routed event with custom event handler type and args.
    /// </summary>
    [RoutedEvent(HandlerType = typeof(ItemActionRoutedEventHandler))]
    private static readonly RoutedEvent itemActionPerformed;

    private void OnClick(object sender, RoutedEventArgs e)
    {
        // Raise the tunnel (preview) event first - it travels DOWN the visual tree
        RaiseEvent(new RoutedEventArgs(PreviewItemClickedEvent, this));

        // Raise the simple ItemClicked event - it bubbles UP the visual tree
        RaiseEvent(new RoutedEventArgs(ItemClickedEvent, this));

        // Raise the custom ItemActionPerformed event with additional data
        var itemActionArgs = new ItemActionRoutedEventArgs(
            ItemActionPerformedEvent,
            ActionType: "Click",
            Timestamp: DateTime.Now);
        RaiseEvent(itemActionArgs);
    }
}

/// <summary>
/// Custom event handler delegate for item action events.
/// </summary>
public delegate void ItemActionRoutedEventHandler(object sender, ItemActionRoutedEventArgs e);

/// <summary>
/// Custom routed event args containing action details.
/// </summary>
public sealed class ItemActionRoutedEventArgs(
    RoutedEvent routedEvent,
    string ActionType,
    DateTime Timestamp)
    : RoutedEventArgs(routedEvent)
{
    public string ActionType { get; } = ActionType;

    public DateTime Timestamp { get; } = Timestamp;
}
