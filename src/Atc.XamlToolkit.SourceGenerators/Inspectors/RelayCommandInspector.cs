namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

internal static class RelayCommandInspector
{
    public static List<RelayCommandToGenerate> Inspect(
        INamedTypeSymbol classSymbol,
        ImmutableArray<ISymbol> memberSymbols)
    {
        var result = new List<RelayCommandToGenerate>();

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
                    result);
            }
        }

        return result;
    }

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static void AppendRelayCommandToGenerate(
        IMethodSymbol methodSymbol,
        ImmutableArray<ISymbol> memberSymbols,
        AttributeData relayCommandAttribute,
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

        relayCommandsToGenerate.Add(
            new RelayCommandToGenerate(
                commandName,
                methodSymbol.Name,
                parameterTypes?.ToArray(),
                parameterNames?.ToArray(),
                parameterValues.Count == 0 ? null : parameterValues.ToArray(),
                canExecuteName,
                invertCanExecute,
                usePropertyForCanExecute,
                methodSymbol.IsAsync,
                useTask,
                executeOnBackgroundThread,
                autoSetIsBusy,
                supportsCancellation));
    }
}