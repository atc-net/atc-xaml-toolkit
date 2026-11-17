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

    /// <summary>
    /// Gets the type whose existing routed event of the same name is shared, instead of
    /// registering a new one. The generator emits
    /// <c>{Type}.{Name}Event.AddOwner(typeof(Owner))</c>. <see cref="HandlerType"/> must be
    /// the handler type of the shared event; the routing strategy is that of the shared event.
    /// </summary>
    public Type? AddOwnerOf { get; init; }

    /// <summary>
    /// Gets a value indicating whether a method that raises the event is generated. The
    /// method depends on <see cref="HandlerType"/>:
    /// <c>Raise{Name}()</c> for <see cref="RoutedEventHandler"/>,
    /// <c>Raise{Name}(T oldValue, T newValue)</c> for <c>RoutedPropertyChangedEventHandler&lt;T&gt;</c>, and
    /// <c>Raise{Name}(TArgs e)</c> for <c>EventHandler&lt;TArgs&gt;</c>.
    /// The method is <c>protected</c>, or <c>private</c> in a sealed class. No method is
    /// generated for any other handler type, or in a static class.
    /// </summary>
    public bool GenerateRaiseMethod { get; init; }

    /// <summary>
    /// Gets the accessibility of the generated <c>Raise{Name}</c> method. When it is not
    /// stated the method is <c>protected</c>, or <c>private</c> in a sealed class.
    /// </summary>
    public PropertyAccessModifier RaiseMethodAccessModifier { get; init; }

    /// <summary>
    /// Gets a value indicating whether to emit default <c>/// &lt;summary&gt;</c> XML
    /// comments on the generated members when the field has no XML doc of its own.
    /// Defaults to <c>false</c>; can be flipped per-assembly via
    /// <c>[assembly: GenerateDocumentationDefault]</c>.
    /// </summary>
    public bool GenerateDocumentation { get; init; }

    public override string ToString()
        => $"{nameof(RoutingStrategy)}: {RoutingStrategy}, " +
           $"{nameof(HandlerType)}: {HandlerType.Name}";
}