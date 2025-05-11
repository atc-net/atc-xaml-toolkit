namespace Atc.XamlToolkit.SourceGenerators.Models.AttributeToGenerate;

internal sealed class RoutedEventToGenerate(
    string ownerType,
    string name,
    string routingStrategy,
    string handlerTypeName)
{
    public string OwnerType { get; } = ownerType;

    public string Name { get; } = name;

    public string? RoutingStrategy { get; } = routingStrategy;

    public string? HandlerTypeName { get; } = handlerTypeName;

    public override string ToString()
        => $"{nameof(OwnerType)}: {OwnerType}, {nameof(Name)}: {Name}, {nameof(RoutingStrategy)}: {RoutingStrategy}, {nameof(HandlerTypeName)}: {HandlerTypeName}";
}