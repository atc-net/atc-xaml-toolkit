namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

internal static class RoutedEventInspector
{
    public static List<RoutedEventToGenerate> Inspect(
        INamedTypeSymbol classSymbol,
        ImmutableArray<ISymbol> memberSymbols)
    {
        var result = new List<RoutedEventToGenerate>();

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
                .FirstOrDefault(x => x.GetToolkitAttributeName()
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

    private static readonly SymbolDisplayFormat ClosedHandlerTypeFormat = SymbolDisplayFormat
        .FullyQualifiedFormat
        .AddMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    // An attribute argument cannot use a type parameter, so a routed event in a generic
    // property template names its handler as an unbound generic type, for example
    // typeof(RoutedPropertyChangedEventHandler<>). It is closed with the type arguments
    // the template is used with.
    private static string? GetHandlerTypeClosedByTemplate(
        IFieldSymbol fieldSymbol,
        AttributeData fieldPropertyAttribute)
    {
        if (fieldSymbol.ContainingType is not { IsGenericType: true } templateType)
        {
            return null;
        }

        foreach (var namedArgument in fieldPropertyAttribute.NamedArguments)
        {
            if (namedArgument.Key == NameConstants.HandlerType &&
                namedArgument.Value.Value is INamedTypeSymbol { IsUnboundGenericType: true } handlerType &&
                handlerType.Arity == templateType.TypeArguments.Length)
            {
                return handlerType
                    .OriginalDefinition
                    .Construct(templateType.TypeArguments, templateType.TypeArgumentNullableAnnotations)
                    .ToDisplayString(ClosedHandlerTypeFormat);
            }
        }

        return null;
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
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

        handlerTypeName = GetHandlerTypeClosedByTemplate(fieldSymbol, fieldPropertyAttribute) ?? handlerTypeName;

        // A handler type that is still an unbound generic (typeof(X<>)) cannot be the type of
        // an event. It is only closed in a generic property template with as many type
        // parameters.
        var hasUnboundHandlerType = handlerTypeName.Contains("<>") || handlerTypeName.Contains("<,");
        if (hasUnboundHandlerType)
        {
            handlerTypeName = NameConstants.RoutedEventHandler;
        }

        var documentationComments = fieldSymbol.ExtractDocumentationComments();

        var routedEvent =
            new RoutedEventToGenerate(
                OwnerType: classSymbol.Name,
                Name: propertyName,
                RoutingStrategy: routingStrategy,
                HandlerTypeName: handlerTypeName)
            {
                DocumentationComments = new EquatableArray<string>(documentationComments?.ToArray() ?? []),
                IsOwnerTypeStatic = classSymbol.IsStatic,
                IsOwnerTypeSealed = classSymbol.IsSealed,
                AddOwnerOf = fieldPropertyAttribute.ExtractAddOwnerOf(),
                GenerateRaiseMethod = IsOptionSet(fieldArgumentValues, NameConstants.GenerateRaiseMethod, defaultValue: false),
                GenerateHandlerMethods = IsOptionSet(fieldArgumentValues, NameConstants.GenerateHandlerMethods, defaultValue: true),
                RaiseMethodAccessModifier = GetRaiseMethodAccessModifier(fieldPropertyAttribute),
                GenerateDocumentation = ResolveGenerateDocumentation(classSymbol, fieldArgumentValues),
            };

        NoteRaiseMethodThatCannotBeGenerated(routedEvent);

        if (hasUnboundHandlerType)
        {
            routedEvent.IgnoredOptions = new EquatableArray<string>(
                routedEvent.IgnoredOptions
                    .Append($"{NameConstants.HandlerType}|an unbound generic handler type is only closed in a generic property template with as many type parameters; RoutedEventHandler is used")
                    .ToArray());
        }

        routedEventToGenerate.Add(routedEvent);
    }

    private static bool IsOptionSet(
        IDictionary<string, string?> fieldArgumentValues,
        string name,
        bool defaultValue)
        => fieldArgumentValues.TryGetValue(name, out var value) &&
           bool.TryParse(value, out var result)
            ? result
            : defaultValue;

    // An explicit per-attribute value wins; otherwise the assembly-level
    // [GenerateDocumentationDefault] decides.
    private static bool ResolveGenerateDocumentation(
        INamedTypeSymbol classSymbol,
        IDictionary<string, string?> fieldArgumentValues)
        => fieldArgumentValues.TryGetValue(NameConstants.GenerateDocumentation, out var value) &&
           bool.TryParse(value, out var generateDocumentation)
            ? generateDocumentation
            : FrameworkElementInspectorHelper.HasGenerateDocumentationDefaultAttribute(classSymbol);

    // Public reads as an empty modifier, so look at whether the option was stated at all.
    private static string? GetRaiseMethodAccessModifier(
        AttributeData fieldPropertyAttribute)
    {
        if (fieldPropertyAttribute.ExtractEnumArgumentName(NameConstants.RaiseMethodAccessModifier) is null)
        {
            return null;
        }

        var modifier = fieldPropertyAttribute.ExtractAccessModifier(NameConstants.RaiseMethodAccessModifier);
        return modifier.Length == 0 ? "public" : modifier;
    }

    private static void NoteRaiseMethodThatCannotBeGenerated(
        RoutedEventToGenerate routedEvent)
    {
        if (!routedEvent.GenerateRaiseMethod || routedEvent.CanGenerateRaiseMethod)
        {
            return;
        }

        var reason = routedEvent.IsOwnerTypeStatic
            ? "a static class cannot raise an event; raise it on the element with RaiseEvent"
            : "the event args of the handler type cannot be created without guessing; only RoutedEventHandler, RoutedPropertyChangedEventHandler<T> and EventHandler<TArgs> are supported";

        routedEvent.IgnoredOptions = new EquatableArray<string>([$"{NameConstants.GenerateRaiseMethod}|{reason}"]);
    }
}