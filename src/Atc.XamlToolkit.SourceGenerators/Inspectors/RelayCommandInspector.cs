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

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static void AppendRelayCommandToGenerate(
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
                                       memberSymbols.HasObservablePropertyOrFieldName(canExecuteName);
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

        relayCommandsToGenerate.Add(
            new RelayCommandToGenerate(
                commandName,
                methodSymbol.Name,
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
                generateDocumentation));
    }
}