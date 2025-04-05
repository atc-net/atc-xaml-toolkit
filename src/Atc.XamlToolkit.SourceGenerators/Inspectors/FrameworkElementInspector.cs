namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

internal static class FrameworkElementInspector
{
    public static FrameworkElementInspectorResult Inspect(
        INamedTypeSymbol classSymbol)
    {
        var attachedPropertiesToGenerate = DependencyPropertyInspector<AttachedPropertyToGenerate>.Inspect(
            classSymbol,
            NameConstants.AttachedPropertyAttribute,
            NameConstants.AttachedProperty);

        var dependencyPropertiesToGenerate = DependencyPropertyInspector<DependencyPropertyToGenerate>.Inspect(
            classSymbol,
            NameConstants.DependencyPropertyAttribute,
            NameConstants.DependencyProperty);

        var relayCommandsToGenerate = RelayCommandInspector.Inspect(classSymbol);

        return new FrameworkElementInspectorResult(
            classSymbol.IsStatic,
            attachedPropertiesToGenerate,
            dependencyPropertiesToGenerate,
            relayCommandsToGenerate);
    }
}