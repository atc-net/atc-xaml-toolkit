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
    /// Gets or sets a value indicating whether a typed <c>Coerce{Name}</c> partial method is
    /// generated. When true, <see cref="CoerceValueCallback"/> names the generated static
    /// callback that calls it.
    /// </summary>
    public bool GenerateCoerceHook { get; set; }

    /// <summary>
    /// Gets or sets the coerce callback the user named in the attribute, when
    /// <see cref="GenerateCoerceHook"/> replaced it with the generated one.
    /// </summary>
    public string? UserCoerceValueCallback { get; set; }

    /// <summary>
    /// Gets or sets the C# modifier of the generated members, e.g. <c>internal</c>; empty
    /// for public.
    /// </summary>
    public string AccessModifier { get; set; } = string.Empty;

    /// <summary>
    /// Gets the C# modifier of the generated identifier field, CLR property and
    /// <c>Get{Name}</c> method.
    /// </summary>
    public string MemberAccessModifier
        => AccessModifier.Length == 0 ? "public" : AccessModifier;

    /// <summary>
    /// Gets the C# modifier of the generated <c>Set{Name}</c> method.
    /// </summary>
    public string SetMethodAccessModifier
        => SetterAccessModifier.Length == 0 ? MemberAccessModifier : SetterAccessModifier;

    /// <summary>
    /// Gets the setter of the generated CLR property with its modifier. An accessor modifier
    /// must be more restrictive than the property, so it is left out when it is the same.
    /// </summary>
    public string SetterKeyword
        => SetterAccessModifier.Length == 0 || SetterAccessModifier == AccessModifier
            ? "set"
            : $"{SetterAccessModifier} set";

    /// <summary>
    /// Gets or sets a value indicating whether the CLR property, or the accessor methods of
    /// an attached property, are generated.
    /// </summary>
    public bool GenerateClrAccessors { get; set; } = true;

    /// <summary>
    /// Gets or sets the type of the element parameter of the accessor methods of an attached
    /// property; <c>null</c> for the platform's default.
    /// </summary>
    public string? TargetType { get; set; }

    /// <summary>
    /// Gets or sets how the accessor methods of an attached property treat a null element:
    /// the name of the <c>NullElementHandling</c> member, or <c>null</c> for the default.
    /// </summary>
    public string? NullElementHandling { get; set; }

    /// <summary>
    /// Gets or sets the name of the element parameter of the accessor methods of an
    /// attached property.
    /// </summary>
    public string ElementParameterName { get; set; } = "element";

    /// <summary>
    /// Gets or sets the metadata class asked for with <c>MetadataType</c> when it is
    /// <c>PropertyMetadata</c> or <c>UIPropertyMetadata</c>; <c>null</c> when the generator
    /// chooses the class.
    /// </summary>
    public string? MetadataType { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the <c>Get{Name}</c> method of an attached
    /// property gets <c>[AttachedPropertyBrowsableForType]</c> for <see cref="TargetType"/>.
    /// </summary>
    public bool BrowsableForTargetType { get; set; }

    /// <summary>
    /// Gets or sets the options that were set on the attribute but have no effect, each as
    /// <c>option|reason</c>. They are reported as diagnostics.
    /// </summary>
    public EquatableArray<string> IgnoredOptions { get; set; } = EquatableArray<string>.Empty;

    /// <summary>
    /// Gets or sets the type whose existing dependency property is shared with
    /// <c>AddOwner</c> instead of registering a new one (WPF only); <c>null</c> otherwise.
    /// </summary>
    public string? AddOwnerOf { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the attribute states a default value, as
    /// opposed to <see cref="DefaultValue"/> being derived from the property type.
    /// </summary>
    public bool HasExplicitDefaultValue { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the property is registered as a read-only
    /// dependency property with a <c>DependencyPropertyKey</c> (WPF only).
    /// </summary>
    public bool UsesPropertyKey { get; set; }

    /// <summary>
    /// Gets the field the generated code writes through: the key of a read-only dependency
    /// property, otherwise the property identifier itself.
    /// </summary>
    public string WritablePropertyIdentifier
        => UsesPropertyKey ? $"{Name}PropertyKey" : $"{Name}Property";

    /// <summary>
    /// Gets or sets the C# modifier of the property's setter, e.g. <c>private</c>;
    /// empty for a setter that is as accessible as the property.
    /// </summary>
    public string SetterAccessModifier { get; set; } = string.Empty;

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