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
}