namespace Atc.XamlToolkit.SourceGenerators.Models.AttributeToGenerate;

internal abstract record BaseFrameworkElementPropertyToGenerate(
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
{
    public bool HasAnyMetadata
        => DefaultValue is not null ||
           PropertyChangedCallback is not null ||
           CoerceValueCallback is not null ||
           Flags is not null ||
           DefaultUpdateSourceTrigger is not null ||
           IsAnimationProhibited is not null;

    public bool HasAnyValidateValueCallback
        => ValidateValueCallback is not null;

    public static T Create<T>(
        bool isOwnerTypeStatic,
        string ownerType,
        string name,
        string type,
        bool isReadOnly,
        bool useNewKeyword,
        object? defaultValue,
        string? propertyChangedCallback,
        string? coerceValueCallback,
        string? validateValueCallback,
        string? flags,
        string? defaultUpdateSourceTrigger,
        bool? isAnimationProhibited,
        string? category,
        string? description)
        where T : BaseFrameworkElementPropertyToGenerate
    {
        BaseFrameworkElementPropertyToGenerate result;
        if (typeof(T) == typeof(AttachedPropertyToGenerate))
        {
            result = new AttachedPropertyToGenerate(
                isOwnerTypeStatic,
                ownerType,
                name,
                type,
                isReadOnly,
                useNewKeyword,
                defaultValue,
                propertyChangedCallback,
                coerceValueCallback,
                validateValueCallback,
                flags,
                defaultUpdateSourceTrigger,
                isAnimationProhibited,
                category,
                description);
        }
        else
        {
            result = new DependencyPropertyToGenerate(
                isOwnerTypeStatic,
                ownerType,
                name,
                type,
                isReadOnly,
                useNewKeyword,
                defaultValue,
                propertyChangedCallback,
                coerceValueCallback,
                validateValueCallback,
                flags,
                defaultUpdateSourceTrigger,
                isAnimationProhibited,
                category,
                description);
        }

        return (T)result;
    }
}