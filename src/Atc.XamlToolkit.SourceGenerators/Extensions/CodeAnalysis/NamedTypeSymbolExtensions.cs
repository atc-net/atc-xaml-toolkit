// ReSharper disable InvertIf
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class NamedTypeSymbolExtensions
{
    public static List<DtoPropertyInfo> ExtractProperties(
        this INamedTypeSymbol namedTypeSymbol,
        List<string>? ignorePropertyNames = null)
    {
        var ignoreSet = ignorePropertyNames is not null
            ? new HashSet<string>(ignorePropertyNames, StringComparer.Ordinal)
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
                var attributes = ExtractPropertyAttributes(p);
                var documentationComments = ExtractPropertyDocumentationComments(p);

                return new DtoPropertyInfo(
                    p.Name,
                    p.Type.ToString(),
                    isRecordParameter,
                    isReadOnly,
                    attributes,
                    documentationComments);
            })
            .ToList();
    }

    public static List<string> ExtractPropertyAttributes(
        IPropertySymbol propertySymbol)
    {
        var attributes = new List<string>();
        foreach (var syntaxRef in propertySymbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is PropertyDeclarationSyntax propertyDeclaration)
            {
                foreach (var attributeList in propertyDeclaration.AttributeLists)
                {
                    foreach (var attribute in attributeList.Attributes)
                    {
                        attributes.Add(attribute.ToString());
                    }
                }
            }
        }

        return attributes;
    }

    public static List<string>? ExtractPropertyDocumentationComments(
        IPropertySymbol propertySymbol)
    {
        var documentationComments = new List<string>();
        foreach (var syntaxRef in propertySymbol.DeclaringSyntaxReferences)
        {
            var syntax = syntaxRef.GetSyntax();
            if (syntax is PropertyDeclarationSyntax propertyDeclaration)
            {
                // Get leading trivia which contains documentation comments
                var leadingTrivia = propertyDeclaration.GetLeadingTrivia();
                foreach (var trivia in leadingTrivia)
                {
                    if (trivia.Kind() is SyntaxKind.SingleLineDocumentationCommentTrivia
                        or SyntaxKind.MultiLineDocumentationCommentTrivia)
                    {
                        // Get the full text of the documentation comment
                        var commentText = trivia
                            .ToFullString()
                            .Trim();
                        if (!string.IsNullOrWhiteSpace(commentText))
                        {
                            documentationComments.Add(commentText);
                        }
                    }
                }
            }
        }

        return documentationComments.Count > 0
            ? documentationComments
            : null;
    }

    public static List<DtoMethodInfo> ExtractMethods(
        this INamedTypeSymbol namedTypeSymbol,
        List<string>? ignoreMethodNames = null)
    {
        var ignoreSet = ignoreMethodNames is not null
            ? new HashSet<string>(ignoreMethodNames, StringComparer.Ordinal)
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