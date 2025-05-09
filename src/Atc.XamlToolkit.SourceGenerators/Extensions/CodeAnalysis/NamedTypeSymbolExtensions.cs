namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class NamedTypeSymbolExtensions
{
    public static string GetAccessModifier(
        this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            _ => string.Empty,
        };

    public static bool InheritsFrom(
        this INamedTypeSymbol namedTypeSymbol,
        params string[] baseClassNames)
    {
        var baseType = namedTypeSymbol.BaseType;
        while (baseType is not null)
        {
            if (baseClassNames.Contains(baseType.Name, StringComparer.Ordinal))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    public static bool HasBaseTypeThePropertyName(
        this INamedTypeSymbol declaringType,
        string propertyName)
    {
        var pascalName = propertyName.EnsureFirstCharacterToUpper();
        for (var baseType = declaringType.BaseType;
             baseType is not null;
             baseType = baseType.BaseType)
        {
            if (baseType
                .GetMembers(pascalName)
                .Any(m => m.Kind == SymbolKind.Property))
            {
                return true;
            }
        }

        return false;
    }
}