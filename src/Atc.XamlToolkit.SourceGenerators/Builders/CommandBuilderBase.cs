// ReSharper disable InvertIf
// ReSharper disable StringLiteralTypo
// ReSharper disable MergeIntoPattern
namespace Atc.XamlToolkit.SourceGenerators.Builders;

[SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
internal abstract class CommandBuilderBase : BuilderBase
{
    public virtual void GenerateRelayCommands(
        CommandBuilderBase builder,
        IList<RelayCommandToGenerate>? relayCommandsToGenerate)
    {
        if (relayCommandsToGenerate is null || relayCommandsToGenerate.Count == 0)
        {
            return;
        }

        builder.AppendLineBeforeMember();

        AppendPrivateBackingFields(builder, relayCommandsToGenerate);

        builder.AppendLine();

        AppendPublicProperties(builder, relayCommandsToGenerate);
    }

    public virtual void GenerateRelayCommandMethods(
        CommandBuilderBase builder,
        IList<RelayCommandToGenerate>? relayCommandsToGenerate)
    {
        if (relayCommandsToGenerate is null || relayCommandsToGenerate.Count == 0)
        {
            return;
        }

        AppendCancelMethods(builder, relayCommandsToGenerate);

        AppendDisposeCommandsMethod(builder, relayCommandsToGenerate);
    }

    private static void AppendPrivateBackingFields(
        CommandBuilderBase builder,
        IList<RelayCommandToGenerate> commands)
    {
        foreach (var cmd in commands)
        {
            var interfaceType = cmd.GetInterfaceType();
            var generic = cmd.GetGenericArgAsString();
            var fieldName = cmd.GetCommandAsFieldName();

            builder.AppendLine($"private {interfaceType}{generic}? {fieldName};");

            // Also generate backing field for cancel command if supports cancellation
            if (cmd.SupportsCancellation && cmd.UseTask)
            {
                var cancelFieldName = cmd
                    .GetCommandAsFieldName()
                    .Replace("Command", "CancelCommand");
                builder.AppendLine($"private IRelayCommand? {cancelFieldName};");
            }
        }
    }

    private static void AppendPublicProperties(
        CommandBuilderBase builder,
        IList<RelayCommandToGenerate> commands)
    {
        for (var i = 0; i < commands.Count; i++)
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
            var appendAsMultiLine = hasCan ||
                                    cmd.ExecuteOnBackgroundThread ||
                                    cmd.AutoSetIsBusy;

            // For WinUI async commands, generate full properties with PropertyChanged subscription
            var needsPropertyChangedSubscription = builder.XamlPlatform == XamlPlatform.WinUI && cmd.UseTask;

            if (needsPropertyChangedSubscription)
            {
                // Generate full property with getter
                builder.AppendLine($"public {interfaceType}{generic} {propName}");
                builder.AppendLine("{");
                builder.IncreaseIndent();
                builder.AppendLine("get");
                builder.AppendLine("{");
                builder.IncreaseIndent();
                builder.AppendLine($"if ({fieldName} == null)");
                builder.AppendLine("{");
                builder.IncreaseIndent();

                // Create command
                if (appendAsMultiLine)
                {
                    builder.AppendLine($"{fieldName} = new {implementationType}{generic}(");
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
                        var suffix = hasCan ? "," : ");";
                        builder.AppendLine($"{execExpr}{suffix}");
                    }

                    if (hasCan)
                    {
                        builder.AppendLine($"{canExecExpr});");
                    }

                    builder.DecreaseIndent();
                }
                else
                {
                    builder.AppendLine($"{fieldName} = new {implementationType}{generic}({execExpr});");
                }

                // Subscribe to PropertyChanged for x:Bind support
                builder.AppendLine($"if ({fieldName} is INotifyPropertyChanged inpc)");
                builder.AppendLine("{");
                builder.IncreaseIndent();
                builder.AppendLine("inpc.PropertyChanged += (s, e) =>");
                builder.AppendLine("{");
                builder.IncreaseIndent();
                builder.AppendLine("if (e.PropertyName == \"IsExecuting\")");
                builder.AppendLine("{");
                builder.IncreaseIndent();
                builder.AppendLine($"OnPropertyChanged(nameof({propName}));");
                builder.DecreaseIndent();
                builder.AppendLine("}");
                builder.DecreaseIndent();
                builder.AppendLine("};");
                builder.DecreaseIndent();
                builder.AppendLine("}");

                builder.DecreaseIndent();
                builder.AppendLine("}");
                builder.AppendLine($"return {fieldName};");
                builder.DecreaseIndent();
                builder.AppendLine("}");
                builder.DecreaseIndent();
                builder.AppendLine("}");
            }
            else if (appendAsMultiLine)
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
                    var suffix = hasCan ? "," : ");";
                    builder.AppendLine($"{execExpr}{suffix}");
                }

                if (hasCan)
                {
                    builder.AppendLine($"{canExecExpr});");
                }

                builder.DecreaseIndent();
            }
            else
            {
                builder.AppendLine($"public {interfaceType}{generic} {propName} => {fieldName} ??= new {implementationType}{generic}({execExpr});");
            }

            // If this command supports cancellation, append the cancel command property immediately after
            if (cmd.SupportsCancellation && cmd.UseTask)
            {
                builder.AppendLine();
                var methodName = cmd.CommandName.Replace("Command", string.Empty);
                var cancelCommandPropName = propName.Replace("Command", "CancelCommand");
                var cancelCommandFieldName = fieldName.Replace("Command", "CancelCommand");
                var cancelMethodName = $"Cancel{methodName}";
                builder.AppendLine($"public IRelayCommand {cancelCommandPropName} => {cancelCommandFieldName} ??= new RelayCommand({cancelMethodName});");
            }

            if (i < commands.Count - 1)
            {
                builder.AppendLine();
            }
        }
    }

    private static void AppendCancelMethods(
        CommandBuilderBase builder,
        IList<RelayCommandToGenerate> commands)
    {
        var first = true;
        foreach (var cmd in commands)
        {
            if (!cmd.SupportsCancellation || !cmd.UseTask)
            {
                continue;
            }

            if (first)
            {
                builder.AppendLine();
                first = false;
            }
            else
            {
                builder.AppendLine();
            }

            var methodName = cmd.CommandName.Replace("Command", string.Empty);
            var propName = cmd.GetCommandAsPropertyName();
            var cancelMethodName = $"Cancel{methodName}";

            // Generate the cancel method
            builder.AppendLine($"public void {cancelMethodName}()");
            builder.AppendLine("{");
            builder.IncreaseIndent();
            builder.AppendLine($"{propName}.Cancel();");
            builder.DecreaseIndent();
            builder.AppendLine("}");
        }
    }

    private static void AppendDisposeCommandsMethod(
        CommandBuilderBase builder,
        IList<RelayCommandToGenerate> commands)
    {
        // All async commands (IRelayCommandAsync) implement IDisposable,
        // so we need to generate DisposeCommands() for all of them
        var hasAsyncCommands = false;
        foreach (var cmd in commands)
        {
            if (cmd.UseTask)
            {
                hasAsyncCommands = true;
                break;
            }
        }

        if (!hasAsyncCommands)
        {
            return;
        }

        builder.AppendLine();

        builder.AppendLine("public void DisposeCommands()");
        builder.AppendLine("{");
        builder.IncreaseIndent();

        foreach (var cmd in commands)
        {
            if (!cmd.UseTask)
            {
                continue;
            }

            var propName = cmd.GetCommandAsPropertyName();
            builder.AppendLine($"{propName}.Dispose();");
        }

        builder.DecreaseIndent();
        builder.AppendLine("}");
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
        var hasCancellationToken = cmd.HasParameterTypesOfCancellationToken();
        var supportsCancellation = cmd.SupportsCancellation && hasCancellationToken;
        var parameterNamesWithoutCt = cmd.GetParameterNamesWithoutCancellationToken();

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
            case true when supportsCancellation && parameterNamesWithoutCt.Length > 0:
                // When AutoSetIsBusy + SupportsCancellation with parameters,
                // the lambda must accept the parameters AND CancellationToken
                var paramList = string.Join(", ", parameterNamesWithoutCt);
                builder.AppendLine($"async ({paramList}, cancellationToken) =>");
                useAwait = true;
                useDispatcherInvokeAsync = true;
                break;
            case true when supportsCancellation:
                // When AutoSetIsBusy + SupportsCancellation without parameters,
                // the lambda must accept CancellationToken only
                builder.AppendLine("async (CancellationToken cancellationToken) =>");
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

        // For WinUI with dispatcher invocation, capture the DispatcherQueue once at the start
        string? capturedDispatcherVar = null;
        if ((useDispatcherInvokeAsync || useDispatcherInvoke) && builder.XamlPlatform == XamlPlatform.WinUI)
        {
            capturedDispatcherVar = builder.GetUniqueVariableName("dispatcherQueue");
            builder.AppendLine($"var {capturedDispatcherVar} = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();");
            builder.AppendLine();
        }

        if (useDispatcherInvokeAsync)
        {
            AppendDispatcherInvokeAsync(builder, "IsBusy = true", capturedDispatcherVar);
        }
        else if (useDispatcherInvoke)
        {
            AppendDispatcherInvoke(builder, "IsBusy = true", capturedDispatcherVar);
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
            AppendDispatcherInvokeAsync(builder, "IsBusy = false", capturedDispatcherVar);
        }
        else if (useDispatcherInvoke)
        {
            AppendDispatcherInvoke(builder, "IsBusy = false", capturedDispatcherVar);
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

    private static void AppendDispatcherInvokeAsync(
        CommandBuilderBase builder,
        string action,
        string? capturedDispatcherVar = null)
    {
        switch (builder.XamlPlatform)
        {
            case XamlPlatform.Wpf:
                builder.AppendLine("await Application.Current.Dispatcher");
                builder.IncreaseIndent();
                builder.AppendLine($".InvokeAsyncIfRequired(() => {action})");
                builder.AppendLine(".ConfigureAwait(false);");
                builder.DecreaseIndent();
                break;
            case XamlPlatform.WinUI:
                // Use the captured dispatcher variable if provided, otherwise get a new one
                var varName = capturedDispatcherVar;
                if (varName is null)
                {
                    varName = builder.GetUniqueVariableName("dispatcherQueue");
                    builder.AppendLine($"var {varName} = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();");
                }

                builder.AppendLine($"if ({varName} is not null)");
                builder.AppendLine("{");
                builder.IncreaseIndent();
                builder.AppendLine($"await {varName}");
                builder.IncreaseIndent();
                builder.AppendLine($".InvokeAsyncIfRequired(() => {action})");
                builder.AppendLine(".ConfigureAwait(false);");
                builder.DecreaseIndent();
                builder.DecreaseIndent();
                builder.AppendLine("}");
                builder.AppendLine("else");
                builder.AppendLine("{");
                builder.IncreaseIndent();
                builder.AppendLine($"{action};");
                builder.DecreaseIndent();
                builder.AppendLine("}");
                break;
            case XamlPlatform.Avalonia:
                builder.AppendLine("await Avalonia.Threading.Dispatcher.UIThread");
                builder.IncreaseIndent();
                builder.AppendLine($".InvokeAsync(() => {action});");
                builder.DecreaseIndent();
                break;
        }
    }

    private static void AppendDispatcherInvoke(
        CommandBuilderBase builder,
        string action,
        string? capturedDispatcherVar = null)
    {
        switch (builder.XamlPlatform)
        {
            case XamlPlatform.Wpf:
                builder.AppendLine($"Application.Current.Dispatcher.InvokeIfRequired(() => {action});");
                break;
            case XamlPlatform.WinUI:
                // Use the captured dispatcher variable if provided, otherwise get a new one
                var varName = capturedDispatcherVar;
                if (varName is null)
                {
                    varName = builder.GetUniqueVariableName("dispatcherQueue");
                    builder.AppendLine($"var {varName} = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();");
                }

                builder.AppendLine($"if ({varName} is not null)");
                builder.AppendLine("{");
                builder.IncreaseIndent();
                builder.AppendLine($"{varName}.InvokeIfRequired(() => {action});");
                builder.DecreaseIndent();
                builder.AppendLine("}");
                builder.AppendLine("else");
                builder.AppendLine("{");
                builder.IncreaseIndent();
                builder.AppendLine($"{action};");
                builder.DecreaseIndent();
                builder.AppendLine("}");
                break;
            case XamlPlatform.Avalonia:
                builder.AppendLine($"Avalonia.Threading.Dispatcher.UIThread.Invoke(() => {action});");
                break;
        }
    }

    private static string BuildExecuteExpression(RelayCommandToGenerate cmd)
        => cmd.AutoSetIsBusy
            ? BuildExecuteExpressionForAutoSetIsBusy(cmd)
            : BuildExecuteExpressionForNoAutoSetIsBusy(cmd);

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

                // When both AutoSetIsBusy and SupportsCancellation are true,
                // the lambda will provide the actual parameter names, so use them
                if (cmd.SupportsCancellation && hasCt)
                {
                    var paramNames = cmd.GetParameterNamesWithoutCancellationToken();
                    return $"{cmd.MethodName}({paramNames[0]}, cancellationToken)";
                }

                return hasCt
                    ? $"{cmd.MethodName}(x, {NameConstants.CancellationTokenNone})"
                    : $"{cmd.MethodName}(x)";
            }

            // When both AutoSetIsBusy and SupportsCancellation are true with multiple parameters,
            // the lambda will provide the actual parameter names, so use them
            if (cmd.SupportsCancellation && hasCt)
            {
                var paramNames = cmd.GetParameterNamesWithoutCancellationToken();
                var paramList = string.Join(", ", paramNames);
                return $"{cmd.MethodName}({paramList}, cancellationToken)";
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
            // When both AutoSetIsBusy and SupportsCancellation are true,
            // the wrapper lambda will accept CancellationToken and pass it through
            return cmd.SupportsCancellation && hasCt
                ? $"{cmd.MethodName}(cancellationToken)"
                : hasCt
                    ? $"{cmd.MethodName}({NameConstants.CancellationTokenNone})"
                    : cmd.MethodName;
        }

        return cmd.SupportsCancellation && hasCt
            ? $"{cmd.MethodName}(cancellationToken)"
            : hasCt
                ? $"{cmd.MethodName}({NameConstants.CancellationTokenNone})"
                : $"{cmd.MethodName}()";
    }

    private static string? BuildCanExecuteExpression(RelayCommandToGenerate cmd)
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