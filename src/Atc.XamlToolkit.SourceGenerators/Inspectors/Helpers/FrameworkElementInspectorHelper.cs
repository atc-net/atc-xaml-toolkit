namespace Atc.XamlToolkit.SourceGenerators.Inspectors.Helpers;

[SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
internal static class FrameworkElementInspectorHelper
{
    public static List<T> InspectPropertyAttributes<T>(
        INamedTypeSymbol classSymbol,
        IEnumerable<AttributeData> propertyAttributes)
        where T : BaseFrameworkElementPropertyToGenerate
    {
        var propertiesToGenerate = new List<T>();

        var ownerType = classSymbol.Name;

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

            Extract(
                argumentValues,
                type,
                out var propertyChangedCallback,
                out var coerceValueCallback,
                out var validateValueCallback,
                out var flags,
                out var defaultUpdateSourceTrigger,
                out var isAnimationProhibited,
                out var category,
                out var description,
                ref defaultValue);

            propertiesToGenerate.Add(
                (T)Activator.CreateInstance(
                    typeof(T),
                    ownerType,
                    propertyName,
                    type,
                    defaultValue,
                    propertyChangedCallback,
                    coerceValueCallback,
                    validateValueCallback,
                    flags,
                    defaultUpdateSourceTrigger,
                    isAnimationProhibited,
                    category,
                    description)!);
        }

        return propertiesToGenerate;
    }

    public static T InspectPropertyAttribute<T>(
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

        var type = fieldSymbol.Type.ToString().EnsureCSharpAliasIfNeeded();

        object? defaultValue = null;

        Extract(
            argumentValues,
            type,
            out var propertyChangedCallback,
            out var coerceValueCallback,
            out var validateValueCallback,
            out var flags,
            out var defaultUpdateSourceTrigger,
            out var isAnimationProhibited,
            out var category,
            out var description,
            ref defaultValue);

        var propertyToGenerate = (T)Activator.CreateInstance(
                typeof(T),
                ownerType,
                propertyName,
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

        return propertyToGenerate;
    }

    private static void Extract(
        IDictionary<string, string?> argumentValues,
        string type,
        out string? propertyChangedCallback,
        out string? coerceValueCallback,
        out string? validateValueCallback,
        out string? flags,
        out string? defaultUpdateSourceTrigger,
        out bool? isAnimationProhibited,
        out string? category,
        out string? description,
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

        defaultValue = defaultValue is null && (type.IsSimpleType() || type.IsSimpleUiType())
            ? SimpleTypeFactory.CreateDefaultValueAsStrForType(type)
            : defaultValue?.TransformDefaultValueIfNeeded(type);
    }
}