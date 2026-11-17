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
            // The value is null when the attribute says '= null'
            var routingStrategyValue = (fieldArgumentValues.First().Value ?? string.Empty).Replace(NameConstants.RoutingStrategy + ".", string.Empty);
            routingStrategy = routingStrategyValue switch
            {
                "0" => "Tunnel",
                "1" => "Bubble",
                "2" => "Direct",
                _ => routingStrategy,
            };

            if (fieldArgumentValues.TryGetValue(NameConstants.HandlerType, out var handlerTypeNameValue) &&
                handlerTypeNameValue is not null)
            {
                handlerTypeName = handlerTypeNameValue.ExtractInnerContent();
            }
        }

        // With the type itself at hand its name is taken from it: cutting the type out of
        // the text 'typeof(...)' goes wrong for a type that has parentheses, such as a tuple.
        var handlerTypeSymbol = GetHandlerTypeSymbol(fieldPropertyAttribute);
        if (handlerTypeSymbol is { IsUnboundGenericType: false, TypeKind: not TypeKind.Error })
        {
            handlerTypeName = handlerTypeSymbol.ToDisplayString();
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
                OwnerType: classSymbol.Name + classSymbol.GetTypeParameterList(),
                Name: propertyName,
                RoutingStrategy: routingStrategy,
                HandlerTypeName: handlerTypeName)
            {
                DocumentationComments = new EquatableArray<string>(documentationComments?.ToArray() ?? []),
                IsOwnerTypeStatic = classSymbol.IsStatic,
                IsOwnerTypeSealed = classSymbol.IsSealed,
                AddOwnerOf = fieldPropertyAttribute.ExtractAddOwnerOf(),
                GenerateRaiseMethod = IsOptionSet(fieldArgumentValues, NameConstants.GenerateRaiseMethod, defaultValue: false),
                HandlerArgsAreNotRoutedEventArgs = HasEventArgsThatAreNotRoutedEventArgs(handlerTypeSymbol),
                GenerateHandlerMethods = IsOptionSet(fieldArgumentValues, NameConstants.GenerateHandlerMethods, defaultValue: true),
                RaiseMethodAccessModifier = GetRaiseMethodAccessModifier(classSymbol, fieldPropertyAttribute),
                GenerateDocumentation = ResolveGenerateDocumentation(classSymbol, fieldArgumentValues),
            };

        NoteRaiseMethodThatCannotBeGenerated(routedEvent);
        NoteOptionsWithoutEffect(routedEvent, fieldArgumentValues);

        if (hasUnboundHandlerType)
        {
            routedEvent.IgnoredOptions = new EquatableArray<string>(
                routedEvent.IgnoredOptions
                    .Append($"{NameConstants.HandlerType}|an unbound generic handler type is only closed in a generic property template with as many type parameters; RoutedEventHandler is used")
                    .ToArray());
        }

        routedEvent.DiagnosticLocation = routedEvent.IgnoredOptions.IsEmpty
            ? null
            : DiagnosticLocation.From(fieldPropertyAttribute);

        routedEventToGenerate.Add(routedEvent);
    }

    private static INamedTypeSymbol? GetHandlerTypeSymbol(
        AttributeData fieldPropertyAttribute)
    {
        foreach (var namedArgument in fieldPropertyAttribute.NamedArguments)
        {
            if (namedArgument.Key == NameConstants.HandlerType)
            {
                return namedArgument.Value.Value as INamedTypeSymbol;
            }
        }

        return null;
    }

    // EventHandler<TArgs> can be the handler of a routed event, but only event args that
    // derive from RoutedEventArgs can be raised with RaiseEvent.
    private static bool HasEventArgsThatAreNotRoutedEventArgs(
        INamedTypeSymbol? handlerTypeSymbol)
    {
        if (handlerTypeSymbol is not { Name: "EventHandler", TypeArguments.Length: 1 } ||
            handlerTypeSymbol.TypeArguments[0] is not { TypeKind: not TypeKind.Error } argsType)
        {
            return false;
        }

        for (var type = argsType; type is not null; type = type.BaseType)
        {
            if (type.Name == "RoutedEventArgs")
            {
                return false;
            }
        }

        return true;
    }

    private static void NoteOptionsWithoutEffect(
        RoutedEventToGenerate routedEvent,
        IDictionary<string, string?> fieldArgumentValues)
    {
        var ignoredOptions = new List<string>(routedEvent.IgnoredOptions);

        if (routedEvent is { RaiseMethodAccessModifier: not null, GenerateRaiseMethod: false })
        {
            ignoredOptions.Add($"{NameConstants.RaiseMethodAccessModifier}|no Raise method is generated without GenerateRaiseMethod = true");
        }

        if (!routedEvent.IsOwnerTypeStatic &&
            fieldArgumentValues.ContainsKey(NameConstants.GenerateHandlerMethods))
        {
            ignoredOptions.Add($"{NameConstants.GenerateHandlerMethods}|handler methods are only generated in a static class; this class gets an event");
        }

        if (ignoredOptions.Count != routedEvent.IgnoredOptions.Count)
        {
            routedEvent.IgnoredOptions = new EquatableArray<string>(ignoredOptions.ToArray());
        }
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
        INamedTypeSymbol classSymbol,
        AttributeData fieldPropertyAttribute)
    {
        if (fieldPropertyAttribute.ExtractEnumArgumentName(NameConstants.RaiseMethodAccessModifier) is null)
        {
            return null;
        }

        var modifier = fieldPropertyAttribute.ExtractAccessModifier(NameConstants.RaiseMethodAccessModifier);
        return modifier.Length == 0 ? "public" : classSymbol.NarrowAccessModifier(modifier);
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