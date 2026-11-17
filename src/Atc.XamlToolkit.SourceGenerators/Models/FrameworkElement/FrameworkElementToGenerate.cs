// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.FrameworkElement;

internal sealed record FrameworkElementToGenerate(
    XamlPlatform XamlPlatform,
    string NamespaceName,
    string ClassName,
    string? ClassAccessModifier,
    bool IsStatic,
    EquatableArray<AttachedPropertyToGenerate> AttachedPropertiesToGenerate,
    EquatableArray<DependencyPropertyToGenerate> DependencyPropertiesToGenerate,
    EquatableArray<RoutedEventToGenerate> RoutedEventsToGenerate,
    EquatableArray<RelayCommandToGenerate> RelayCommandsToGenerate)
    : GenerateBase(NamespaceName, ClassName, ClassAccessModifier, IsStatic)
{
    /// <summary>
    /// Gets or sets what <c>[DefaultStyleKey]</c> on the class asks for; null when the attribute is absent.
    /// </summary>
    public DefaultStyleKeyToGenerate? DefaultStyleKey { get; set; }

    /// <summary>
    /// Gets or sets the fields marked with <c>[TemplatePartField]</c>.
    /// </summary>
    public EquatableArray<TemplatePartToGenerate> TemplatePartsToGenerate { get; set; } = EquatableArray<TemplatePartToGenerate>.Empty;

    /// <summary>
    /// Gets or sets the names of the types given to <c>[UseDependencyPropertyTemplate]</c>
    /// that cannot be applied as a property template.
    /// </summary>
    public EquatableArray<string> UnusablePropertyTemplates { get; set; } = EquatableArray<string>.Empty;

    /// <summary>
    /// Gets a value indicating whether there is any code to emit for the class.
    /// </summary>
    public bool HasAnythingToGenerate
        => AttachedPropertiesToGenerate.Count > 0 ||
           DependencyPropertiesToGenerate.Count > 0 ||
           RoutedEventsToGenerate.Count > 0 ||
           RelayCommandsToGenerate.Count > 0 ||
           DefaultStyleKey is not null ||
           TemplatePartsToGenerate.Count > 0;
}