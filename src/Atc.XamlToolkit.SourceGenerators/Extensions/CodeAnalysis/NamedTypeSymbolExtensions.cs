// ReSharper disable InvertIf
namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class NamedTypeSymbolExtensions
{
    public static List<DtoPropertyInfo> ExtractProperties(
        this INamedTypeSymbol namedTypeSymbol,
        List<string>? ignoreProperties = null)
    {
        var ignoreSet = ignoreProperties is not null
            ? new HashSet<string>(ignoreProperties, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);

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
                        !p.IsStatic &&
                        !ignoreSet.Contains(p.Name))
            .Select(p =>
            {
                var isRecordParameter = isRecord &&
                                        primaryConstructorParameters.Contains(p.Name);

                var isReadOnly = p.SetMethod is null;

                return new DtoPropertyInfo(
                    p.Name,
                    p.Type.ToString(),
                    isRecordParameter,
                    isReadOnly);
            })
            .ToList();
    }

    public static List<DtoMethodInfo> ExtractMethods(
        this INamedTypeSymbol namedTypeSymbol,
        List<string>? ignoreMethods = null)
    {
        var ignoreSet = ignoreMethods is not null
            ? new HashSet<string>(ignoreMethods, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);

        return namedTypeSymbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public &&
                        m is { IsStatic: false, MethodKind: MethodKind.Ordinary, IsImplicitlyDeclared: false } &&
                        m.Name != "ToString" &&
                        !ignoreSet.Contains(m.Name))
            .Select(m =>
            {
                var parameters = m.Parameters
                    .Select(p => new DtoMethodParameterInfo(
                        p.Name,
                        p.Type.ToString()))
                    .ToList();

                return new DtoMethodInfo(
                    m.Name,
                    m.ReturnType.ToString(),
                    parameters);
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