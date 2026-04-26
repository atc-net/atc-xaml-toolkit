namespace Atc.XamlToolkit.SourceGenerators.Factories;

[SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2008:Enable analyzer release tracking", Justification = "Release tracking files are not used in this project — diagnostic IDs are stable.")]
internal static class DiagnosticFactory
{
    private static readonly DiagnosticDescriptor MissingPartialKeywordDescriptor =
        new(
            id: "AtcXamlToolkit0002",
            title: "View-model class must be partial",
            messageFormat: "Class '{0}' uses [ObservableProperty], [RelayCommand], or [ComputedProperty] but is not declared 'partial'. Add the 'partial' keyword so the source generator can emit the corresponding members.",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The view-model source generator requires every class that contains [ObservableProperty], [RelayCommand], or [ComputedProperty] to be partial. Without the 'partial' keyword the generator silently skips the class and the expected properties / commands never exist.");

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

    public static Diagnostic CreateMissingPartialKeyword(
        string className,
        Location location)
        => Diagnostic.Create(MissingPartialKeywordDescriptor, location, className);
}