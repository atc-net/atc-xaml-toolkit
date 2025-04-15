// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.XamlToolkit.SourceGenerators.Inspectors.Attributes;

internal static class ObservablePropertyInspector
{
    public static List<ObservablePropertyToGenerate> Inspect(
        INamedTypeSymbol classSymbol)
    {
        var result = new List<ObservablePropertyToGenerate>();

        var memberSymbols = classSymbol.GetMembers();

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
                result);
        }

        return result;
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static void AppendPropertyToGenerate(
        IFieldSymbol fieldSymbol,
        ImmutableArray<AttributeData> fieldSymbolAttributes,
        AttributeData fieldPropertyAttribute,
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
        if (fieldArgumentValues.TryGetValue(NameConstants.DependentProperties, out var dependentPropertiesValue))
        {
            propertyNamesToInvalidate = [];

            propertyNamesToInvalidate.AddRange(
                dependentPropertiesValue!
                    .Split(',')
                    .Select(x => x.Trim().ExtractInnerContent()));
        }
        else
        {
            foreach (var argumentValue in fieldArgumentValues)
            {
                if (argumentValue.Key
                    is NameConstants.Name
                    or NameConstants.DependentCommands
                    or NameConstants.AfterChangedCallback
                    or NameConstants.BeforeChangedCallback
                    or NameConstants.BroadcastOnChange)
                {
                    continue;
                }

                propertyNamesToInvalidate ??= [];
                propertyNamesToInvalidate.Add(argumentValue.Value!.ExtractInnerContent());
            }
        }

        string[]? commandNamesToInvalidate = null;
        if (fieldArgumentValues.TryGetValue(NameConstants.DependentCommands, out var dependentCommandsValue))
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

        var notifyPropertyChangedForAttributes = fieldSymbolAttributes
            .Where(x => x.AttributeClass?.Name
                is NameConstants.NotifyPropertyChangedForAttribute
                or NameConstants.NotifyPropertyChangedFor)
            .ToList();

        if (notifyPropertyChangedForAttributes.Count > 0)
        {
            propertyNamesToInvalidate ??= [];

            foreach (var notifyPropertyChangedForAttribute in notifyPropertyChangedForAttributes)
            {
                var argumentValues = notifyPropertyChangedForAttribute.ExtractConstructorArgumentValues();
                foreach (var argumentValue in argumentValues
                             .Where(parameter => !propertyNamesToInvalidate.Contains(parameter.Value!, StringComparer.Ordinal)))
                {
                    propertyNamesToInvalidate.Add(argumentValue.Value!);
                }
            }
        }

        propertiesToGenerate.Add(
            new ObservablePropertyToGenerate(
                propertyName,
                propertyType,
                backingFieldName,
                fieldSymbol.IsReadOnly)
            {
                PropertyNamesToInvalidate = propertyNamesToInvalidate,
                CommandNamesToInvalidate = commandNamesToInvalidate,
                BeforeChangedCallback = beforeChangedCallback,
                AfterChangedCallback = afterChangedCallback,
                BroadcastOnChange = broadcastOnChange,
            });
    }
}