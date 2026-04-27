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

        var ownerType = classSymbol.Name;

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

            var type = propertyAttribute.ExtractClassFirstArgumentType(ref defaultValue);

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

            propertiesToGenerate.Add(
                BaseFrameworkElementPropertyToGenerate.Create<T>(
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
                    generateDocumentation));
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

        var ownerType = classSymbol.Name;

        var propertyName = fieldSymbol.Name;

        if (propertyName.StartsWith("_", StringComparison.Ordinal))
        {
            propertyName = propertyName.Substring(1);
        }

        propertyName = propertyName.EnsureFirstCharacterToUpper();

        var type = fieldSymbol
            .Type
            .ToDisplayString()
            .EnsureCSharpAliasIfNeeded();

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

        return BaseFrameworkElementPropertyToGenerate.Create<T>(
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
    }

    private static bool HasGenerateDocumentationDefaultAttribute(
        INamedTypeSymbol classSymbol)
    {
        var assemblySymbol = classSymbol.ContainingAssembly;
        if (assemblySymbol is null)
        {
            return false;
        }

        foreach (var attribute in assemblySymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name
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