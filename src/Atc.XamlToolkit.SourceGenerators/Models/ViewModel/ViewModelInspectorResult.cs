namespace Atc.XamlToolkit.SourceGenerators.Models.ViewModel;

internal sealed class ViewModelInspectorResult(
    List<ObservablePropertyToGenerate> propertiesToGenerate,
    List<RelayCommandToGenerate> relayCommandsToGenerate,
    List<ComputedPropertyToGenerate> computedPropertiesToGenerate)
{
    public List<ObservablePropertyToGenerate> PropertiesToGenerate { get; } = propertiesToGenerate;

    public List<RelayCommandToGenerate> RelayCommandsToGenerate { get; } = relayCommandsToGenerate;

    public List<ComputedPropertyToGenerate> ComputedPropertiesToGenerate { get; } = computedPropertiesToGenerate;

    public bool FoundAnythingToGenerate
        => PropertiesToGenerate.Count > 0 ||
           RelayCommandsToGenerate.Count > 0;

    public override string ToString()
        => $"{nameof(PropertiesToGenerate)}.Count: {PropertiesToGenerate.Count}, {nameof(RelayCommandsToGenerate)}.Count: {RelayCommandsToGenerate.Count}, {nameof(ComputedPropertiesToGenerate)}.Count: {ComputedPropertiesToGenerate.Count}, {nameof(FoundAnythingToGenerate)}: {FoundAnythingToGenerate}";
}