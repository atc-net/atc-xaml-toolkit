namespace Atc.XamlToolkit.SourceGenerators.Factories;

internal static class DiagnosticFactory
{
    public static Diagnostic CreateContainsDuplicateNamesForRelayCommand()
        => Diagnostic.Create(
            new DiagnosticDescriptor(
                id: "AtcXamlToolkit0001",
                title: "Duplicate RelayCommand Name",
                messageFormat: "The RelayCommand is defined multiple times with the same name. To avoid conflicts, specify a unique commandName in the attribute.",
                category: "Hint",
                DiagnosticSeverity.Warning,
                isEnabledByDefault: true),
            Location.None);
}