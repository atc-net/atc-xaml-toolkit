namespace Atc.XamlToolkit.SourceGenerators.Models.AttributeToGenerate;

internal sealed class AttachedPropertyToGenerate(
    bool isOwnerTypeStatic,
    string ownerType,
    string name,
    string type,
    bool isReadOnly,
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
        isOwnerTypeStatic,
        ownerType,
        name,
        type,
        isReadOnly,
        defaultValue,
        propertyChangedCallback,
        coerceValueCallback,
        validateValueCallback,
        flags,
        defaultUpdateSourceTrigger,
        isAnimationProhibited,
        category,
        description);