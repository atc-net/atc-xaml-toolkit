namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

/// <summary>
/// Inspector for identifying computed properties and their dependencies.
/// </summary>
internal static class ComputedPropertyInspector
{
    /// <summary>
    /// Inspects a class symbol for properties marked with [ComputedProperty] attribute
    /// and determines which properties they depend on.
    /// </summary>
    /// <param name="classSymbol">The class symbol to inspect.</param>
    /// <param name="observableProperties">The list of observable properties that will be generated.</param>
    /// <returns>A list of computed properties with their dependencies.</returns>
    public static List<ComputedPropertyToGenerate> Inspect(
        INamedTypeSymbol classSymbol,
        List<ObservablePropertyToGenerate>? observableProperties = null)
    {
        var result = new List<ComputedPropertyToGenerate>();

        var memberSymbols = classSymbol.GetMembers();

        foreach (var memberSymbol in memberSymbols)
        {
            if (memberSymbol is not IPropertySymbol propertySymbol)
            {
                continue;
            }

            var propertyAttributes = propertySymbol.GetAttributes();

            var computedPropertyAttribute = propertyAttributes
                .FirstOrDefault(x => x.AttributeClass?.Name
                    is NameConstants.ComputedPropertyAttribute
                    or NameConstants.ComputedProperty);

            if (computedPropertyAttribute is null)
            {
                continue;
            }

            // Analyze the property to find dependencies
            var dependentProperties = AnalyzePropertyDependencies(propertySymbol, classSymbol, observableProperties);

            if (dependentProperties.Count > 0)
            {
                result.Add(new ComputedPropertyToGenerate(
                    propertySymbol.Name,
                    dependentProperties));
            }
        }

        return result;
    }

    /// <summary>
    /// Analyzes a property to find which other properties it depends on.
    /// </summary>
    /// <param name="propertySymbol">The property to analyze.</param>
    /// <param name="classSymbol">The containing class symbol.</param>
    /// <param name="observableProperties">The list of observable properties that will be generated.</param>
    /// <returns>A collection of property names this property depends on.</returns>
    private static ICollection<string> AnalyzePropertyDependencies(
        IPropertySymbol propertySymbol,
        INamedTypeSymbol classSymbol,
        List<ObservablePropertyToGenerate>? observableProperties)
    {
        var dependencies = new HashSet<string>(StringComparer.Ordinal);

        var allProperties = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in classSymbol.GetMembers())
        {
            if (member is IPropertySymbol prop && prop.Name != propertySymbol.Name)
            {
                allProperties.Add(prop.Name);
            }
        }

        if (observableProperties is not null)
        {
            foreach (var observableProperty in observableProperties)
            {
                if (observableProperty.Name != propertySymbol.Name)
                {
                    allProperties.Add(observableProperty.Name);
                }
            }
        }

        if (propertySymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is not PropertyDeclarationSyntax propertySyntax)
        {
            return dependencies;
        }

        var identifiers = new List<string>();

        if (propertySyntax.ExpressionBody is not null)
        {
            var identifierNodes = propertySyntax.ExpressionBody.DescendantNodes()
                .OfType<IdentifierNameSyntax>();

            identifiers.AddRange(identifierNodes.Select(id => id.Identifier.ValueText));
        }
        else if (propertySyntax.AccessorList is not null)
        {
            var getter = propertySyntax.AccessorList.Accessors
                .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));

            if (getter is not null)
            {
                var identifierNodes = getter.DescendantNodes()
                    .OfType<IdentifierNameSyntax>();

                identifiers.AddRange(identifierNodes.Select(id => id.Identifier.ValueText));
            }
        }

        foreach (var identifier in identifiers)
        {
            if (allProperties.Contains(identifier))
            {
                dependencies.Add(identifier);
            }
        }

        return dependencies;
    }
}