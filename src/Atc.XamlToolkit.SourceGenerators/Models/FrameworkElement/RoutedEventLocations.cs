namespace Atc.XamlToolkit.SourceGenerators.Models.FrameworkElement;

internal sealed record RoutedEventLocations(
    EquatableArray<(string FieldName, Location Location)> Locations);