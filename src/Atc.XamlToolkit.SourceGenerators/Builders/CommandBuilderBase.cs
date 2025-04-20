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
    }

    private static void AppendPrivateBackingFields(
        CommandBuilderBase builder,
        RelayCommandToGenerate[] commands)
    {
        foreach (var cmd in commands)
        {
            var interfaceType = cmd.IsAsync
                ? NameConstants.IRelayCommandAsync
                : NameConstants.IRelayCommand;
            var generic = GetGenericArg(cmd);
            var fieldName = ToFieldName(cmd.CommandName);
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
            var interfaceType = cmd.IsAsync
                ? NameConstants.IRelayCommandAsync
                : NameConstants.IRelayCommand;
            var implementationType = cmd.IsAsync
                ? NameConstants.RelayCommandAsync
                : NameConstants.RelayCommand;
            var generic = GetGenericArg(cmd);
            var propName = cmd.CommandName;
            var fieldName = ToFieldName(propName);
            var execExpr = BuildExecuteExpression(cmd);
            var canExecExpr = BuildCanExecuteExpression(cmd);
            var hasCan = canExecExpr != null;

            if (hasCan || cmd.ExecuteOnBackgroundThread)
            {
                // Multi-line
                builder.AppendLine($"public {interfaceType}{generic} {propName} => {fieldName} ??= new {implementationType}{generic}(");
                builder.IncreaseIndent();

                if (hasCan)
                {
                    builder.AppendLine($"{execExpr},");
                    builder.AppendLine($"{canExecExpr});");
                }
                else
                {
                    builder.AppendLine($"{execExpr});");
                }

                builder.DecreaseIndent();
            }
            else
            {
                // Single-line no-can
                builder.AppendLine($"public {interfaceType}{generic} {propName} => {fieldName} ??= new {implementationType}{generic}({execExpr});");
            }

            if (i < commands.Length - 1)
            {
                builder.AppendLine();
            }
        }
    }

    private static string ToFieldName(
        string commandName)
        => char.ToLowerInvariant(commandName[0]) + commandName.Substring(1);

    private static string GetGenericArg(
        RelayCommandToGenerate cmd)
    {
        if (cmd.ParameterValues?.Length > 0)
        {
            return string.Empty;
        }

        var types = cmd.ParameterTypes ?? [];
        var real = types
            .Where(t => !t.EndsWith(NameConstants.CancellationToken, StringComparison.Ordinal))
            .ToArray();

        return real.Length switch
        {
            0 => string.Empty,
            1 => $"<{real[0]}>",
            _ => $"<({string.Join(", ", real)})>",
        };
    }

    private static string BuildExecuteExpression(
        RelayCommandToGenerate cmd)
    {
        var types = cmd.ParameterTypes ?? [];
        var hasCt = types.Any(t => t.EndsWith(NameConstants.CancellationToken, StringComparison.Ordinal));
        var realTypes = types
            .Where(t => !t.EndsWith(NameConstants.CancellationToken, StringComparison.Ordinal))
            .ToArray();

        if (cmd.ParameterValues?.Length > 0)
        {
            var parameterValues = string.Join(", ", cmd.ParameterValues);
            return cmd is { ExecuteOnBackgroundThread: true, IsAsync: true }
                ? $"() => Task.Run(() => {cmd.MethodName}({parameterValues}))"
                : $"() => {cmd.MethodName}({parameterValues})";
        }

        if (realTypes.Length > 0)
        {
            if (realTypes.Length == 1)
            {
                if (cmd.ExecuteOnBackgroundThread)
                {
                    var paramCall = hasCt
                        ? $"(x, {NameConstants.CancellationTokenNone})"
                        : "(x)";

                    return $"x => Task.Run(() => {cmd.MethodName}{paramCall})";
                }

                return hasCt
                    ? $"x => {cmd.MethodName}(x, {NameConstants.CancellationTokenNone})"
                    : cmd.MethodName;
            }

            var args = string.Join(", ", realTypes.Select((_, i) => $"x.Item{i + 1}"));

            var call = hasCt
                ? $"({args}, {NameConstants.CancellationTokenNone})"
                : $"({args})";

            return cmd.ExecuteOnBackgroundThread
                ? $"x => Task.Run(() => {cmd.MethodName}{call})"
                : $"x => {cmd.MethodName}{call}";
        }

        if (cmd.ExecuteOnBackgroundThread)
        {
            return hasCt
                ? $"() => Task.Run(() => {cmd.MethodName}({NameConstants.CancellationTokenNone}))"
                : $"() => Task.Run({cmd.MethodName})";
        }

        return hasCt ?
            $"() => {cmd.MethodName}({NameConstants.CancellationTokenNone})"
            : cmd.MethodName;
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
            var parameterValues = string.Join(", ", cmd.ParameterValues);
            return $"{cmd.CanExecuteName}({parameterValues})";
        }

        var types = cmd.ParameterTypes ?? [];
        var paramCount = types.Length;
        var isCancellationOnly = paramCount == 1 &&
                                 types[0].EndsWith(NameConstants.CancellationToken, StringComparison.Ordinal);
        var realTypes = types
            .Where(t => !t.EndsWith(NameConstants.CancellationToken, StringComparison.Ordinal))
            .ToArray();

        string? expr;
        if (cmd.UsePropertyForCanExecute)
        {
            if (paramCount == 0 || isCancellationOnly)
            {
                expr = $"() => {cmd.CanExecuteName}";
            }
            else if (paramCount == 1)
            {
                expr = "_ => " + cmd.CanExecuteName;
            }
            else
            {
                expr = $"x => {cmd.CanExecuteName}";
            }
        }
        else
        {
            switch (realTypes.Length)
            {
                case 1:
                    expr = cmd.CanExecuteName;
                    break;
                case > 1:
                {
                    var args = string.Join(", ", realTypes.Select((_, i) => $"x.Item{i + 1}"));
                    expr = $"x => {cmd.CanExecuteName}({args})";
                    break;
                }

                default:
                    expr = cmd.CanExecuteName;
                    break;
            }
        }

        if (cmd.InvertCanExecute)
        {
            expr = "!" + expr;
        }

        return expr;
    }
}