namespace Atc.XamlToolkit.SourceGenerators.Inspectors.Attributes;

internal static class DependencyPropertyInspector<T>
    where T : BaseFrameworkElementPropertyToGenerate
{
    public static List<T> Inspect(
        INamedTypeSymbol classSymbol,
        string primaryAttributeName,
        string secondaryAttributeName)
    {
        var properties = new List<T>();

        properties.AddRange(InspectFromClass(classSymbol, primaryAttributeName, secondaryAttributeName));

        var propertyToGenerates = InspectFromFields(classSymbol, primaryAttributeName, secondaryAttributeName);

        if (properties.Count == 0)
        {
            properties.AddRange(propertyToGenerates);
        }
        else
        {
            foreach (var propertyToGenerate in propertyToGenerates)
            {
                if (properties.Find(x => x.Name == propertyToGenerate.Name) is null)
                {
                    properties.Add(propertyToGenerate);
                }
            }
        }

        return properties;
    }

    private static IEnumerable<T> InspectFromClass(
        INamedTypeSymbol classSymbol,
        string primaryAttributeName,
        string secondaryAttributeName)
    {
        var classAttributes = classSymbol.GetAttributes();
        var propertyAttributes = classAttributes
            .Where(x =>
                x.AttributeClass?.Name == primaryAttributeName ||
                x.AttributeClass?.Name == secondaryAttributeName);

        return FrameworkElementInspectorHelper.InspectPropertyAttributes<T>(
            classSymbol,
            propertyAttributes);
    }

    private static IEnumerable<T> InspectFromFields(
        INamedTypeSymbol classSymbol,
        string primaryAttributeName,
        string secondaryAttributeName)
    {
        var properties = new List<T>();

        foreach (var memberSymbol in classSymbol.GetMembers())
        {
            if (memberSymbol is not IFieldSymbol fieldSymbol)
            {
                continue;
            }

            if (fieldSymbol.DeclaredAccessibility != Accessibility.Private)
            {
                continue;
            }

            if (char.IsUpper(fieldSymbol.Name[0]))
            {
                continue;
            }

            var fieldAttributes = fieldSymbol.GetAttributes();
            var fieldPropertyAttribute = fieldAttributes
                .FirstOrDefault(x =>
                    x.AttributeClass?.Name == primaryAttributeName ||
                    x.AttributeClass?.Name == secondaryAttributeName);

            if (fieldPropertyAttribute is null)
            {
                continue;
            }

            var propertyToGenerate = FrameworkElementInspectorHelper.InspectPropertyAttribute<T>(
                classSymbol,
                fieldSymbol,
                fieldPropertyAttribute);

            properties.Add(propertyToGenerate);
        }

        return properties;
    }
}