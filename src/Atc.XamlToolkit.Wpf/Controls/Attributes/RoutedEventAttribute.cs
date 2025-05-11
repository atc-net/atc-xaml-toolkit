// ReSharper disable RedundantAttributeUsageProperty
namespace Atc.XamlToolkit.Controls.Attributes;

[AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
public sealed class RoutedEventAttribute(RoutingStrategy routingStrategy) : Attribute
{
    public RoutedEventAttribute()
        : this(RoutingStrategy.Bubble)
    {
    }

    /// <summary>
    /// The routing strategy (Bubble, Tunnel, or Direct).
    /// </summary>
    public RoutingStrategy RoutingStrategy { get; } = routingStrategy;

    /// <summary>
    /// CLR delegate type for the event.
    /// Defaults to <see cref="RoutedEventHandler"/>.
    /// </summary>
    public Type HandlerType { get; init; } = typeof(RoutedEventHandler);

    public override string ToString()
        => $"{nameof(RoutingStrategy)}: {RoutingStrategy}, " +
           $"{nameof(HandlerType)}: {HandlerType.Name}";
}