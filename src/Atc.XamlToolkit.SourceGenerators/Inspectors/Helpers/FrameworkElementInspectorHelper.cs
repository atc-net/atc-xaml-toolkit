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
            ApplyAddOwner(propertyToGenerate, propertyAttribute, argumentValues, xamlPlatform);
            ApplyReadOnlyKey(propertyToGenerate, argumentValues, xamlPlatform);
            propertyToGenerate.UserPropertyChangedCallback = generatePartialHooks ? userPropertyChangedCallback : null;
            propertyToGenerate.GenerateCoerceHook = generateCoerceHook;
            propertyToGenerate.UserCoerceValueCallback = generateCoerceHook ? userCoerceValueCallback : null;
            propertyToGenerate.RequiredNamespaces = new EquatableArray<string>(requiredNamespaces.ToArray());

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
        propertyToGenerate.GeneratePartialHooks = generatePartialHooks;
        propertyToGenerate.SetterAccessModifier = propertyAttribute.ExtractSetterAccessModifier();
        ApplyAddOwner(propertyToGenerate, propertyAttribute, argumentValues, xamlPlatform);
        ApplyReadOnlyKey(propertyToGenerate, argumentValues, xamlPlatform);
        propertyToGenerate.UserPropertyChangedCallback = generatePartialHooks ? userPropertyChangedCallback : null;
        propertyToGenerate.GenerateCoerceHook = generateCoerceHook;
        propertyToGenerate.UserCoerceValueCallback = generateCoerceHook ? userCoerceValueCallback : null;

        var requiredNamespaces = new SortedSet<string>(StringComparer.Ordinal);
        fieldSymbol.Type.AddRequiredNamespacesTo(requiredNamespaces);
        propertyToGenerate.RequiredNamespaces = new EquatableArray<string>(requiredNamespaces.ToArray());

        var documentationComments = fieldSymbol.ExtractDocumentationComments();
        propertyToGenerate.DocumentationComments = new EquatableArray<string>(documentationComments?.ToArray() ?? []);

        return propertyToGenerate;
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
        propertyToGenerate.HasExplicitDefaultValue = argumentValues.ContainsKey(NameConstants.DefaultValue);
    }

    // IsReadOnly = true registers a read-only dependency property with a key. Only WPF has
    // them. The setter writes through the key and is private unless stated otherwise.
    private static void ApplyReadOnlyKey(
        BaseFrameworkElementPropertyToGenerate propertyToGenerate,
        IDictionary<string, string?> argumentValues,
        XamlPlatform xamlPlatform)
    {
        if (xamlPlatform != XamlPlatform.Wpf ||
            propertyToGenerate.AddOwnerOf is not null ||
            !argumentValues.TryGetValue(NameConstants.IsReadOnly, out var value) ||
            !"true".Equals(value, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        propertyToGenerate.UsesPropertyKey = true;

        if (propertyToGenerate.SetterAccessModifier.Length == 0)
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
            propertyChangedCallback = propertyChangedCallbackValue!.ExtractInnerContent();
        }

        coerceValueCallback = null;
        if (argumentValues.TryGetValue(NameConstants.CoerceValueCallback, out var coerceValueCallbackValue))
        {
            coerceValueCallback = coerceValueCallbackValue!.ExtractInnerContent();
        }

        validateValueCallback = null;
        if (argumentValues.TryGetValue(NameConstants.ValidateValueCallback, out var validateValueCallbackValue))
        {
            validateValueCallback = validateValueCallbackValue!.ExtractInnerContent();
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