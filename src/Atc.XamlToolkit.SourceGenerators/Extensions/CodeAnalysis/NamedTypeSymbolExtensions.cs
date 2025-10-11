// ReSharper disable InvertIf
namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class NamedTypeSymbolExtensions
{
    public static List<DtoPropertyInfo> ExtractProperties(
        this INamedTypeSymbol namedTypeSymbol)
    {
        var isRecord = namedTypeSymbol.IsRecord;

        // Get primary constructor parameters for records
        var primaryConstructorParameters = new HashSet<string>(StringComparer.Ordinal);
        if (isRecord)
        {
            var primaryConstructor = namedTypeSymbol.Constructors.FirstOrDefault(c =>
                c.Parameters.Length > 0 &&
                c.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is RecordDeclarationSyntax);

            if (primaryConstructor is not null)
            {
                foreach (var parameter in primaryConstructor.Parameters)
                {
                    primaryConstructorParameters.Add(parameter.Name);
                }
            }
        }

        return namedTypeSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public &&
                        p is { IsStatic: false, SetMethod: not null })
            .Select(p =>
            {
                var isRecordParameter = isRecord &&
                                        primaryConstructorParameters.Contains(p.Name);

                return new DtoPropertyInfo(
                    p.Name,
                    p.Type.ToString(),
                    isRecordParameter);
            })
            .ToList();
    }

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

    public static bool HasObservableDtoViewModelAttribute(
        this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol
            .GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name
                is NameConstants.ObservableDtoViewModelAttribute
                or NameConstants.ObservableDtoViewModel) is not null;

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
}