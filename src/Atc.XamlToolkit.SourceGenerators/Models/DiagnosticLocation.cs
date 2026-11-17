namespace Atc.XamlToolkit.SourceGenerators.Models;

/// <summary>
/// Where a diagnostic points, as plain values. A pipeline model must not hold a
/// <see cref="Location"/>, which keeps the syntax tree and the compilation alive.
/// </summary>
/// <param name="FilePath">The path of the source file.</param>
/// <param name="Start">The start of the span in the file.</param>
/// <param name="Length">The length of the span.</param>
/// <param name="StartLine">The zero-based line the span starts on.</param>
/// <param name="StartCharacter">The zero-based character the span starts at.</param>
/// <param name="EndLine">The zero-based line the span ends on.</param>
/// <param name="EndCharacter">The zero-based character the span ends at.</param>
internal sealed record DiagnosticLocation(
    string FilePath,
    int Start,
    int Length,
    int StartLine,
    int StartCharacter,
    int EndLine,
    int EndCharacter)
{
    public static DiagnosticLocation? From(AttributeData attributeData)
    {
        if (attributeData.ApplicationSyntaxReference is not { } syntaxReference)
        {
            return null;
        }

        var lineSpan = syntaxReference.SyntaxTree.GetLineSpan(syntaxReference.Span);

        return new DiagnosticLocation(
            syntaxReference.SyntaxTree.FilePath,
            syntaxReference.Span.Start,
            syntaxReference.Span.Length,
            lineSpan.StartLinePosition.Line,
            lineSpan.StartLinePosition.Character,
            lineSpan.EndLinePosition.Line,
            lineSpan.EndLinePosition.Character);
    }

    public Location ToLocation()
        => Location.Create(
            FilePath,
            new TextSpan(Start, Length),
            new LinePositionSpan(
                new LinePosition(StartLine, StartCharacter),
                new LinePosition(EndLine, EndCharacter)));
}