namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class AttributeSyntaxExtensions
{
    /// <summary>
    /// Returns the bare attribute identifier with no generic type-argument suffix.
    /// <c>[Foo]</c> resolves to <c>"Foo"</c>; <c>[Foo&lt;T&gt;]</c> resolves to <c>"Foo"</c>.
    /// Fully-qualified attribute names (<c>[Ns.Foo]</c>) intentionally fall through to
    /// <c>ToString()</c> — the syntax-only predicates that call this method match against
    /// short names, so a qualified shape will not match and is filtered out at the predicate
    /// stage instead. Used by every syntax-only predicate in the source generators to match
    /// attribute names without dragging in the SemanticModel.
    /// </summary>
    /// <param name="attribute">The attribute syntax to inspect.</param>
    /// <returns>The bare identifier (e.g., <c>"ObservableProperty"</c>).</returns>
    public static string GetSimpleAttributeName(this AttributeSyntax attribute)
        => attribute.Name switch
        {
            GenericNameSyntax genericName => genericName.Identifier.Text,
            _ => attribute.Name.ToString(),
        };
}