namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

internal static class RelayCommandInspector
{
    public static List<RelayCommandToGenerate> Inspect(
        INamedTypeSymbol classSymbol,
        ImmutableArray<ISymbol> memberSymbols)
    {
        var result = new List<RelayCommandToGenerate>();

        // Project-level default for the GenerateDocumentation flag, opted in via
        // [assembly: GenerateDocumentationDefault]. When present, methods that
        // don't set the flag explicitly still emit the default summary.
        var generateDocumentationDefault = HasGenerateDocumentationDefaultAttribute(classSymbol);

        foreach (var memberSymbol in memberSymbols)
        {
            if (memberSymbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            foreach (var attr in methodSymbol.GetAttributes())
            {
                if (attr.AttributeClass?.Name
                    is not NameConstants.RelayCommandAttribute
                    and not NameConstants.RelayCommand)
                {
                    continue;
                }

                AppendRelayCommandToGenerate(
                    classSymbol,
                    methodSymbol,
                    memberSymbols,
                    attr,
                    generateDocumentationDefault,
                    result);
            }
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

    // True when a base class declares the property, or an [ObservableProperty] field for it.
    private static bool HasInheritedPropertyName(
        INamedTypeSymbol classSymbol,
        string name)
    {
        for (var baseType = classSymbol.BaseType;
             baseType is not null;
             baseType = baseType.BaseType)
        {
            var baseMemberSymbols = baseType.GetMembers();
            if (baseMemberSymbols.HasPropertyName(name) ||
                baseMemberSymbols.HasObservablePropertyOrFieldName(name))
            {
                return true;
            }
        }

        return false;
    }

    // Returns the ParameterValue / ParameterValues arguments as C# expressions.
    // Bound constants are rendered as literals (quoted strings, true/false, qualified enum
    // members); the textual fallback is only used when the arguments could not be bound.
    private static List<string> ExtractParameterValues(
        AttributeData relayCommandAttribute,
        IDictionary<string, string?> relayCommandArgumentValues)
    {
        foreach (var namedArgument in relayCommandAttribute.NamedArguments)
        {
            if (namedArgument.Key == NameConstants.ParameterValue &&
                namedArgument.Value.Kind is not (TypedConstantKind.Error or TypedConstantKind.Array))
            {
                return [namedArgument.Value.ToCSharpString()];
            }

            if (namedArgument.Key == NameConstants.ParameterValues &&
                namedArgument.Value is { Kind: TypedConstantKind.Array, IsNull: false } &&
                namedArgument.Value.Values.All(x => x.Kind is not (TypedConstantKind.Error or TypedConstantKind.Array)))
            {
                return namedArgument.Value.Values
                    .Select(x => x.ToCSharpString())
                    .ToList();
            }
        }

        var parameterValues = new List<string>();
        if (relayCommandArgumentValues.TryGetValue(NameConstants.ParameterValue, out var parameterValueValue))
        {
            parameterValues.Add(parameterValueValue!);
        }
        else if (relayCommandArgumentValues.TryGetValue(NameConstants.ParameterValues, out var parameterValuesValue))
        {
            parameterValues.AddRange(
                parameterValuesValue!
                    .Split(',')
                    .Select(x => x.Trim()));
        }

        return parameterValues;
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static void AppendRelayCommandToGenerate(
        INamedTypeSymbol classSymbol,
        IMethodSymbol methodSymbol,
        ImmutableArray<ISymbol> memberSymbols,
        AttributeData relayCommandAttribute,
        bool generateDocumentationDefault,
        List<RelayCommandToGenerate> relayCommandsToGenerate)
    {
        var relayCommandArgumentValues = relayCommandAttribute.ExtractConstructorArgumentValues();

        var commandName = relayCommandArgumentValues.TryGetValue(NameConstants.Name, out var nameValue)
            ? nameValue!.EnsureFirstCharacterToUpper()
            : methodSymbol.Name.EnsureFirstCharacterToUpper();

        if (commandName.EndsWith(NameConstants.Handler, StringComparison.Ordinal))
        {
            commandName = commandName.Substring(0, commandName.Length - NameConstants.Handler.Length);
        }

        if (!commandName.EndsWith(NameConstants.Command, StringComparison.Ordinal))
        {
            commandName += NameConstants.Command;
        }

        if (commandName == methodSymbol.Name)
        {
            commandName += "X";
        }

        string? canExecuteName = null;
        if (relayCommandArgumentValues.TryGetValue(NameConstants.CanExecute, out var canExecuteNameValue))
        {
            canExecuteName = canExecuteNameValue!.ExtractInnerContent();
        }

        var invertCanExecute = relayCommandArgumentValues.TryGetValue(NameConstants.InvertCanExecute, out var invertCanExecuteValue) &&
                               "true".Equals(invertCanExecuteValue, StringComparison.OrdinalIgnoreCase);

        var usePropertyForCanExecute = false;
        if (canExecuteName is not null)
        {
            usePropertyForCanExecute = memberSymbols.HasPropertyName(canExecuteName) ||
                                       memberSymbols.HasObservablePropertyOrFieldName(canExecuteName) ||
                                       HasInheritedPropertyName(classSymbol, canExecuteName);
        }

        var parameterValues = ExtractParameterValues(relayCommandAttribute, relayCommandArgumentValues);

        List<string>? parameterTypes = null;
        List<string>? parameterNames = null;
        if (methodSymbol.Parameters.Length > 0)
        {
            parameterTypes = methodSymbol.Parameters
                .Select(parameterSymbol => parameterSymbol.Type.ToDisplayString())
                .ToList();
            parameterNames = methodSymbol.Parameters
                .Select(parameterSymbol => parameterSymbol.Name)
                .ToList();
        }

        var useTask = methodSymbol.ReturnType.Name
            is NameConstants.Task
            or NameConstants.ValueTask;

        var executeOnBackgroundThread = false;
        if (relayCommandArgumentValues.TryGetValue(NameConstants.ExecuteOnBackgroundThread, out var executeOnBackgroundThreadValue) &&
            bool.TryParse(executeOnBackgroundThreadValue, out var executeOnBackgroundThreadValueAsBool))
        {
            executeOnBackgroundThread = executeOnBackgroundThreadValueAsBool;
        }

        var autoSetIsBusy = false;
        if (relayCommandArgumentValues.TryGetValue(NameConstants.AutoSetIsBusy, out var autoSetIsBusyValue) &&
            bool.TryParse(autoSetIsBusyValue, out var autoSetIsBusyValueAsBool))
        {
            autoSetIsBusy = autoSetIsBusyValueAsBool;
        }

        var supportsCancellation = false;
        if (relayCommandArgumentValues.TryGetValue(NameConstants.SupportsCancellation, out var supportsCancellationValue) &&
            bool.TryParse(supportsCancellationValue, out var supportsCancellationValueAsBool))
        {
            supportsCancellation = supportsCancellationValueAsBool;
        }

        // Three-state resolution mirrors ObservablePropertyInspector: explicit per-attribute
        // value wins; otherwise fall back to assembly-level [GenerateDocumentationDefault].
        bool generateDocumentation;
        if (relayCommandArgumentValues.TryGetValue(NameConstants.GenerateDocumentation, out var generateDocumentationValue) &&
            bool.TryParse(generateDocumentationValue, out var generateDocumentationValueAsBool))
        {
            generateDocumentation = generateDocumentationValueAsBool;
        }
        else
        {
            generateDocumentation = generateDocumentationDefault;
        }

        var documentationComments = methodSymbol.ExtractDocumentationComments();

        // A ValueTask-returning method is called through a generated Task-returning adapter.
        var returnsValueTask = methodSymbol.ReturnType.Name is NameConstants.ValueTask;

        relayCommandsToGenerate.Add(
            new RelayCommandToGenerate(
                commandName,
                returnsValueTask ? methodSymbol.Name + "AsTask" : methodSymbol.Name,
                new EquatableArray<string>(parameterTypes?.ToArray() ?? []),
                new EquatableArray<string>(parameterNames?.ToArray() ?? []),
                new EquatableArray<string>(parameterValues.ToArray()),
                canExecuteName,
                invertCanExecute,
                usePropertyForCanExecute,
                methodSymbol.IsAsync,
                useTask,
                executeOnBackgroundThread,
                autoSetIsBusy,
                supportsCancellation,
                generateDocumentation)
            {
                DocumentationComments = new EquatableArray<string>(documentationComments?.ToArray() ?? []),
                ValueTaskMethodName = returnsValueTask ? methodSymbol.Name : null,
            });
    }
}