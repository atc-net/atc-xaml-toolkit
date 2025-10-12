namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

internal static class RoutedEventInspector
{
    public static List<RoutedEventToGenerate> Inspect(
        INamedTypeSymbol classSymbol)
    {
        var result = new List<RoutedEventToGenerate>();

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

            if (fieldSymbol.Name.EndsWith("Event", StringComparison.Ordinal))
            {
                continue;
            }

            if (!fieldSymbol.Type.ToString().Contains("RoutedEvent"))
            {
                continue;
            }

            var fieldSymbolAttributes = fieldSymbol.GetAttributes();

            var fieldPropertyAttribute = fieldSymbolAttributes
                .FirstOrDefault(x => x.AttributeClass?.Name
                    is NameConstants.RoutedEventAttribute
                    or NameConstants.RoutedEvent);

            if (fieldPropertyAttribute is null)
            {
                continue;
            }

            AppendRoutedEventToGenerate(
                classSymbol,
                fieldSymbol,
                fieldPropertyAttribute,
                result);
        }

        return result;
    }

    private static void AppendRoutedEventToGenerate(
        INamedTypeSymbol classSymbol,
        IFieldSymbol fieldSymbol,
        AttributeData fieldPropertyAttribute,
        List<RoutedEventToGenerate> routedEventToGenerate)
    {
        var propertyName = fieldSymbol
            .Name
            .RemovePrefixFromField()
            .EnsureFirstCharacterToUpper();

        var fieldArgumentValues = fieldPropertyAttribute.ExtractConstructorArgumentValues();

        var routingStrategy = "Bubble";
        var handlerTypeName = NameConstants.RoutedEventHandler;

        if (fieldArgumentValues.Count > 0)
        {
            var routingStrategyValue = fieldArgumentValues.First().Value!.Replace(NameConstants.RoutingStrategy + ".", string.Empty);
            routingStrategy = routingStrategyValue switch
            {
                "0" => "Tunnel",
                "1" => "Bubble",
                "2" => "Direct",
                _ => routingStrategy,
            };

            if (fieldArgumentValues.TryGetValue(NameConstants.HandlerType, out var handlerTypeNameValue))
            {
                handlerTypeName = handlerTypeNameValue!.ExtractInnerContent();
            }
        }

        routedEventToGenerate.Add(
            new RoutedEventToGenerate(
                ownerType: classSymbol.Name,
                name: propertyName,
                routingStrategy: routingStrategy,
                handlerTypeName: handlerTypeName));
    }
}