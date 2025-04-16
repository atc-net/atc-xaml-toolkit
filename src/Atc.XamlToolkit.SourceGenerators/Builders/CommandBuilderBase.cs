namespace Atc.XamlToolkit.SourceGenerators.Builders;

[SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
internal abstract class CommandBuilderBase : BuilderBase
{
    public virtual void GenerateRelayCommands(
        CommandBuilderBase builder,
        IEnumerable<RelayCommandToGenerate>? relayCommandsToGenerate)
    {
        if (relayCommandsToGenerate is null)
        {
            return;
        }

        var relayCommandsToGenerateAsArray = relayCommandsToGenerate.ToArray();

        if (relayCommandsToGenerateAsArray.Length == 0)
        {
            return;
        }

        builder.AppendLineBeforeMember();

        foreach (var relayCommandToGenerate in relayCommandsToGenerateAsArray)
        {
            var interfaceType = relayCommandToGenerate.IsAsync
                ? NameConstants.IRelayCommandAsync
                : NameConstants.IRelayCommand;

            if (relayCommandToGenerate.ParameterValues is null)
            {
                GenerateRelayCommandBackingFieldWithOutParameterValues(builder, relayCommandToGenerate, interfaceType);
            }
            else
            {
                GenerateRelayCommandBackingFieldWithParameterValues(builder, relayCommandToGenerate, interfaceType);
            }
        }

        foreach (var relayCommandToGenerate in relayCommandsToGenerateAsArray)
        {
            builder.AppendLineBeforeMember();

            var interfaceType = relayCommandToGenerate.IsAsync
                ? NameConstants.IRelayCommandAsync
                : NameConstants.IRelayCommand;

            var implementationType = relayCommandToGenerate.IsAsync
                ? NameConstants.RelayCommandAsync
                : NameConstants.RelayCommand;

            if (relayCommandToGenerate.ParameterValues is null)
            {
                GenerateRelayCommandWithOutParameterValues(builder, relayCommandToGenerate, interfaceType, implementationType);
            }
            else
            {
                GenerateRelayCommandWithParameterValues(builder, relayCommandToGenerate, interfaceType, implementationType);
            }
        }
    }

    private static void GenerateRelayCommandBackingFieldWithOutParameterValues(
        CommandBuilderBase builder,
        RelayCommandToGenerate rc,
        string interfaceType)
    {
        if (rc.ParameterTypes is null || rc.ParameterTypes.Length == 0)
        {
            GenerateCommandBackingFieldLine(
                builder,
                interfaceType,
                rc.CommandName);
        }
        else if (rc.ParameterTypes.Length == 1)
        {
            var parameterType = rc.ParameterTypes[0];

            if (parameterType.EndsWith(nameof(CancellationToken), StringComparison.Ordinal))
            {
                GenerateCommandBackingFieldLine(
                    builder,
                    interfaceType,
                    rc.CommandName);
            }
            else
            {
                var generic = $"<{parameterType}>";
                GenerateCommandBackingFieldLine(
                    builder,
                    $"{interfaceType}{generic}",
                    rc.CommandName);
            }
        }
        else
        {
            var (tupleGeneric, _, _) = GetConstructorParametersWithParameterTypes(rc);
            GenerateCommandBackingFieldLine(
                builder,
                $"{interfaceType}{tupleGeneric}",
                rc.CommandName);
        }
    }

    private static void GenerateRelayCommandBackingFieldWithParameterValues(
        CommandBuilderBase builder,
        RelayCommandToGenerate rc,
        string interfaceType)
    {
        GenerateCommandBackingFieldLine(
            builder,
            interfaceType,
            rc.CommandName);
    }

    private static void GenerateRelayCommandWithOutParameterValues(
        CommandBuilderBase builder,
        RelayCommandToGenerate rc,
        string interfaceType,
        string implementationType)
    {
        if (rc.ParameterTypes is null || rc.ParameterTypes.Length == 0)
        {
            GenerateCommandLine(
                builder,
                interfaceType,
                implementationType,
                rc.CommandName,
                rc.MethodName,
                canExecuteName: rc.CanExecuteName,
                invertCanExecute: rc.InvertCanExecute,
                usePropertyForCanExecute: rc.UsePropertyForCanExecute,
                useExecuteOnBackgroundThread: rc.ExecuteOnBackgroundThread);
        }
        else if (rc.ParameterTypes.Length == 1)
        {
            var parameterType = rc.ParameterTypes[0];

            if (parameterType.EndsWith(nameof(CancellationToken), StringComparison.Ordinal))
            {
                GenerateCommandLine(
                    builder,
                    interfaceType,
                    implementationType,
                    rc.CommandName,
                    $"{rc.MethodName}(CancellationToken.None)",
                    canExecuteName: rc.CanExecuteName,
                    invertCanExecute: rc.InvertCanExecute,
                    isLambda: true,
                    useExecuteOnBackgroundThread: rc.ExecuteOnBackgroundThread);
            }
            else
            {
                var generic = $"<{parameterType}>";
                GenerateCommandLine(
                    builder,
                    $"{interfaceType}{generic}",
                    $"{implementationType}{generic}",
                    rc.CommandName,
                    rc.MethodName,
                    canExecuteName: rc.CanExecuteName,
                    invertCanExecute: rc.InvertCanExecute,
                    usePropertyForCanExecute: rc.UsePropertyForCanExecute,
                    useExecuteOnBackgroundThread: rc.ExecuteOnBackgroundThread);
            }
        }
        else
        {
            var (tupleGeneric, constructorParametersMulti, filteredConstructorParameters) = GetConstructorParametersWithParameterTypes(rc);
            if (rc.UsePropertyForCanExecute)
            {
                GenerateCommandLine(
                    builder,
                    $"{interfaceType}{tupleGeneric}",
                    $"{implementationType}{tupleGeneric}",
                    rc.CommandName,
                    $"x => {rc.MethodName}({constructorParametersMulti})",
                    canExecuteName: rc.CanExecuteName is null ? null : rc.InvertCanExecute ? $"x => !{rc.CanExecuteName}" : $"x => {rc.CanExecuteName}",
                    useExecuteOnBackgroundThread: rc.ExecuteOnBackgroundThread);
            }
            else
            {
                GenerateCommandLine(
                    builder,
                    $"{interfaceType}{tupleGeneric}",
                    $"{implementationType}{tupleGeneric}",
                    rc.CommandName,
                    $"x => {rc.MethodName}({constructorParametersMulti})",
                    canExecuteName: rc.CanExecuteName is null ? null : rc.InvertCanExecute ? $"x => !{rc.CanExecuteName}({filteredConstructorParameters})" : $"x => {rc.CanExecuteName}({filteredConstructorParameters})",
                    useExecuteOnBackgroundThread: rc.ExecuteOnBackgroundThread);
            }
        }
    }

    private static void GenerateRelayCommandWithParameterValues(
        CommandBuilderBase builder,
        RelayCommandToGenerate rc,
        string interfaceType,
        string implementationType)
    {
        if (rc.ParameterValues!.Length == 1)
        {
            GenerateCommandLine(
                builder,
                interfaceType,
                implementationType,
                rc.CommandName,
                $"() => {rc.MethodName}({rc.ParameterValues[0]})",
                rc.CanExecuteName);
        }
        else
        {
            var constructorParameters = string.Join(", ", rc.ParameterValues!);
            if (rc.CanExecuteName is null)
            {
                GenerateCommandLine(
                    builder,
                    interfaceType,
                    implementationType,
                    rc.CommandName,
                    $"() => {rc.MethodName}({constructorParameters})");
            }
            else
            {
                if (rc.UsePropertyForCanExecute)
                {
                    GenerateCommandLine(
                        builder,
                        interfaceType,
                        implementationType,
                        rc.CommandName,
                        $"() => {rc.MethodName}({constructorParameters})",
                        rc.InvertCanExecute ? $"!{rc.CanExecuteName}" : $"{rc.CanExecuteName}");
                }
                else
                {
                    GenerateCommandLine(
                        builder,
                        interfaceType,
                        implementationType,
                        rc.CommandName,
                        $"() => {rc.MethodName}({constructorParameters})",
                        rc.InvertCanExecute ? $"!{rc.CanExecuteName}({constructorParameters})" : $"{rc.CanExecuteName}({constructorParameters})");
                }
            }
        }
    }

    private static (
        string Generic,
        string ConstructorParameters,
        string FilteredConstructorParameters) GetConstructorParametersWithParameterTypes(
        RelayCommandToGenerate rc)
    {
        var filteredParameterTypes = rc.ParameterTypes!
            .Where(x => !x.EndsWith(nameof(CancellationToken), StringComparison.Ordinal))
            .ToList();

        var generic = filteredParameterTypes.Count > 1
            ? $"<({string.Join(", ", filteredParameterTypes)})>"
            : $"<{filteredParameterTypes[0]}>";

        var constructorParametersList = new List<string>();
        var tupleItemNumber = 0;

        foreach (var parameterType in rc.ParameterTypes!)
        {
            if (parameterType.EndsWith(nameof(CancellationToken), StringComparison.Ordinal))
            {
                constructorParametersList.Add("CancellationToken.None");
            }
            else
            {
                tupleItemNumber++;
                constructorParametersList.Add($"x.Item{tupleItemNumber}");
            }
        }

        var constructorParameters = string.Join(", ", constructorParametersList);
        var filteredConstructorParameters = string.Join(", ", constructorParametersList.Where(x => !x.Contains(nameof(CancellationToken))));

        return (generic, constructorParameters, filteredConstructorParameters);
    }

    private static void GenerateCommandBackingFieldLine(
        CommandBuilderBase builder,
        string interfaceType,
        string commandName)
    {
        builder.AppendLine($"private {interfaceType}? {commandName.EnsureFirstCharacterToLower()};");
    }

    private static void GenerateCommandLine(
        CommandBuilderBase builder,
        string interfaceType,
        string implementationType,
        string commandName,
        string constructorParameters,
        string? canExecuteName = null,
        bool invertCanExecute = false,
        bool usePropertyForCanExecute = false,
        bool isLambda = false,
        bool useExecuteOnBackgroundThread = false)
    {
        builder.Append($"public {interfaceType} {commandName} => {commandName.EnsureFirstCharacterToLower()} ??= new {implementationType}(");

        if (useExecuteOnBackgroundThread)
        {
            builder.AppendLine();
            builder.IncreaseIndent();
            if (constructorParameters.StartsWith("x =>", StringComparison.Ordinal) &&
                !implementationType.Contains("<("))
            {
                builder.Append("x => ");
                constructorParameters = constructorParameters
                    .Replace("x => ", string.Empty)
                    .Replace("x.Item1", "x");

                canExecuteName = canExecuteName?
                    .Replace("x => ", string.Empty)
                    .Replace("(x.Item1)", string.Empty);
            }
            else if (implementationType.Contains("<"))
            {
                builder.Append("x => ");
                constructorParameters += "(x)";
            }
            else
            {
                builder.Append("() => ");
            }

            builder.Append("Task.Run(");
            if (constructorParameters.Contains("("))
            {
                builder.Append("() => ");
            }

            if (canExecuteName is null)
            {
                builder.AppendLine($"{constructorParameters}));");
            }
            else
            {
                builder.AppendLine($"{constructorParameters}),");
                builder.Append(canExecuteName);
                builder.AppendLine(");");
            }

            builder.DecreaseIndent();
        }
        else
        {
            var commandInstance = isLambda
                ? $"() => {constructorParameters}"
                : constructorParameters;

            if (constructorParameters.StartsWith("x =>", StringComparison.Ordinal) &&
                !implementationType.Contains("<("))
            {
                commandInstance = commandInstance
                    .Replace("x.Item1", "x");

                canExecuteName = canExecuteName?
                    .Replace("x => ", string.Empty)
                    .Replace("(x.Item1)", string.Empty);
            }

            if (canExecuteName is null)
            {
                builder.Append(commandInstance);
                builder.AppendLine(");");
            }
            else
            {
                builder.AppendLine();
                builder.IncreaseIndent();
                builder.Append(commandInstance);

                if (usePropertyForCanExecute)
                {
                    if (interfaceType.Contains('<'))
                    {
                        builder.AppendLine(",");
                        builder.Append(
                            invertCanExecute
                                ? $"_ => !{canExecuteName}"
                                : $"_ => {canExecuteName}");
                    }
                    else
                    {
                        builder.AppendLine(",");
                        builder.Append(
                            invertCanExecute
                                ? $"() => !{canExecuteName}"
                                : $"() => {canExecuteName}");
                    }
                }
                else
                {
                    builder.AppendLine(",");
                    builder.Append(
                        invertCanExecute
                            ? $"!{canExecuteName}"
                            : canExecuteName);
                }

                builder.AppendLine(");");
                builder.DecreaseIndent();
            }
        }
    }
}