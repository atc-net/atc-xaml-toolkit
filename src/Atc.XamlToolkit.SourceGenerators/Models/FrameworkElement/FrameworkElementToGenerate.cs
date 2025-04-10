// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.FrameworkElement;

internal sealed class FrameworkElementToGenerate(
    string namespaceName,
    string className,
    string accessModifier,
    bool isStatic)
    : GenerateBase(
        namespaceName,
        className,
        accessModifier,
        isStatic)
{
    public IList<AttachedPropertyToGenerate>? AttachedPropertiesToGenerate { get; set; }

    public IList<DependencyPropertyToGenerate>? DependencyPropertiesToGenerate { get; set; }

    public IList<RoutedEventToGenerate>? RoutedEventsToGenerate { get; set; }

    public IList<RelayCommandToGenerate>? RelayCommandsToGenerate { get; set; }

    public override string ToString()
        => $"{base.ToString()}, {nameof(AttachedPropertiesToGenerate)}.Count: {AttachedPropertiesToGenerate?.Count}, {nameof(DependencyPropertiesToGenerate)}.Count: {DependencyPropertiesToGenerate?.Count}, {nameof(RoutedEventsToGenerate)}.Count: {RoutedEventsToGenerate?.Count}, {nameof(RelayCommandsToGenerate)}.Count: {RelayCommandsToGenerate?.Count}";
}