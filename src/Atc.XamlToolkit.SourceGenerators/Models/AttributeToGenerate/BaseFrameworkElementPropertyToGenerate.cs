namespace Atc.XamlToolkit.SourceGenerators.Models.AttributeToGenerate;

internal abstract class BaseFrameworkElementPropertyToGenerate(
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
{
    public bool IsOwnerTypeStatic { get; } = isOwnerTypeStatic;

    public string OwnerType { get; } = ownerType;

    public string Name { get; } = name;

    public string Type { get; } = type;

    public bool IsReadOnly { get; } = isReadOnly;

    public bool UseNewKeyword { get; } = useNewKeyword;

    public object? DefaultValue { get; } = defaultValue;

    public string? PropertyChangedCallback { get; } = propertyChangedCallback;

    public string? CoerceValueCallback { get; } = coerceValueCallback;

    public string? ValidateValueCallback { get; } = validateValueCallback;

    public string? Flags { get; } = flags;

    public string? DefaultUpdateSourceTrigger { get; } = defaultUpdateSourceTrigger;

    public bool? IsAnimationProhibited { get; } = isAnimationProhibited;

    public string? Category { get; } = category;

    public string? Description { get; } = description;

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

    public override string ToString()
        => $"{nameof(OwnerType)}: {OwnerType}, {nameof(Name)}: {Name}, {nameof(Type)}: {Type}, {nameof(DefaultValue)}: {DefaultValue}, {nameof(PropertyChangedCallback)}: {PropertyChangedCallback}, {nameof(CoerceValueCallback)}: {CoerceValueCallback}, {nameof(ValidateValueCallback)}: {ValidateValueCallback}, {nameof(Flags)}: {Flags}, {nameof(DefaultUpdateSourceTrigger)}: {DefaultUpdateSourceTrigger}, {nameof(IsAnimationProhibited)}: {IsAnimationProhibited}, {nameof(Category)}: {Category}, {nameof(Description)}: {Description}";
}