namespace Atc.XamlToolkit.SourceGenerators.Models.AttributeToGenerate;

internal sealed class DependencyPropertyToGenerate(
    string ownerType,
    string name,
    string type,
    object? defaultValue,
    string? propertyChangedCallback,
    string? coerceValueCallback,
    string? validateValueCallback,
    string? flags,
    string? defaultUpdateSourceTrigger,
    bool? isAnimationProhibited,
    string? category,
    string? description)
    : BaseFrameworkElementPropertyToGenerate(
        ownerType,
        name,
        type,
        defaultValue,
        propertyChangedCallback,
        coerceValueCallback,
        validateValueCallback,
        flags,
        defaultUpdateSourceTrigger,
        isAnimationProhibited,
        category,
        description);