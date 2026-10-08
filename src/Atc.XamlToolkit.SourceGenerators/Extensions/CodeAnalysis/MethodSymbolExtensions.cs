// ReSharper disable InvertIf
namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class MethodSymbolExtensions
{
    // XML doc elements that only make sense on a method. They are stripped when the
    // method's docs are copied to a generated property: <param> and <typeparam>
    // would raise CS1572 / CS1711 there, and <returns> has no meaning on a property.
    private static readonly string[] MethodOnlyDocumentationElements =
    [
        "param",
        "typeparam",
        "returns",
    ];

    // <paramref>/<typeparamref> would raise CS1734 / CS1735 on the generated property,
    // so they are rewritten to <c>name</c>.
    private static readonly Regex ParameterReferenceRegex = new(
        """<(?:paramref|typeparamref)\s+name\s*=\s*["'](?<name>[^"']*)["']\s*/>""",
        RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant,
        TimeSpan.FromSeconds(1));

    public static List<string>? ExtractDocumentationComments(
        this IMethodSymbol methodSymbol)
    {
        var documentationComments = new List<string>();

        // Extract documentation comments from the syntax tree
        foreach (var syntaxRef in methodSymbol.DeclaringSyntaxReferences)
        {
            if (syntaxRef.GetSyntax() is not MethodDeclarationSyntax methodDeclaration)
            {
                continue;
            }

            // Get leading trivia which contains documentation comments
            var leadingTrivia = methodDeclaration.GetLeadingTrivia();
            foreach (var trivia in leadingTrivia)
            {
                if (trivia.Kind() is SyntaxKind.SingleLineDocumentationCommentTrivia
                    or SyntaxKind.MultiLineDocumentationCommentTrivia)
                {
                    // Get the full text of the documentation comment
                    var commentText = RemoveMethodOnlyDocumentationElements(
                            trivia.ToFullString())
                        .Trim();
                    commentText = ParameterReferenceRegex.Replace(
                        commentText,
                        "<c>${name}</c>");
                    if (!string.IsNullOrWhiteSpace(commentText))
                    {
                        documentationComments.Add(commentText);
                    }
                }
            }
        }

        return documentationComments.Count > 0
            ? documentationComments
            : null;
    }

    /// <summary>
    /// Removes the lines that hold a method-only element (for example <c>&lt;param&gt;</c>),
    /// including the continuation lines of an element that spans several lines.
    /// </summary>
    private static string RemoveMethodOnlyDocumentationElements(
        string commentText)
    {
        var lines = commentText.Split('\n');
        var keptLines = new List<string>(lines.Length);
        string? closingTagToSkipUntil = null;

        foreach (var line in lines)
        {
            if (closingTagToSkipUntil is not null)
            {
                if (line.Contains(closingTagToSkipUntil))
                {
                    closingTagToSkipUntil = null;
                }

                continue;
            }

            var elementName = GetMethodOnlyElementName(line);
            if (elementName is null)
            {
                keptLines.Add(line);
                continue;
            }

            var closingTag = $"</{elementName}>";
            if (!line.Contains(closingTag) &&
                !line.TrimEnd().EndsWith("/>", StringComparison.Ordinal))
            {
                closingTagToSkipUntil = closingTag;
            }
        }

        return string.Join("\n", keptLines);
    }

    private static string? GetMethodOnlyElementName(string line)
    {
        var content = line.TrimStart();
        if (!content.StartsWith("///", StringComparison.Ordinal))
        {
            return null;
        }

        content = content.Substring(3).TrimStart();

        foreach (var elementName in MethodOnlyDocumentationElements)
        {
            var startTag = $"<{elementName}";
            if (content.StartsWith(startTag, StringComparison.Ordinal) &&
                content.Length > startTag.Length &&
                content[startTag.Length] is ' ' or '>' or '/')
            {
                return elementName;
            }
        }

        return null;
    }
}