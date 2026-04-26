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
    : GenerateBase(NamespaceName, ClassName, ClassAccessModifier, IsStatic);