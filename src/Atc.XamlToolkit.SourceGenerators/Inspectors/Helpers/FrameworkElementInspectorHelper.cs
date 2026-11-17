namespace Atc.XamlToolkit.SourceGenerators.Inspectors.Helpers;

[SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
internal static class FrameworkElementInspectorHelper
{
    public static List<T> InspectPropertyAttributes<T>(
        INamedTypeSymbol classSymbol,
        IEnumerable<AttributeData> propertyAttributes,
        XamlPlatform xamlPlatform)
        where T : BaseFrameworkElementPropertyToGenerate
    {
        var propertiesToGenerate = new List<T>();

        var ownerType = classSymbol.Name + classSymbol.GetTypeParameterList();

        // Project-level default for the GenerateDocumentation flag, opted in via
        // [assembly: GenerateDocumentationDefault]. When present, attributes that
        // don't set the flag explicitly still emit the default summary.
        var generateDocumentationDefault = HasGenerateDocumentationDefaultAttribute(classSymbol);

        foreach (var propertyAttribute in propertyAttributes)
        {
            var argumentValues = propertyAttribute.ExtractConstructorArgumentValues();

            string? propertyName = null;
            if (argumentValues.TryGetValue(NameConstants.Name, out var nameValue))
            {
                propertyName = nameValue!
                    .RemovePrefixFromField()
                    .EnsureFirstCharacterToUpper();
            }

            if (propertyName is null)
            {
                continue;
            }

            object? defaultValue = null;

            var requiredNamespaces = new SortedSet<string>(StringComparer.Ordinal);

            var type = propertyAttribute.ExtractClassFirstArgumentType(ref defaultValue, out var isNullableValueType, requiredNamespaces);

            propertyAttribute.EscapeLiteralArgumentValues(argumentValues, type);

            const bool isReadOnly = false;

            var useNewKeyword = classSymbol.HasBaseTypeThePropertyName(propertyName);

            Extract(
                argumentValues,
                type,
                xamlPlatform,
                generateDocumentationDefault,
                out var propertyChangedCallback,
                out var coerceValueCallback,
                out var validateValueCallback,
                out var flags,
                out var defaultUpdateSourceTrigger,
                out var isAnimationProhibited,
                out var category,
                out var description,
                out var generateDocumentation,
                ref defaultValue);

            defaultValue = GetDefaultValueExpression(propertyAttribute) ?? defaultValue;

            var metadataType = ResolveMetadataType(
                propertyAttribute,
                xamlPlatform,
                ref flags,
                defaultUpdateSourceTrigger,
                isAnimationProhibited,
                out var ignoredMetadataType);

            var userPropertyChangedCallback = propertyChangedCallback;
            var generatePartialHooks = HasGeneratePartialHooks(argumentValues);
            if (generatePartialHooks)
            {
                propertyChangedCallback = GetGeneratedPropertyChangedCallbackName(propertyName);
            }

            var userCoerceValueCallback = coerceValueCallback;
            var generateCoerceHook = HasGenerateCoerceHook(argumentValues, xamlPlatform);
            if (generateCoerceHook)
            {
                coerceValueCallback = GetGeneratedCoerceValueCallbackName(propertyName);
            }

            defaultValue = UseTypeDefaultWhereNullIsInvalid(
                defaultValue,
                propertyAttribute.AttributeClass?.TypeArguments.FirstOrDefault(),
                type,
                propertyChangedCallback ?? coerceValueCallback ?? validateValueCallback ?? flags ?? defaultUpdateSourceTrigger ?? isAnimationProhibited?.ToString());

            var propertyToGenerate = BaseFrameworkElementPropertyToGenerate.Create<T>(
                classSymbol.IsStatic,
                ownerType,
                propertyName,
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

            propertyToGenerate.IsNullableValueType = isNullableValueType;
            propertyToGenerate.GeneratePartialHooks = generatePartialHooks;
            propertyToGenerate.SetterAccessModifier = propertyAttribute.ExtractSetterAccessModifier();
            ApplyMetadataType(propertyToGenerate, metadataType, ignoredMetadataType);
            ApplyAccessOptions(propertyToGenerate, propertyAttribute, argumentValues);
            ApplyAddOwner(propertyToGenerate, propertyAttribute, argumentValues, xamlPlatform);
            ApplyReadOnlyKey(propertyToGenerate, propertyAttribute, argumentValues, xamlPlatform);
            DropSetterModifierThatIsNotMoreRestrictive(propertyToGenerate, propertyAttribute);
            DropMetadataOverrideForOwnerThatIsNotADependencyObject(propertyToGenerate, classSymbol);
            propertyToGenerate.AccessModifier = classSymbol.NarrowAccessModifier(propertyToGenerate.AccessModifier);
            propertyToGenerate.SetterAccessModifier = classSymbol.NarrowAccessModifier(propertyToGenerate.SetterAccessModifier);
            propertyToGenerate.PropertyKeyAccessModifier = classSymbol.NarrowAccessModifier(propertyToGenerate.PropertyKeyAccessModifier);
            propertyToGenerate.UserPropertyChangedCallback = generatePartialHooks ? userPropertyChangedCallback : null;
            propertyToGenerate.GenerateCoerceHook = generateCoerceHook;
            propertyToGenerate.UserCoerceValueCallback = generateCoerceHook ? userCoerceValueCallback : null;
            propertyToGenerate.RequiredNamespaces = new EquatableArray<string>(requiredNamespaces.ToArray());
            propertyToGenerate.DiagnosticLocation = propertyToGenerate.IgnoredOptions.IsEmpty ? null : DiagnosticLocation.From(propertyAttribute);

            propertiesToGenerate.Add(propertyToGenerate);
        }

        return propertiesToGenerate;
    }

    public static T InspectPropertyAttribute<T>(
        XamlPlatform xamlPlatform,
        INamedTypeSymbol classSymbol,
        IFieldSymbol fieldSymbol,
        AttributeData propertyAttribute)
        where T : BaseFrameworkElementPropertyToGenerate
    {
        var argumentValues = propertyAttribute.ExtractConstructorArgumentValues();

        var ownerType = classSymbol.Name + classSymbol.GetTypeParameterList();

        var propertyName = fieldSymbol.Name;

        if (propertyName.StartsWith("_", StringComparison.Ordinal))
        {
            propertyName = propertyName.Substring(1);
        }

        propertyName = propertyName.EnsureFirstCharacterToUpper();

        // PropertyName names the property independently of the field.
        var statedPropertyName = propertyAttribute.ExtractStringArgument(NameConstants.PropertyName);
        var isPropertyNameUsable = statedPropertyName is null || IsUsableIdentifier(statedPropertyName);
        if (statedPropertyName is not null && isPropertyNameUsable)
        {
            propertyName = statedPropertyName;
        }

        var type = fieldSymbol
            .Type
            .ToGeneratedCodeTypeName();

        propertyAttribute.EscapeLiteralArgumentValues(argumentValues, type);

        var useNewKeyword = classSymbol.HasBaseTypeThePropertyName(propertyName);

        object? defaultValue = null;

        var generateDocumentationDefault = HasGenerateDocumentationDefaultAttribute(classSymbol);

        Extract(
            argumentValues,
            type,
            xamlPlatform,
            generateDocumentationDefault,
            out var propertyChangedCallback,
            out var coerceValueCallback,
            out var validateValueCallback,
            out var flags,
            out var defaultUpdateSourceTrigger,
            out var isAnimationProhibited,
            out var category,
            out var description,
            out var generateDocumentation,
            ref defaultValue);

        defaultValue = GetDefaultValueExpression(propertyAttribute) ?? defaultValue;

        var metadataType = ResolveMetadataType(
            propertyAttribute,
            xamlPlatform,
            ref flags,
            defaultUpdateSourceTrigger,
            isAnimationProhibited,
            out var ignoredMetadataType);

        var userPropertyChangedCallback = propertyChangedCallback;
        var generatePartialHooks = HasGeneratePartialHooks(argumentValues);
        if (generatePartialHooks)
        {
            propertyChangedCallback = GetGeneratedPropertyChangedCallbackName(propertyName);
        }

        var userCoerceValueCallback = coerceValueCallback;
        var generateCoerceHook = HasGenerateCoerceHook(argumentValues, xamlPlatform);
        if (generateCoerceHook)
        {
            coerceValueCallback = GetGeneratedCoerceValueCallbackName(propertyName);
        }

        defaultValue = UseTypeDefaultWhereNullIsInvalid(
            defaultValue,
            fieldSymbol.Type,
            type,
            propertyChangedCallback ?? coerceValueCallback ?? validateValueCallback ?? flags ?? defaultUpdateSourceTrigger ?? isAnimationProhibited?.ToString());

        var propertyToGenerate = BaseFrameworkElementPropertyToGenerate.Create<T>(
            classSymbol.IsStatic,
            ownerType,
            propertyName,
            type,
            fieldSymbol.IsReadOnly,
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

        propertyToGenerate.IsNullableValueType = fieldSymbol.Type.IsNullableValueTypeOrUnknown();
        if (!isPropertyNameUsable)
        {
            AddIgnoredOption(propertyToGenerate, NameConstants.PropertyName, "the name is not a valid C# identifier; the name of the field is used");
        }

        propertyToGenerate.GeneratePartialHooks = generatePartialHooks;
        propertyToGenerate.SetterAccessModifier = propertyAttribute.ExtractSetterAccessModifier();
        ApplyMetadataType(propertyToGenerate, metadataType, ignoredMetadataType);
        ApplyAccessOptions(propertyToGenerate, propertyAttribute, argumentValues);
        ApplyAddOwner(propertyToGenerate, propertyAttribute, argumentValues, xamlPlatform);
        ApplyReadOnlyKey(propertyToGenerate, propertyAttribute, argumentValues, xamlPlatform);
        DropSetterModifierThatIsNotMoreRestrictive(propertyToGenerate, propertyAttribute);
        DropMetadataOverrideForOwnerThatIsNotADependencyObject(propertyToGenerate, classSymbol);
        propertyToGenerate.AccessModifier = classSymbol.NarrowAccessModifier(propertyToGenerate.AccessModifier);
        propertyToGenerate.SetterAccessModifier = classSymbol.NarrowAccessModifier(propertyToGenerate.SetterAccessModifier);
        propertyToGenerate.PropertyKeyAccessModifier = classSymbol.NarrowAccessModifier(propertyToGenerate.PropertyKeyAccessModifier);
        propertyToGenerate.UserPropertyChangedCallback = generatePartialHooks ? userPropertyChangedCallback : null;
        propertyToGenerate.GenerateCoerceHook = generateCoerceHook;
        propertyToGenerate.UserCoerceValueCallback = generateCoerceHook ? userCoerceValueCallback : null;

        var requiredNamespaces = new SortedSet<string>(StringComparer.Ordinal);
        fieldSymbol.Type.AddRequiredNamespacesTo(requiredNamespaces);
        propertyToGenerate.RequiredNamespaces = new EquatableArray<string>(requiredNamespaces.ToArray());
        propertyToGenerate.DiagnosticLocation = propertyToGenerate.IgnoredOptions.IsEmpty ? null : DiagnosticLocation.From(propertyAttribute);

        var documentationComments = fieldSymbol.ExtractDocumentationComments();
        propertyToGenerate.DocumentationComments = new EquatableArray<string>(documentationComments?.ToArray() ?? []);

        return propertyToGenerate;
    }

    // DefaultValueExpression is C# source text. It is emitted as it stands, so it is taken
    // after DefaultValue has been fitted to the property type.
    private static string? GetDefaultValueExpression(
        AttributeData propertyAttribute)
    {
        var expression = propertyAttribute.ExtractStringArgument(NameConstants.DefaultValueExpression);
        return string.IsNullOrWhiteSpace(expression)
            ? null
            : expression!.Trim();
    }

    // AccessModifier, GenerateClrAccessors and TargetType decide how the generated members
    // are declared; they do not change the registration.
    private static void ApplyAccessOptions(
        BaseFrameworkElementPropertyToGenerate propertyToGenerate,
        AttributeData propertyAttribute,
        IDictionary<string, string?> argumentValues)
    {
        propertyToGenerate.AccessModifier = propertyAttribute.ExtractAccessModifier(NameConstants.AccessModifier);
        propertyToGenerate.TargetType = propertyAttribute.ExtractTypeOfArgument(NameConstants.TargetType);
        propertyToGenerate.NullElementHandling = propertyAttribute.ExtractEnumArgumentName(NameConstants.NullElementHandling);

        // The second parameter of Set{Name} is called value, so that name is taken.
        var elementParameterName = propertyAttribute.ExtractStringArgument(NameConstants.ElementParameterName);
        if (elementParameterName is not null)
        {
            if (elementParameterName != "value" && IsUsableIdentifier(elementParameterName))
            {
                propertyToGenerate.ElementParameterName = elementParameterName;
            }
            else
            {
                AddIgnoredOption(propertyToGenerate, NameConstants.ElementParameterName, "the name is 'value', which the second parameter has, or not a valid C# identifier; 'element' is used");
            }
        }

        propertyToGenerate.BrowsableForTargetType =
            argumentValues.TryGetValue(NameConstants.BrowsableForTargetType, out var browsable) &&
            "true".Equals(browsable, StringComparison.OrdinalIgnoreCase);

        if (propertyToGenerate is { BrowsableForTargetType: true, TargetType: null })
        {
            AddIgnoredOption(propertyToGenerate, NameConstants.BrowsableForTargetType, "it needs TargetType, which names the type the property is offered on");
        }

        propertyToGenerate.GenerateClrAccessors =
            !argumentValues.TryGetValue(NameConstants.GenerateClrAccessors, out var value) ||
            !"false".Equals(value, StringComparison.OrdinalIgnoreCase);
    }

    // AddOwnerOf = typeof(X) shares the existing property of X instead of registering a new
    // one. Only WPF has AddOwner in this form.
    private static void ApplyAddOwner(
        BaseFrameworkElementPropertyToGenerate propertyToGenerate,
        AttributeData propertyAttribute,
        IDictionary<string, string?> argumentValues,
        XamlPlatform xamlPlatform)
    {
        if (xamlPlatform != XamlPlatform.Wpf)
        {
            return;
        }

        propertyToGenerate.AddOwnerOf = propertyAttribute.ExtractAddOwnerOf();
        propertyToGenerate.HasExplicitDefaultValue =
            argumentValues.ContainsKey(NameConstants.DefaultValue) ||
            GetDefaultValueExpression(propertyAttribute) is not null;

        if (propertyToGenerate.AddOwnerOf is null)
        {
            return;
        }

        if (argumentValues.ContainsKey(NameConstants.ValidateValueCallback))
        {
            AddIgnoredOption(
                propertyToGenerate,
                NameConstants.ValidateValueCallback,
                "validation belongs to the registration of the property that AddOwnerOf shares");
        }

        if (argumentValues.TryGetValue(NameConstants.IsReadOnly, out var isReadOnly) &&
            "true".Equals(isReadOnly, StringComparison.OrdinalIgnoreCase))
        {
            AddIgnoredOption(
                propertyToGenerate,
                NameConstants.IsReadOnly,
                "a property shared with AddOwnerOf is as writable as the shared property");
        }
    }

    // What can reach a member with a given modifier, as a set: 1 = derived types in other
    // assemblies, 2 = other types in this assembly, 4 = derived types in this assembly,
    // 8 = everything else.
    private static int GetAccessibilityDomain(string modifier)
        => modifier switch
        {
            "private" => 0,
            "private protected" => 4,
            "internal" => 2 | 4,
            "protected" => 1 | 4,
            "protected internal" => 1 | 2 | 4,
            _ => 1 | 2 | 4 | 8,
        };

    // C# only accepts an accessor modifier that is more restrictive than the property
    // (CS0273). The Get and Set methods of an attached property are separate methods, so
    // the rule does not apply to them.
    private static void DropSetterModifierThatIsNotMoreRestrictive(
        BaseFrameworkElementPropertyToGenerate propertyToGenerate,
        AttributeData propertyAttribute)
    {
        if (propertyToGenerate is AttachedPropertyToGenerate ||
            propertyToGenerate.IsOwnerTypeStatic ||
            propertyAttribute.ExtractEnumArgumentName(NameConstants.SetterAccessModifier) is null ||
            propertyToGenerate.SetterAccessModifier == propertyToGenerate.AccessModifier)
        {
            return;
        }

        var propertyDomain = GetAccessibilityDomain(propertyToGenerate.AccessModifier);
        var setterDomain = GetAccessibilityDomain(propertyToGenerate.SetterAccessModifier);

        if ((setterDomain & propertyDomain) == setterDomain &&
            setterDomain != propertyDomain)
        {
            return;
        }

        propertyToGenerate.SetterAccessModifier = string.Empty;
        AddIgnoredOption(
            propertyToGenerate,
            NameConstants.SetterAccessModifier,
            "the setter of a property has to be more restrictive than the property, which AccessModifier sets");
    }

    // Metadata that is created with callbacks or flags takes a default value, and null is
    // not a value of a value type: registering it throws when the class is first used.
    private static object? UseTypeDefaultWhereNullIsInvalid(
        object? defaultValue,
        ITypeSymbol? typeSymbol,
        string type,
        string? anyOtherMetadata)
        => defaultValue is null &&
           anyOtherMetadata is not null &&
           typeSymbol is { IsValueType: true, TypeKind: not TypeKind.Error } &&
           typeSymbol.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T
            ? $"default({type})"
            : defaultValue;

    private static bool IsUsableIdentifier(string name)
        => SyntaxFacts.IsValidIdentifier(name) &&
           SyntaxFacts.GetKeywordKind(name) == SyntaxKind.None;

    // AddOwner with metadata is an override of the metadata, which WPF only allows for an
    // owner that derives from DependencyObject; for any other owner it throws when the class
    // is first used. A static class, or a class without a base class, is not one.
    private static void DropMetadataOverrideForOwnerThatIsNotADependencyObject(
        BaseFrameworkElementPropertyToGenerate propertyToGenerate,
        INamedTypeSymbol classSymbol)
    {
        if (propertyToGenerate.AddOwnerOf is null ||
            !(classSymbol.IsStatic || classSymbol.BaseType?.SpecialType == SpecialType.System_Object))
        {
            return;
        }

        var hasOverride =
            propertyToGenerate.HasExplicitDefaultValue ||
            !string.IsNullOrEmpty(propertyToGenerate.PropertyChangedCallback) ||
            !string.IsNullOrEmpty(propertyToGenerate.CoerceValueCallback) ||
            !string.IsNullOrEmpty(propertyToGenerate.Flags) ||
            !string.IsNullOrEmpty(propertyToGenerate.DefaultUpdateSourceTrigger) ||
            propertyToGenerate.IsAnimationProhibited is not null;

        if (!hasOverride)
        {
            return;
        }

        propertyToGenerate.DropsMetadataOverride = true;
        propertyToGenerate.GeneratePartialHooks = false;
        propertyToGenerate.GenerateCoerceHook = false;

        AddIgnoredOption(
            propertyToGenerate,
            NameConstants.AddOwnerOf,
            "the default value, callbacks and metadata options are left out, because metadata can only be overridden for an owner that derives from DependencyObject");
    }

    private static void AddIgnoredOption(
        BaseFrameworkElementPropertyToGenerate propertyToGenerate,
        string option,
        string reason)
        => propertyToGenerate.IgnoredOptions = new EquatableArray<string>(
            propertyToGenerate.IgnoredOptions.Append($"{option}|{reason}").ToArray());

    // MetadataType asks for a metadata class. FrameworkPropertyMetadata is what the generator
    // picks as soon as a flag is set, so it is expressed as the flag None. PropertyMetadata
    // and UIPropertyMetadata are passed on, unless an option is set that they cannot carry.
    private static string? ResolveMetadataType(
        AttributeData propertyAttribute,
        XamlPlatform xamlPlatform,
        ref string? flags,
        string? defaultUpdateSourceTrigger,
        bool? isAnimationProhibited,
        out string? ignoredBecause)
    {
        ignoredBecause = null;

        if (xamlPlatform != XamlPlatform.Wpf)
        {
            return null;
        }

        var metadataType = propertyAttribute.ExtractEnumArgumentName(NameConstants.MetadataType);
        switch (metadataType)
        {
            case NameConstants.FrameworkPropertyMetadata:
                if (string.IsNullOrEmpty(flags))
                {
                    flags = "FrameworkPropertyMetadataOptions.None";
                }

                return null;
            case NameConstants.PropertyMetadata:
            case NameConstants.UIPropertyMetadata:
                if (!string.IsNullOrEmpty(flags))
                {
                    ignoredBecause = "Flags needs FrameworkPropertyMetadata";
                }
                else if (!string.IsNullOrEmpty(defaultUpdateSourceTrigger))
                {
                    ignoredBecause = "DefaultUpdateSourceTrigger needs FrameworkPropertyMetadata";
                }
                else if (isAnimationProhibited is not null &&
                         metadataType == NameConstants.PropertyMetadata)
                {
                    ignoredBecause = "IsAnimationProhibited needs UIPropertyMetadata or FrameworkPropertyMetadata";
                }

                return ignoredBecause is null ? metadataType : null;
            default:
                return null;
        }
    }

    private static void ApplyMetadataType(
        BaseFrameworkElementPropertyToGenerate propertyToGenerate,
        string? metadataType,
        string? ignoredBecause)
    {
        propertyToGenerate.MetadataType = metadataType;

        if (ignoredBecause is not null)
        {
            AddIgnoredOption(propertyToGenerate, NameConstants.MetadataType, ignoredBecause);
        }
    }

    // IsReadOnly = true registers a read-only dependency property with a key. Only WPF has
    // them. The setter writes through the key and is private unless stated otherwise.
    private static void ApplyReadOnlyKey(
        BaseFrameworkElementPropertyToGenerate propertyToGenerate,
        AttributeData propertyAttribute,
        IDictionary<string, string?> argumentValues,
        XamlPlatform xamlPlatform)
    {
        if (xamlPlatform != XamlPlatform.Wpf ||
            propertyToGenerate.AddOwnerOf is not null ||
            !argumentValues.TryGetValue(NameConstants.IsReadOnly, out var value) ||
            !"true".Equals(value, StringComparison.OrdinalIgnoreCase))
        {
            if (xamlPlatform == XamlPlatform.Wpf &&
                propertyAttribute.ExtractEnumArgumentName(NameConstants.PropertyKeyAccessModifier) is not null)
            {
                AddIgnoredOption(propertyToGenerate, NameConstants.PropertyKeyAccessModifier, "only a read-only property (IsReadOnly = true, without AddOwnerOf) has a key");
            }

            return;
        }

        // A static class is not a DependencyObject, so a read-only property cannot be
        // registered for it other than as an attached property.
        if (propertyToGenerate is not AttachedPropertyToGenerate &&
            propertyToGenerate.IsOwnerTypeStatic)
        {
            AddIgnoredOption(propertyToGenerate, NameConstants.IsReadOnly, "a read-only property in a static class has to be an [AttachedProperty]");
            return;
        }

        propertyToGenerate.UsesPropertyKey = true;

        // Public reads as an empty modifier, so look at whether the option was stated at all.
        if (propertyAttribute.ExtractEnumArgumentName(NameConstants.PropertyKeyAccessModifier) is not null)
        {
            var keyModifier = propertyAttribute.ExtractAccessModifier(NameConstants.PropertyKeyAccessModifier);
            propertyToGenerate.PropertyKeyAccessModifier = keyModifier.Length == 0 ? "public" : keyModifier;
        }

        // An explicit SetterAccessModifier = Public reads as "no modifier" too, so look at
        // whether the option was stated at all.
        if (propertyAttribute.ExtractEnumArgumentName(NameConstants.SetterAccessModifier) is null)
        {
            propertyToGenerate.SetterAccessModifier = "private";
        }
    }

    // Only WPF has a coerce callback.
    private static bool HasGenerateCoerceHook(
        IDictionary<string, string?> argumentValues,
        XamlPlatform xamlPlatform)
        => xamlPlatform == XamlPlatform.Wpf &&
           argumentValues.TryGetValue(NameConstants.GenerateCoerceHook, out var value) &&
           "true".Equals(value, StringComparison.OrdinalIgnoreCase);

    // The generated static callback that is registered with the property system and
    // calls the typed Coerce{Name} partial method.
    private static string GetGeneratedCoerceValueCallbackName(
        string propertyName)
        => $"Coerce{propertyName}PropertyCallback";

    private static bool HasGeneratePartialHooks(
        IDictionary<string, string?> argumentValues)
        => argumentValues.TryGetValue(NameConstants.GeneratePartialHooks, out var value) &&
           "true".Equals(value, StringComparison.OrdinalIgnoreCase);

    // The generated static callback that is registered with the property system and
    // calls the typed On{Name}Changed partial method.
    private static string GetGeneratedPropertyChangedCallbackName(
        string propertyName)
        => $"On{propertyName}PropertyChangedCallback";

    internal static bool HasGenerateDocumentationDefaultAttribute(
        INamedTypeSymbol classSymbol)
    {
        var assemblySymbol = classSymbol.ContainingAssembly;
        if (assemblySymbol is null)
        {
            return false;
        }

        foreach (var attribute in assemblySymbol.GetAttributes())
        {
            if (attribute.GetToolkitAttributeName()
                is NameConstants.GenerateDocumentationDefault
                or NameConstants.GenerateDocumentationDefaultAttribute)
            {
                return true;
            }
        }

        return false;
    }

    private static void Extract(
        IDictionary<string, string?> argumentValues,
        string type,
        XamlPlatform xamlPlatform,
        bool generateDocumentationDefault,
        out string? propertyChangedCallback,
        out string? coerceValueCallback,
        out string? validateValueCallback,
        out string? flags,
        out string? defaultUpdateSourceTrigger,
        out bool? isAnimationProhibited,
        out string? category,
        out string? description,
        out bool generateDocumentation,
        ref object? defaultValue)
    {
        if (argumentValues.TryGetValue(NameConstants.DefaultValue, out var defaultValueValue))
        {
            defaultValue = defaultValueValue;
        }

        propertyChangedCallback = null;
        if (argumentValues.TryGetValue(NameConstants.PropertyChangedCallback, out var propertyChangedCallbackValue))
        {
            // The value is null when the attribute says '= null'
            propertyChangedCallback = propertyChangedCallbackValue?.ExtractInnerContent();
        }

        coerceValueCallback = null;
        if (argumentValues.TryGetValue(NameConstants.CoerceValueCallback, out var coerceValueCallbackValue))
        {
            // The value is null when the attribute says '= null'
            coerceValueCallback = coerceValueCallbackValue?.ExtractInnerContent();
        }

        validateValueCallback = null;
        if (argumentValues.TryGetValue(NameConstants.ValidateValueCallback, out var validateValueCallbackValue))
        {
            // The value is null when the attribute says '= null'
            validateValueCallback = validateValueCallbackValue?.ExtractInnerContent();
        }

        flags = null;
        if (argumentValues.TryGetValue(NameConstants.Flags, out var flagsValue))
        {
            flags = flagsValue;
        }

        defaultUpdateSourceTrigger = null;
        if (argumentValues.TryGetValue(NameConstants.DefaultUpdateSourceTrigger, out var defaultUpdateSourceTriggerValue))
        {
            defaultUpdateSourceTrigger = defaultUpdateSourceTriggerValue;
        }

        isAnimationProhibited = null;
        if (argumentValues.TryGetValue(NameConstants.IsAnimationProhibited, out var isAnimationProhibitedValue))
        {
            isAnimationProhibited = isAnimationProhibitedValue!.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        category = null;
        if (argumentValues.TryGetValue(NameConstants.Category, out var categoryValue))
        {
            category = categoryValue;
        }

        description = null;
        if (argumentValues.TryGetValue(NameConstants.Description, out var descriptionValue))
        {
            description = descriptionValue;
        }

        // Three-state resolution mirrors ObservablePropertyInspector / RelayCommandInspector:
        // explicit per-attribute value wins; otherwise fall back to assembly-level
        // [GenerateDocumentationDefault].
        if (argumentValues.TryGetValue(NameConstants.GenerateDocumentation, out var generateDocumentationValue) &&
            bool.TryParse(generateDocumentationValue, out var generateDocumentationValueAsBool))
        {
            generateDocumentation = generateDocumentationValueAsBool;
        }
        else
        {
            generateDocumentation = generateDocumentationDefault;
        }

        defaultValue = defaultValue is null && type.IsKnownValueType()
            ? SimpleTypeFactory.CreateDefaultValueAsStrForType(type, xamlPlatform)
            : defaultValue?.TransformDefaultValueIfNeeded(type, xamlPlatform);
    }
}