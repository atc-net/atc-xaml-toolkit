namespace Atc.XamlToolkit.SourceGenerators.Models.AttributeToGenerate;

internal sealed record DependencyPropertyToGenerate(
    bool IsOwnerTypeStatic,
    string OwnerType,
    string Name,
    string Type,
    bool IsReadOnly,
    bool UseNewKeyword,
    object? DefaultValue,
    string? PropertyChangedCallback,
    string? CoerceValueCallback,
    string? ValidateValueCallback,
    string? Flags,
    string? DefaultUpdateSourceTrigger,
    bool? IsAnimationProhibited,
    string? Category,
    string? Description)
    : BaseFrameworkElementPropertyToGenerate(
        IsOwnerTypeStatic,
        OwnerType,
        Name,
        Type,
        IsReadOnly,
        UseNewKeyword,
        DefaultValue,
        PropertyChangedCallback,
        CoerceValueCallback,
        ValidateValueCallback,
        Flags,
        DefaultUpdateSourceTrigger,
        IsAnimationProhibited,
        Category,
        Description);