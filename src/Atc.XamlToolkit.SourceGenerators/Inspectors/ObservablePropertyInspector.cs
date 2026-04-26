// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

internal static class ObservablePropertyInspector
{
    public static List<ObservablePropertyToGenerate> Inspect(
        INamedTypeSymbol classSymbol,
        ImmutableArray<ISymbol> memberSymbols,
        bool inheritFromViewModel)
    {
        var result = new List<ObservablePropertyToGenerate>();

        // Project-level default for the GenerateDocumentation flag, opted in via
        // [assembly: GenerateDocumentationDefault]. When present, fields that
        // don't set the flag explicitly still emit the default summary.
        var generateDocumentationDefault = HasGenerateDocumentationDefaultAttribute(classSymbol);

        foreach (var memberSymbol in memberSymbols)
        {
            if (memberSymbol is not IFieldSymbol fieldSymbol)
            {
                continue;
            }

            if (fieldSymbol.DeclaredAccessibility != Accessibility.Private)
            {
                continue;
            }

            if (char.IsUpper(fieldSymbol.Name[0]))
            {
                continue;
            }

            var fieldSymbolAttributes = fieldSymbol.GetAttributes();

            var fieldPropertyAttribute = fieldSymbolAttributes
                .FirstOrDefault(x => x.AttributeClass?.Name
                    is NameConstants.ObservablePropertyAttribute
                    or NameConstants.ObservableProperty);

            if (fieldPropertyAttribute is null)
            {
                continue;
            }

            AppendPropertyToGenerate(
                fieldSymbol,
                fieldSymbolAttributes,
                fieldPropertyAttribute,
                inheritFromViewModel,
                generateDocumentationDefault,
                result);
        }

        return result;
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

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static void AppendPropertyToGenerate(
        IFieldSymbol fieldSymbol,
        ImmutableArray<AttributeData> fieldSymbolAttributes,
        AttributeData fieldPropertyAttribute,
        bool inheritFromViewModel,
        bool generateDocumentationDefault,
        List<ObservablePropertyToGenerate> propertiesToGenerate)
    {
        var backingFieldName = fieldSymbol.Name;
        var propertyType = fieldSymbol.Type.ToString();

        var fieldArgumentValues = fieldPropertyAttribute.ExtractConstructorArgumentValues();

        var propertyName = fieldArgumentValues.TryGetValue(NameConstants.Name, out var nameValue)
            ? nameValue!
                .RemovePrefixFromField()
                .EnsureFirstCharacterToUpper()
            : backingFieldName
                .RemovePrefixFromField()
                .EnsureFirstCharacterToUpper();

        List<string>? propertyNamesToInvalidate = null;
        if (fieldArgumentValues.TryGetValue(NameConstants.DependentPropertyNames, out var dependentPropertiesValue))
        {
            propertyNamesToInvalidate = [];

            propertyNamesToInvalidate.AddRange(
                dependentPropertiesValue!
                    .Split(',')
                    .Select(x => x
                        .Trim()
                        .ExtractInnerContent()));
        }
        else
        {
            foreach (var argumentValue in fieldArgumentValues)
            {
                if (argumentValue.Key
                    is NameConstants.Name
                    or NameConstants.DependentCommandNames
                    or NameConstants.AfterChangedCallback
                    or NameConstants.BeforeChangedCallback
                    or NameConstants.BroadcastOnChange
                    or NameConstants.UseIsDirty
                    or NameConstants.IsRequired
                    or NameConstants.GeneratePartialHooks
                    or NameConstants.GenerateDocumentation)
                {
                    continue;
                }

                propertyNamesToInvalidate ??= [];
                propertyNamesToInvalidate.Add(argumentValue.Value!.ExtractInnerContent());
            }
        }

        string[]? commandNamesToInvalidate = null;
        if (fieldArgumentValues.TryGetValue(NameConstants.DependentCommandNames, out var dependentCommandsValue))
        {
            commandNamesToInvalidate = dependentCommandsValue?
                .Split(',')
                .Select(x => x.Trim())
                .ToArray();
        }

        string? beforeChangedCallback = null;
        if (fieldArgumentValues.TryGetValue(NameConstants.BeforeChangedCallback, out var beforeChangedCallbackValue))
        {
            beforeChangedCallback = beforeChangedCallbackValue;
        }

        string? afterChangedCallback = null;
        if (fieldArgumentValues.TryGetValue(NameConstants.AfterChangedCallback, out var afterChangedCallbackValue))
        {
            afterChangedCallback = afterChangedCallbackValue;
        }

        var broadcastOnChange = fieldArgumentValues.TryGetValue(NameConstants.BroadcastOnChange, out var broadcastOnChangeValue) &&
                                "true".Equals(broadcastOnChangeValue, StringComparison.OrdinalIgnoreCase);

        var useIsDirty = fieldPropertyAttribute.ExtractUseIsDirtyValue(inheritFromViewModel, defaultValue: false);

        var generatePartialHooks = fieldArgumentValues.TryGetValue(NameConstants.GeneratePartialHooks, out var generatePartialHooksValue) &&
                                   "true".Equals(generatePartialHooksValue, StringComparison.OrdinalIgnoreCase);

        // Resolve GenerateDocumentation with three-state semantics:
        //   - explicit per-property `= true`  → on
        //   - explicit per-property `= false` → off (overrides assembly default)
        //   - not specified                   → fall back to assembly-level default
        bool generateDocumentation;
        if (fieldArgumentValues.TryGetValue(NameConstants.GenerateDocumentation, out var generateDocumentationValue))
        {
            generateDocumentation = "true".Equals(generateDocumentationValue, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            generateDocumentation = generateDocumentationDefault;
        }

        var isRequired = fieldArgumentValues.TryGetValue(NameConstants.IsRequired, out var isRequiredValue) &&
                         "true".Equals(isRequiredValue, StringComparison.OrdinalIgnoreCase);

        foreach (var attr in fieldSymbolAttributes)
        {
            if (attr.AttributeClass?.Name
                is not NameConstants.NotifyPropertyChangedForAttribute
                and not NameConstants.NotifyPropertyChangedFor)
            {
                continue;
            }

            propertyNamesToInvalidate ??= [];

            var argumentValues = attr.ExtractConstructorArgumentValues();
            foreach (var argumentValue in argumentValues)
            {
                if (!propertyNamesToInvalidate.Contains(argumentValue.Value!, StringComparer.Ordinal))
                {
                    propertyNamesToInvalidate.Add(argumentValue.Value!);
                }
            }
        }

        // [NotifyCanExecuteChangedFor("FooCommand")] — companion attribute that
        // augments the existing DependentCommandNames list.
        List<string>? extraCommandNames = null;
        foreach (var attr in fieldSymbolAttributes)
        {
            if (attr.AttributeClass?.Name
                is not NameConstants.NotifyCanExecuteChangedForAttribute
                and not NameConstants.NotifyCanExecuteChangedFor)
            {
                continue;
            }

            extraCommandNames ??= [];

            var argumentValues = attr.ExtractConstructorArgumentValues();
            foreach (var argumentValue in argumentValues)
            {
                if (!extraCommandNames.Contains(argumentValue.Value!, StringComparer.Ordinal))
                {
                    extraCommandNames.Add(argumentValue.Value!);
                }
            }
        }

        if (extraCommandNames is not null)
        {
            commandNamesToInvalidate = commandNamesToInvalidate is null
                ? extraCommandNames.ToArray()
                : commandNamesToInvalidate.Concat(extraCommandNames).Distinct(StringComparer.Ordinal).ToArray();
        }

        // [NotifyDataErrorInfo] — companion attribute that opts the setter
        // into inline validation. The generator emits a ValidateProperty(...)
        // call after the field assignment in the setter.
        var validatesOnChange = false;
        foreach (var attr in fieldSymbolAttributes)
        {
            if (attr.AttributeClass?.Name
                is NameConstants.NotifyDataErrorInfoAttribute
                or NameConstants.NotifyDataErrorInfo)
            {
                validatesOnChange = true;
                break;
            }
        }

        var customAttributes = fieldSymbol.ExtractCustomAttributes();
        var documentationComments = fieldSymbol.ExtractDocumentationComments();

        propertiesToGenerate.Add(
            new ObservablePropertyToGenerate(
                propertyName,
                propertyType,
                backingFieldName,
                fieldSymbol.IsReadOnly)
            {
                PropertyNamesToInvalidate = new EquatableArray<string>(propertyNamesToInvalidate?.ToArray() ?? []),
                CommandNamesToInvalidate = new EquatableArray<string>(commandNamesToInvalidate ?? []),
                BeforeChangedCallback = beforeChangedCallback,
                AfterChangedCallback = afterChangedCallback,
                BroadcastOnChange = broadcastOnChange,
                UseIsDirty = useIsDirty,
                IsRequired = isRequired,
                GeneratePartialHooks = generatePartialHooks,
                GenerateDocumentation = generateDocumentation,
                ValidatesOnChange = validatesOnChange,
                CustomAttributes = new EquatableArray<string>(customAttributes?.ToArray() ?? []),
                DocumentationComments = new EquatableArray<string>(documentationComments?.ToArray() ?? []),
            });
    }
}