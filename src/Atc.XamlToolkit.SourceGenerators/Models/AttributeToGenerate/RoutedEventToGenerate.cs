namespace Atc.XamlToolkit.SourceGenerators.Models.AttributeToGenerate;

internal sealed record RoutedEventToGenerate(
    string OwnerType,
    string Name,
    string? RoutingStrategy,
    string? HandlerTypeName);