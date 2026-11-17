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
    /// Gets or sets a value indicating whether an event in a static class gets
    /// <c>Add{Name}Handler</c> / <c>Remove{Name}Handler</c> methods.
    /// </summary>
    public bool GenerateHandlerMethods { get; set; } = true;

    /// <summary>
    /// Gets or sets the C# modifier of the <c>Raise{Name}</c> method when the attribute
    /// states one; <c>null</c> for the default.
    /// </summary>
    public string? RaiseMethodAccessModifier { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether default summaries are emitted when the
    /// field has no XML doc of its own.
    /// </summary>
    public bool GenerateDocumentation { get; set; }

    /// <summary>
    /// Gets or sets the options that were set on the attribute but have no effect, each as
    /// <c>option|reason</c>. They are reported as diagnostics.
    /// </summary>
    public EquatableArray<string> IgnoredOptions { get; set; } = EquatableArray<string>.Empty;

    /// <summary>
    /// Gets or sets where the diagnostics for <see cref="IgnoredOptions"/> point; only set
    /// when there is something to report.
    /// </summary>
    public DiagnosticLocation? DiagnosticLocation { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the handler is an <c>EventHandler&lt;T&gt;</c>
    /// whose event args do not derive from <c>RoutedEventArgs</c>, so they cannot be raised
    /// with <c>RaiseEvent</c>.
    /// </summary>
    public bool HandlerArgsAreNotRoutedEventArgs { get; set; }

    /// <summary>
    /// Gets the handler type without its namespace and type arguments.
    /// </summary>
    public string HandlerSimpleName
    {
        get
        {
            var handlerTypeName = HandlerTypeName ?? string.Empty;
            var typeArgumentStart = handlerTypeName.IndexOf('<');
            var name = typeArgumentStart > 0
                ? handlerTypeName.Substring(0, typeArgumentStart)
                : handlerTypeName;

            return name.Substring(name.LastIndexOfAny(['.', ':']) + 1);
        }
    }

    /// <summary>
    /// Gets the type arguments of the handler type as written; empty when it has none.
    /// </summary>
    public string HandlerTypeArgument
    {
        get
        {
            var handlerTypeName = HandlerTypeName ?? string.Empty;
            var typeArgumentStart = handlerTypeName.IndexOf('<');
            var typeArgumentEnd = handlerTypeName.LastIndexOf('>');

            return typeArgumentStart > 0 && typeArgumentEnd > typeArgumentStart
                ? handlerTypeName.Substring(typeArgumentStart + 1, typeArgumentEnd - typeArgumentStart - 1)
                : string.Empty;
        }
    }

    /// <summary>
    /// Gets a value indicating whether a <c>Raise{Name}</c> method can be generated: the
    /// owner is not static and the event args of the handler type can be created without
    /// guessing.
    /// </summary>
    public bool CanGenerateRaiseMethod
        => !IsOwnerTypeStatic &&
           !HandlerArgsAreNotRoutedEventArgs &&
           HandlerSimpleName switch
           {
               NameConstants.RoutedEventHandler => HandlerTypeArgument.Length == 0,
               "RoutedPropertyChangedEventHandler" or "EventHandler" => HandlerTypeArgument.Length > 0,
               _ => false,
           };
}