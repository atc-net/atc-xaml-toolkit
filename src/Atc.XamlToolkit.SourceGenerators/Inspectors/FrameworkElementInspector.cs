namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

internal static class FrameworkElementInspector
{
    public static FrameworkElementInspectorResult Inspect(
        XamlPlatform xamlPlatform,
        INamedTypeSymbol classSymbol)
        => Inspect(xamlPlatform, classSymbol, ImmutableArray<ISymbol>.Empty);

    public static FrameworkElementInspectorResult Inspect(
        XamlPlatform xamlPlatform,
        INamedTypeSymbol classSymbol,
        ImmutableArray<ISymbol> templateMembers)
    {
        // The fields of the property templates the class uses are treated as its own.
        var memberSymbols = templateMembers.AddRange(classSymbol.GetMembers());

        var attachedPropertiesToGenerate = DependencyPropertyInspector<AttachedPropertyToGenerate>.Inspect(
            xamlPlatform,
            classSymbol,
            memberSymbols,
            NameConstants.AttachedPropertyAttribute,
            NameConstants.AttachedProperty);

        // For Avalonia, use StyledPropertyAttribute; for WPF/WinUI, use DependencyPropertyAttribute
        var dependencyPropertyAttributeName = xamlPlatform == XamlPlatform.Avalonia
            ? NameConstants.StyledPropertyAttribute
            : NameConstants.DependencyPropertyAttribute;

        var dependencyPropertyName = xamlPlatform == XamlPlatform.Avalonia
            ? NameConstants.StyledProperty
            : NameConstants.DependencyProperty;

        var dependencyPropertiesToGenerate = DependencyPropertyInspector<DependencyPropertyToGenerate>.Inspect(
            xamlPlatform,
            classSymbol,
            memberSymbols,
            dependencyPropertyAttributeName,
            dependencyPropertyName);

        var routedEventsToGenerate = RoutedEventInspector.Inspect(classSymbol, memberSymbols);

        var relayCommandsToGenerate = RelayCommandInspector.Inspect(classSymbol, memberSymbols);

        return new FrameworkElementInspectorResult(
            classSymbol.IsStatic,
            attachedPropertiesToGenerate,
            dependencyPropertiesToGenerate,
            routedEventsToGenerate,
            relayCommandsToGenerate);
    }
}