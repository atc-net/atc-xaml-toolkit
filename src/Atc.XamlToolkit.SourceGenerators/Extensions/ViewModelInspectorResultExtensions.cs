namespace Atc.XamlToolkit.SourceGenerators.Extensions;

internal static class ViewModelInspectorResultExtensions
{
    public static void ApplyCommandsAndProperties(
        this ViewModelInspectorResult result,
        List<ObservablePropertyToGenerate> allObservableProperties,
        List<RelayCommandToGenerate> allRelayCommands,
        List<ComputedPropertyToGenerate> allComputedProperties)
    {
        if (allObservableProperties.Count == 0)
        {
            allObservableProperties.AddRange(result.PropertiesToGenerate);
        }
        else
        {
            foreach (var propertyToGenerate in result.PropertiesToGenerate
                         .Where(p => allObservableProperties.Find(x => x.BackingFieldName == p.BackingFieldName) is null))
            {
                allObservableProperties.Add(propertyToGenerate);
            }
        }

        if (allRelayCommands.Count == 0)
        {
            allRelayCommands.AddRange(result.RelayCommandsToGenerate);
        }
        else
        {
            foreach (var relayCommandToGenerate in result.RelayCommandsToGenerate
                         .Where(rc => allRelayCommands.Find(x => x.CommandName == rc.CommandName) is null))
            {
                allRelayCommands.Add(relayCommandToGenerate);
            }
        }

        if (allComputedProperties.Count == 0)
        {
            allComputedProperties.AddRange(result.ComputedPropertiesToGenerate);
        }
        else
        {
            foreach (var computedPropertyToGenerate in result.ComputedPropertiesToGenerate
                         .Where(cp => allComputedProperties.Find(x => x.Name == cp.Name) is null))
            {
                allComputedProperties.Add(computedPropertyToGenerate);
            }
        }
    }
}