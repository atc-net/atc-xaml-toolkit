// ReSharper disable InvertIf
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class PropertySymbolExtensions
{
    public static List<string> ExtractPropertyAttributes(
        this IPropertySymbol propertySymbol)
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

    // The copied attributes are written as in the user's file, so their namespaces must be imported.
    public static EquatableArray<string> ExtractPropertyAttributeNamespaces(
        this IPropertySymbol propertySymbol)
    {
        var requiredNamespaces = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var attribute in propertySymbol.GetAttributes())
        {
            attribute.AddRequiredNamespacesTo(requiredNamespaces);
        }

        return new EquatableArray<string>(requiredNamespaces.ToArray());
    }

    public static List<string>? ExtractPropertyDocumentationComments(
        this IPropertySymbol propertySymbol)
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
}