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
    /// <param name="memberSymbols">The pre-fetched members of the class symbol.</param>
    /// <param name="observableProperties">The list of observable properties that will be generated.</param>
    /// <returns>A list of computed properties with their dependencies.</returns>
    public static List<ComputedPropertyToGenerate> Inspect(
        INamedTypeSymbol classSymbol,
        ImmutableArray<ISymbol> memberSymbols,
        List<ObservablePropertyToGenerate>? observableProperties = null)
    {
        var result = new List<ComputedPropertyToGenerate>();

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
            var dependentProperties = AnalyzePropertyDependencies(propertySymbol, memberSymbols, observableProperties);

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
    /// <param name="memberSymbols">The pre-fetched members of the containing class.</param>
    /// <param name="observableProperties">The list of observable properties that will be generated.</param>
    /// <returns>A collection of property names this property depends on.</returns>
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static ICollection<string> AnalyzePropertyDependencies(
        IPropertySymbol propertySymbol,
        ImmutableArray<ISymbol> memberSymbols,
        List<ObservablePropertyToGenerate>? observableProperties)
    {
        var dependencies = new HashSet<string>(StringComparer.Ordinal);

        var allProperties = new HashSet<string>(StringComparer.Ordinal);
        foreach (var member in memberSymbols)
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
            var identifierNodes = propertySyntax
                .ExpressionBody
                .DescendantNodes()
                .OfType<IdentifierNameSyntax>();

            identifiers.AddRange(identifierNodes.Select(id => id.Identifier.ValueText));
        }
        else
        {
            var getter = propertySyntax
                .AccessorList?
                .Accessors
                .FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));

            if (getter != null)
            {
                var identifierNodes = getter
                    .DescendantNodes()
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

    /// <summary>
    /// Links computed properties to observable properties by adding them to the PropertyNamesToInvalidate list.
    /// </summary>
    /// <param name="observableProperties">The list of observable properties.</param>
    /// <param name="computedProperties">The list of computed properties.</param>
    public static void LinkToObservableProperties(
        List<ObservablePropertyToGenerate>? observableProperties,
        List<ComputedPropertyToGenerate> computedProperties)
    {
        if (observableProperties is null)
        {
            return;
        }

        foreach (var observableProperty in observableProperties)
        {
            foreach (var computedProperty in computedProperties)
            {
                if (computedProperty.DependentPropertyNames.Contains(observableProperty.Name, StringComparer.Ordinal))
                {
                    observableProperty.PropertyNamesToInvalidate ??= [];
                    if (!observableProperty.PropertyNamesToInvalidate.Contains(computedProperty.Name, StringComparer.Ordinal))
                    {
                        observableProperty.PropertyNamesToInvalidate.Add(computedProperty.Name);
                    }
                }
            }
        }
    }
}