// ReSharper disable InvertIf
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

        var commands = relayCommandsToGenerate.ToArray();
        if (commands.Length == 0)
        {
            return;
        }

        builder.AppendLineBeforeMember();

        AppendPrivateBackingFields(builder, commands);

        builder.AppendLine();

        AppendPublicProperties(builder, commands);

        AppendCancelMethods(builder, commands);
    }

    private static void AppendPrivateBackingFields(
        CommandBuilderBase builder,
        RelayCommandToGenerate[] commands)
    {
        foreach (var cmd in commands)
        {
            var interfaceType = cmd.GetInterfaceType();
            var generic = cmd.GetGenericArgAsString();
            var fieldName = cmd.GetCommandAsFieldName();

            builder.AppendLine($"private {interfaceType}{generic}? {fieldName};");
        }
    }

    private static void AppendPublicProperties(
        CommandBuilderBase builder,
        RelayCommandToGenerate[] commands)
    {
        for (var i = 0; i < commands.Length; i++)
        {
            var cmd = commands[i];
            var interfaceType = cmd.GetInterfaceType();
            var implementationType = cmd.GetImplementationType();
            var generic = cmd.GetGenericArgAsString();
            var propName = cmd.GetCommandAsPropertyName();
            var fieldName = cmd.GetCommandAsFieldName();
            var execExpr = BuildExecuteExpression(cmd);
            var canExecExpr = BuildCanExecuteExpression(cmd);
            var hasCan = canExecExpr is not null;
            var hasExecuteOnBackgroundThreadParam = cmd.SupportsCancellation &&
                                                    cmd.ExecuteOnBackgroundThread &&
                                                    cmd.HasParameterTypesOfCancellationToken();
            var appendAsMultiLine = hasCan ||
                                    cmd.ExecuteOnBackgroundThread ||
                                    cmd.AutoSetIsBusy ||
                                    hasExecuteOnBackgroundThreadParam;

            if (appendAsMultiLine)
            {
                builder.AppendLine($"public {interfaceType}{generic} {propName} => {fieldName} ??= new {implementationType}{generic}(");
                builder.IncreaseIndent();

                if (cmd.AutoSetIsBusy)
                {
                    AppendAutoSetIsBusyBlock(
                        builder,
                        cmd,
                        execExpr,
                        hasCan);
                }
                else
                {
                    var suffix = hasCan || hasExecuteOnBackgroundThreadParam ? "," : ");";
                    builder.AppendLine($"{execExpr}{suffix}");
                }

                if (hasCan)
                {
                    var suffix = hasExecuteOnBackgroundThreadParam ? "," : ");";
                    builder.AppendLine($"{canExecExpr}{suffix}");
                }

                if (hasExecuteOnBackgroundThreadParam)
                {
                    builder.AppendLine("executeOnBackgroundThread: true);");
                }

                builder.DecreaseIndent();
            }
            else
            {
                builder.AppendLine($"public {interfaceType}{generic} {propName} => {fieldName} ??= new {implementationType}{generic}({execExpr});");
            }

            if (i < commands.Length - 1)
            {
                builder.AppendLine();
            }
        }
    }

    private static void AppendCancelMethods(
        CommandBuilderBase builder,
        RelayCommandToGenerate[] commands)
    {
        var commandsWithCancellation = commands
            .Where(cmd => cmd.SupportsCancellation && cmd.UseTask)
            .ToArray();

        if (commandsWithCancellation.Length == 0)
        {
            return;
        }

        builder.AppendLine();

        for (var i = 0; i < commandsWithCancellation.Length; i++)
        {
            var cmd = commandsWithCancellation[i];
            var methodName = cmd.CommandName.Replace("Command", string.Empty);
            var propName = cmd.GetCommandAsPropertyName();

            builder.AppendLine($"public void Cancel{methodName}()");
            builder.AppendLine("{");
            builder.IncreaseIndent();
            builder.AppendLine($"{propName}.Cancel();");
            builder.DecreaseIndent();
            builder.AppendLine("}");

            if (i < commandsWithCancellation.Length - 1)
            {
                builder.AppendLine();
            }
        }
    }

    private static void AppendAutoSetIsBusyBlock(
        CommandBuilderBase builder,
        RelayCommandToGenerate cmd,
        string execExpr,
        bool hasCan)
    {
        var hasExecExprCancellationTokenNone = execExpr.Contains(NameConstants.CancellationTokenNone);

        var useAsyncLambda = hasExecExprCancellationTokenNone ||
                             cmd.GetParameterTypesWithoutCancellationToken().Length > 0 ||
                             execExpr.StartsWith("Task.Run(", StringComparison.Ordinal);

        var execExprContainParameters = execExpr.Contains("(x)") ||
                                        execExpr.Contains("(x, ") ||
                                        execExpr.Contains("(x.");

        var useAwait = false;
        var useDispatcherInvokeAsync = false;
        var useDispatcherInvoke = false;
        switch (cmd.UseTask)
        {
            case false:
                builder.AppendLine(
                    execExprContainParameters
                        ? "x =>"
                        : "() =>");
                useDispatcherInvoke = true;
                break;
            case true when execExprContainParameters:
                builder.AppendLine("async x =>");
                useAwait = true;
                useDispatcherInvokeAsync = true;
                break;
            case true when useAsyncLambda:
            case true when cmd.IsAsync:
                builder.AppendLine("async () =>");
                useAwait = true;
                useDispatcherInvokeAsync = true;
                break;
            case true when !execExprContainParameters ||
                           !execExpr.StartsWith("Task.Run(", StringComparison.Ordinal) ||
                           !execExpr.StartsWith("() => ", StringComparison.Ordinal):
                builder.AppendLine("async () =>");
                useDispatcherInvokeAsync = true;
                break;
        }

        builder.AppendLine("{");
        builder.IncreaseIndent();
        if (useDispatcherInvokeAsync)
        {
            builder.AppendLine("await Application.Current.Dispatcher");
            builder.IncreaseIndent();
            builder.AppendLine(".InvokeAsyncIfRequired(() => IsBusy = true)");
            builder.AppendLine(".ConfigureAwait(false);");
            builder.DecreaseIndent();
        }
        else if (useDispatcherInvoke)
        {
            builder.AppendLine("Application.Current.Dispatcher.InvokeIfRequired(() => IsBusy = true);");
        }
        else
        {
            builder.AppendLine("IsBusy = true;");
        }

        builder.AppendLine();
        builder.AppendLine("try");
        builder.AppendLine("{");
        builder.IncreaseIndent();

        if (cmd.UseTask)
        {
            if (useAwait &&
                !execExpr.StartsWith("Task.Run(", StringComparison.Ordinal) &&
                !execExpr.StartsWith("() => ", StringComparison.Ordinal))
            {
                if (execExpr.StartsWith("x => ", StringComparison.Ordinal))
                {
                    execExpr = execExpr.Replace("x => ", string.Empty);
                }

                builder.AppendLine(
                    execExpr.EndsWith(")", StringComparison.Ordinal)
                        ? $"await {execExpr}.ConfigureAwait(false);"
                        : $"await {execExpr}().ConfigureAwait(false);");
            }
            else
            {
                builder.AppendLine("await Task");
                builder.IncreaseIndent();
                builder.AppendLine($".Run({execExpr.RemoveTaskDotRun()})");
                builder.AppendLine(".ConfigureAwait(false);");
                builder.DecreaseIndent();
            }
        }
        else
        {
            if (execExpr.StartsWith("Task.Run(", StringComparison.Ordinal))
            {
                builder.AppendLine("_ = Task");
                builder.IncreaseIndent();
                builder.AppendLine($".Run({execExpr.RemoveTaskDotRun()})");
                builder.AppendLine(".ConfigureAwait(false);");
                builder.DecreaseIndent();
            }
            else
            {
                builder.AppendLine($"{execExpr};");
            }
        }

        builder.DecreaseIndent();
        builder.AppendLine("}");
        builder.AppendLine("finally");
        builder.AppendLine("{");
        builder.IncreaseIndent();
        if (useDispatcherInvokeAsync)
        {
            builder.AppendLine("await Application.Current.Dispatcher");
            builder.IncreaseIndent();
            builder.AppendLine(".InvokeAsyncIfRequired(() => IsBusy = false)");
            builder.AppendLine(".ConfigureAwait(false);");
            builder.DecreaseIndent();
        }
        else if (useDispatcherInvoke)
        {
            builder.AppendLine("Application.Current.Dispatcher.InvokeIfRequired(() => IsBusy = false);");
        }
        else
        {
            builder.AppendLine("IsBusy = false;");
        }

        builder.DecreaseIndent();
        builder.AppendLine("}");
        builder.DecreaseIndent();
        builder.AppendLine(hasCan ? "}," : "});");
    }

    private static string BuildExecuteExpression(
        RelayCommandToGenerate cmd)
    {
        return cmd.AutoSetIsBusy
            ? BuildExecuteExpressionForAutoSetIsBusy(cmd)
            : BuildExecuteExpressionForNoAutoSetIsBusy(cmd);
    }

    private static string BuildExecuteExpressionForNoAutoSetIsBusy(
        RelayCommandToGenerate cmd)
    {
        if (cmd.ParameterValues?.Length > 0)
        {
            var parameterValuesAsCommaSeparated = cmd.GetParameterValuesAsCommaSeparated();
            return cmd is { ExecuteOnBackgroundThread: true, UseTask: true }
                ? $"() => Task.Run(() => {cmd.MethodName}({parameterValuesAsCommaSeparated}))"
                : $"() => {cmd.MethodName}({parameterValuesAsCommaSeparated})";
        }

        var filteredParameterTypes = cmd.GetParameterTypesWithoutCancellationToken();
        var hasCt = cmd.HasParameterTypesOfCancellationToken();

        // When SupportsCancellation is true but the method doesn't have CancellationToken parameter,
        // we need to wrap it in a lambda that accepts the CancellationToken (but doesn't use it)
        if (cmd.SupportsCancellation && !hasCt)
        {
            if (filteredParameterTypes.Length == 0)
            {
                // async (ct) => await MethodAsync()
                return $"async (ct) => await {cmd.MethodName}()";
            }
            else if (filteredParameterTypes.Length == 1)
            {
                // async (param, ct) => await MethodAsync(param)
                return $"async (x, ct) => await {cmd.MethodName}(x)";
            }
            else
            {
                // async (tuple, ct) => await MethodAsync(tuple.Item1, tuple.Item2)
                var args = cmd.GetParameterTypesWithoutCancellationTokenAsItemNumberArgsAsCommaSeparated();
                return $"async (x, ct) => await {cmd.MethodName}({args})";
            }
        }

        if (filteredParameterTypes.Length > 0)
        {
            if (filteredParameterTypes.Length == 1)
            {
                // When SupportsCancellation = true and ExecuteOnBackgroundThread = true,
                // don't wrap in Task.Run - let the command handle it via the executeOnBackgroundThread parameter
                if (cmd.ExecuteOnBackgroundThread && !(cmd.SupportsCancellation && hasCt))
                {
                    var paramCall = hasCt
                        ? $"(x, {NameConstants.CancellationTokenNone})"
                        : "(x)";

                    return $"x => Task.Run(() => {cmd.MethodName}{paramCall})";
                }

                // When SupportsCancellation = true and method has CancellationToken, pass method with explicit cast
                if (cmd.SupportsCancellation && hasCt)
                {
                    var paramType = filteredParameterTypes[0];
                    return $"(System.Func<{paramType}, System.Threading.CancellationToken, System.Threading.Tasks.Task>){cmd.MethodName}";
                }

                return hasCt
                    ? $"x => {cmd.MethodName}(x, {NameConstants.CancellationTokenNone})"
                    : cmd.MethodName;
            }

            var args = cmd.GetParameterTypesWithoutCancellationTokenAsItemNumberArgsAsCommaSeparated();

            var call = hasCt
                ? $"({args}, {NameConstants.CancellationTokenNone})"
                : $"({args})";

            // When SupportsCancellation = true and ExecuteOnBackgroundThread = true,
            // don't wrap in Task.Run
            if (cmd.ExecuteOnBackgroundThread && !(cmd.SupportsCancellation && hasCt))
            {
                return $"Task.Run(() => {cmd.MethodName}{call})";
            }

            // When SupportsCancellation = true and method has CancellationToken
            if (cmd.SupportsCancellation && hasCt)
            {
                // Remove CancellationToken.None from the call
                var ctFreeCall = args is not null ? $"({args})" : "()";
                return $"x => {cmd.MethodName}{ctFreeCall}";
            }

            return $"x => {cmd.MethodName}{call}";
        }

        // When SupportsCancellation = true and ExecuteOnBackgroundThread = true,
        // don't wrap in Task.Run - let the command handle it
        if (cmd.ExecuteOnBackgroundThread && !(cmd.SupportsCancellation && hasCt))
        {
            return hasCt
                ? $"() => Task.Run(() => {cmd.MethodName}({NameConstants.CancellationTokenNone}))"
                : $"() => Task.Run({cmd.MethodName})";
        }

        // When SupportsCancellation = true and method has CancellationToken, pass method with explicit cast
        if (cmd.SupportsCancellation && hasCt)
        {
            return $"(System.Func<System.Threading.CancellationToken, System.Threading.Tasks.Task>){cmd.MethodName}";
        }

        return hasCt
            ? $"() => {cmd.MethodName}({NameConstants.CancellationTokenNone})"
            : cmd.MethodName;
    }

    private static string BuildExecuteExpressionForAutoSetIsBusy(
        RelayCommandToGenerate cmd)
    {
        if (cmd.ParameterValues?.Length > 0)
        {
            var parameterValues = cmd.GetParameterValuesAsCommaSeparated();
            return cmd is { ExecuteOnBackgroundThread: true }
                ? $"Task.Run(() => {cmd.MethodName}({parameterValues}))"
                : $"{cmd.MethodName}({parameterValues})";
        }

        var filteredParameterTypes = cmd.GetParameterTypesWithoutCancellationToken();
        var hasCt = cmd.HasParameterTypesOfCancellationToken();

        if (filteredParameterTypes.Length > 0)
        {
            if (filteredParameterTypes.Length == 1)
            {
                if (cmd.ExecuteOnBackgroundThread)
                {
                    var paramCall = hasCt
                        ? $"(x, {NameConstants.CancellationTokenNone})"
                        : "(x)";

                    return cmd.UseTask
                        ? $"() => {cmd.MethodName}{paramCall}"
                        : $"Task.Run(() => {cmd.MethodName}{paramCall})";
                }

                return hasCt
                    ? $"{cmd.MethodName}(x, {NameConstants.CancellationTokenNone})"
                    : $"{cmd.MethodName}(x)";
            }

            var args = cmd.GetParameterTypesWithoutCancellationTokenAsItemNumberArgsAsCommaSeparated();

            var call = hasCt
                ? $"({args}, {NameConstants.CancellationTokenNone})"
                : $"({args})";

            return cmd.ExecuteOnBackgroundThread
                ? $"Task.Run(() => {cmd.MethodName}{call})"
                : $"x => {cmd.MethodName}{call}";
        }

        if (cmd.ExecuteOnBackgroundThread)
        {
            return hasCt
                ? $"Task.Run(() => {cmd.MethodName}({NameConstants.CancellationTokenNone}))"
                : $"Task.Run({cmd.MethodName})";
        }

        if (cmd.UseTask)
        {
            return hasCt
                ? $"{cmd.MethodName}({NameConstants.CancellationTokenNone})"
                : cmd.MethodName;
        }

        return hasCt
            ? $"{cmd.MethodName}({NameConstants.CancellationTokenNone})"
            : $"{cmd.MethodName}()";
    }

    private static string? BuildCanExecuteExpression(
        RelayCommandToGenerate cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd.CanExecuteName))
        {
            return null;
        }

        if (cmd.ParameterValues?.Length > 0)
        {
            return $"{cmd.CanExecuteName}({cmd.GetParameterValuesAsCommaSeparated()})";
        }

        var types = cmd.ParameterTypes ?? [];
        var paramCount = types.Length;
        var isCancellationOnly = paramCount == 1 &&
                                 types[0].EndsWith(NameConstants.CancellationToken, StringComparison.Ordinal);

        var filteredParameterTypes = cmd.GetParameterTypesWithoutCancellationToken();

        string? expr;
        if (cmd.UsePropertyForCanExecute)
        {
            if (paramCount == 0 || isCancellationOnly)
            {
                expr = cmd.InvertCanExecute
                    ? $"() => !{cmd.CanExecuteName}"
                    : $"() => {cmd.CanExecuteName}";
            }
            else if (paramCount == 1)
            {
                expr = cmd.InvertCanExecute
                    ? $"_ => !{cmd.CanExecuteName}"
                    : $"_ => {cmd.CanExecuteName}";
            }
            else
            {
                expr = cmd.InvertCanExecute
                    ? $"x => !{cmd.CanExecuteName}"
                    : $"x => {cmd.CanExecuteName}";
            }
        }
        else
        {
            switch (filteredParameterTypes.Length)
            {
                case 1:
                    expr = cmd.InvertCanExecute
                        ? $"!{cmd.CanExecuteName}"
                        : cmd.CanExecuteName;
                    break;
                case > 1:
                {
                    var args = cmd.GetParameterTypesWithoutCancellationTokenAsItemNumberArgsAsCommaSeparated();

                    expr = cmd.InvertCanExecute
                        ? $"x => !{cmd.CanExecuteName}({args})"
                        : $"x => {cmd.CanExecuteName}({args})";
                    break;
                }

                default:
                    expr = cmd.InvertCanExecute
                        ? $"!{cmd.CanExecuteName}"
                        : cmd.CanExecuteName;
                    break;
            }
        }

        return expr;
    }
}