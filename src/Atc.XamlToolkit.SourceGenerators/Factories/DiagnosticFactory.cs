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

    private static readonly DiagnosticDescriptor ObservablePropertyFieldNotPrivateDescriptor =
        new(
            id: "AtcXamlToolkit0003",
            title: "[ObservableProperty] field must be private",
            messageFormat: "Field '{0}' is decorated with [ObservableProperty] but is not declared 'private'. The source generator silently skips non-private fields. Mark the field 'private' so the generated public property pair compiles.",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "[ObservableProperty] generates a public property whose backing field is the decorated field. The pattern requires the backing field to be private — otherwise the public field and the generated public property would collide.");

    private static readonly DiagnosticDescriptor ObservablePropertyFieldNameNotCamelCaseDescriptor =
        new(
            id: "AtcXamlToolkit0004",
            title: "[ObservableProperty] field must be camelCase",
            messageFormat: "Field '{0}' is decorated with [ObservableProperty] but starts with an upper-case letter. The source generator silently skips PascalCase fields. Rename the field to camelCase so the generator can derive the public property name.",
            category: "Usage",
            DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "The [ObservableProperty] generator infers the public property name by taking the field name and upper-casing the first letter. PascalCase fields would collide with the generated property of the same name; the generator therefore skips them.");

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

    public static Diagnostic CreateObservablePropertyFieldNotPrivate(
        string fieldName,
        Location location)
        => Diagnostic.Create(ObservablePropertyFieldNotPrivateDescriptor, location, fieldName);

    public static Diagnostic CreateObservablePropertyFieldNameNotCamelCase(
        string fieldName,
        Location location)
        => Diagnostic.Create(ObservablePropertyFieldNameNotCamelCaseDescriptor, location, fieldName);
}