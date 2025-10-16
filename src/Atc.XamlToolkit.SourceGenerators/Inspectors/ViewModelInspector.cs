namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

internal static class ViewModelInspector
{
    internal static ViewModelInspectorResult Inspect(
        INamedTypeSymbol viewModelClassSymbol,
        bool inheritFromViewModel)
    {
        var propertiesToGenerate = ObservablePropertyInspector.Inspect(
            viewModelClassSymbol,
            inheritFromViewModel);

        var relayCommandsToGenerate = RelayCommandInspector.Inspect(
            viewModelClassSymbol);

        var computedPropertiesToGenerate = ComputedPropertyInspector.Inspect(
            viewModelClassSymbol,
            propertiesToGenerate);

        return new ViewModelInspectorResult(
            propertiesToGenerate,
            relayCommandsToGenerate,
            computedPropertiesToGenerate);
    }
}