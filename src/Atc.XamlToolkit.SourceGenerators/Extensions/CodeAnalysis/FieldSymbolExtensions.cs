// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class FieldSymbolExtensions
{
    public static bool HasObservablePropertyName(
        this IFieldSymbol fieldSymbol,
        string name)
    {
        var attribute = fieldSymbol
            .GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name
                is NameConstants.ObservablePropertyAttribute
                or NameConstants.ObservableProperty);

        if (attribute is null)
        {
            return false;
        }

        if (fieldSymbol.Name == name)
        {
            return true;
        }

        var argumentValues = attribute.ExtractConstructorArgumentValues();

        return argumentValues.TryGetValue(NameConstants.Name, out var nameValue) &&
               name.Equals(nameValue!.EnsureFirstCharacterToUpper(), StringComparison.Ordinal);
    }

    public static bool HasObservableFieldName(
        this IFieldSymbol fieldSymbol,
        string name)
        => fieldSymbol.Name == name.EnsureFirstCharacterToLower() &&
           fieldSymbol
               .GetAttributes()
               .FirstOrDefault(attr => attr.AttributeClass?.Name
                   is NameConstants.ObservablePropertyAttribute
                   or NameConstants.ObservableProperty) is not null;

    public static List<string>? ExtractCustomAttributes(
        this IFieldSymbol fieldSymbol)
    {
        var customAttributes = new List<string>();

        // Extract attributes from the syntax tree to get the full attribute string
        foreach (var syntaxRef in fieldSymbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is not VariableDeclaratorSyntax
                {
                    Parent: VariableDeclarationSyntax { Parent: FieldDeclarationSyntax fieldDeclaration }
                })
            {
                continue;
            }

            foreach (var attributeList in fieldDeclaration.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name.ToString();

                    // Skip source generator specific attributes
                    if (attributeName is NameConstants.ObservablePropertyAttribute
                        or NameConstants.ObservableProperty
                        or NameConstants.NotifyPropertyChangedForAttribute
                        or NameConstants.NotifyPropertyChangedFor)
                    {
                        continue;
                    }

                    // Get the full attribute string from syntax
                    customAttributes.Add(attribute.ToString());
                }
            }
        }

        return customAttributes.Count > 0
            ? customAttributes
            : null;
    }
}