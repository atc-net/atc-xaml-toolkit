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
    string? Description,
    bool GenerateDocumentation)
{
    public EquatableArray<string> DocumentationComments { get; set; } = EquatableArray<string>.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether a typed <c>On{Name}Changed</c> partial method is
    /// generated. When true, <see cref="PropertyChangedCallback"/> names the generated static
    /// callback that calls it.
    /// </summary>
    public bool GeneratePartialHooks { get; set; }

    /// <summary>
    /// Gets or sets the callback the user named in the attribute, when
    /// <see cref="GeneratePartialHooks"/> replaced it with the generated one.
    /// </summary>
    public string? UserPropertyChangedCallback { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the type is a <c>Nullable&lt;T&gt;</c> value type,
    /// a type where a trailing '?' is only a nullability annotation, or unknown (<c>null</c>).
    /// </summary>
    public bool? IsNullableValueType { get; set; }

    /// <summary>
    /// Gets or sets the namespaces the generated file must import for the property type,
    /// which is written by its simple name.
    /// </summary>
    public EquatableArray<string> RequiredNamespaces { get; set; } = EquatableArray<string>.Empty;

    /// <summary>
    /// Gets the type as it must be written in a <c>typeof</c> expression or as a type argument:
    /// a nullable value type keeps its '?', a nullable reference type loses it.
    /// </summary>
    public string TypeForTypeOf
        => IsNullableValueType switch
        {
            true => Type,
            false => Type.RemoveNullableSuffix(),
            _ => Type.TrimNullableForTypeOf(),
        };

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
        string? description,
        bool generateDocumentation)
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
                description,
                generateDocumentation);
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
                description,
                generateDocumentation);
        }

        return (T)result;
    }
}