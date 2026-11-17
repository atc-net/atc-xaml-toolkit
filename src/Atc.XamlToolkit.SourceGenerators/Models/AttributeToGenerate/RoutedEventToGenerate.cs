namespace Atc.XamlToolkit.SourceGenerators.Models.AttributeToGenerate;

internal sealed record RoutedEventToGenerate(
    string OwnerType,
    string Name,
    string? RoutingStrategy,
    string? HandlerTypeName)
{
    public EquatableArray<string> DocumentationComments { get; set; } = EquatableArray<string>.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the owner is a static class. It then gets
    /// <c>Add{Name}Handler</c> / <c>Remove{Name}Handler</c> methods instead of a CLR event.
    /// </summary>
    public bool IsOwnerTypeStatic { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the owner is a sealed class.
    /// </summary>
    public bool IsOwnerTypeSealed { get; set; }

    /// <summary>
    /// Gets or sets the type whose existing routed event is shared with <c>AddOwner</c>
    /// instead of registering a new one; <c>null</c> otherwise.
    /// </summary>
    public string? AddOwnerOf { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a <c>Raise{Name}</c> method is generated.
    /// </summary>
    public bool GenerateRaiseMethod { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether default summaries are emitted when the
    /// field has no XML doc of its own.
    /// </summary>
    public bool GenerateDocumentation { get; set; }
}