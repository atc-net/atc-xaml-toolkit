namespace Atc.XamlToolkit.SourceGenerators.Extensions;

internal static class FrameworkElementInspectorExtensions
{
    public static void ApplyCommandsAndPropertiesAndEvents(
        this FrameworkElementInspectorResult result,
        List<AttachedPropertyToGenerate> allAttachedProperties,
        List<DependencyPropertyToGenerate> allDependencyProperties,
        List<RoutedEventToGenerate> allRoutedEvents,
        List<RelayCommandToGenerate> allRelayCommands)
    {
        if (allAttachedProperties.Count == 0)
        {
            allAttachedProperties.AddRange(result.AttachedPropertiesToGenerate);
        }
        else
        {
            foreach (var propertyToGenerate in result.AttachedPropertiesToGenerate
                         .Where(p => allAttachedProperties.Find(x => x.Name == p.Name) is null))
            {
                allAttachedProperties.Add(propertyToGenerate);
            }
        }

        if (allDependencyProperties.Count == 0)
        {
            allDependencyProperties.AddRange(result.DependencyPropertiesToGenerate);
        }
        else
        {
            foreach (var propertyToGenerate in result.DependencyPropertiesToGenerate
                         .Where(p => allDependencyProperties.Find(x => x.Name == p.Name) is null))
            {
                allDependencyProperties.Add(propertyToGenerate);
            }
        }

        if (allRoutedEvents.Count == 0)
        {
            allRoutedEvents.AddRange(result.RoutedEventsToGenerate);
        }
        else
        {
            foreach (var eventToGenerate in result.RoutedEventsToGenerate
                         .Where(p => allRoutedEvents.Find(x => x.Name == p.Name) is null))
            {
                allRoutedEvents.Add(eventToGenerate);
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
    }
}