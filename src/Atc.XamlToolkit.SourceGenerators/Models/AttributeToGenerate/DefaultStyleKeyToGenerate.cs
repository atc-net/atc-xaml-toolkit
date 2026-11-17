// ReSharper disable CheckNamespace
namespace Atc.XamlToolkit.SourceGenerators.Models.ToGenerate;

/// <summary>
/// What <c>[DefaultStyleKey]</c> on a class asks for.
/// </summary>
/// <param name="StyleTypeName">The type to use as the style key as written in generated code; null means the class itself.</param>
/// <param name="RequiredNamespaces">The namespaces needed to write <paramref name="StyleTypeName"/> by its simple name.</param>
/// <param name="HasOwnStaticConstructor">Whether the class declares a static constructor of its own.</param>
/// <param name="OwnStaticConstructorCallsOverride">Whether that constructor calls the generated <c>OverrideDefaultStyleKey()</c>.</param>
internal sealed record DefaultStyleKeyToGenerate(
    string? StyleTypeName,
    EquatableArray<string> RequiredNamespaces,
    bool HasOwnStaticConstructor,
    bool OwnStaticConstructorCallsOverride);