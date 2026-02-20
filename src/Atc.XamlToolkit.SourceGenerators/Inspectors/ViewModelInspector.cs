namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

internal static class ViewModelInspector
{
    internal static ViewModelInspectorResult Inspect(
        INamedTypeSymbol viewModelClassSymbol,
        bool inheritFromViewModel)
    {
        var memberSymbols = viewModelClassSymbol.GetMembers();

        var propertiesToGenerate = ObservablePropertyInspector.Inspect(
            viewModelClassSymbol,
            memberSymbols,
            inheritFromViewModel);

        var relayCommandsToGenerate = RelayCommandInspector.Inspect(
            viewModelClassSymbol,
            memberSymbols);

        var computedPropertiesToGenerate = ComputedPropertyInspector.Inspect(
            viewModelClassSymbol,
            memberSymbols,
            propertiesToGenerate);

        return new ViewModelInspectorResult(
            propertiesToGenerate,
            relayCommandsToGenerate,
            computedPropertiesToGenerate);
    }
}