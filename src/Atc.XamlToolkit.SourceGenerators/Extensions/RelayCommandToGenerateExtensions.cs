namespace Atc.XamlToolkit.SourceGenerators.Extensions;

[SuppressMessage("Design", "MA0076:Do not use implicit culture-sensitive ToString in interpolated strings", Justification = "OK.")]
internal static class RelayCommandToGenerateExtensions
{
    public static bool HasParameterTypesOfCancellationToken(
        this RelayCommandToGenerate relayCommandToGenerate)
    {
        var parameterTypes = relayCommandToGenerate.ParameterTypes ?? [];
        return parameterTypes.Any(t => t.EndsWith(NameConstants.CancellationToken, StringComparison.Ordinal));
    }

    public static string GetInterfaceType(
        this RelayCommandToGenerate relayCommandToGenerate)
        => relayCommandToGenerate.UseTask
            ? NameConstants.IRelayCommandAsync
            : NameConstants.IRelayCommand;

    public static string GetImplementationType(
        this RelayCommandToGenerate relayCommandToGenerate)
        => relayCommandToGenerate.UseTask
            ? NameConstants.RelayCommandAsync
            : NameConstants.RelayCommand;

    public static string GetCommandAsPropertyName(
        this RelayCommandToGenerate relayCommandToGenerate)
        => char.ToUpperInvariant(relayCommandToGenerate.CommandName[0]) + relayCommandToGenerate.CommandName.Substring(1);

    public static string GetCommandAsFieldName(
        this RelayCommandToGenerate relayCommandToGenerate)
        => char.ToLowerInvariant(relayCommandToGenerate.CommandName[0]) + relayCommandToGenerate.CommandName.Substring(1);

    public static string GetGenericArgAsString(
        this RelayCommandToGenerate relayCommandToGenerate)
    {
        if (relayCommandToGenerate.ParameterValues?.Length > 0)
        {
            return string.Empty;
        }

        var filteredParameterTypes = relayCommandToGenerate.GetParameterTypesWithoutCancellationToken();

        return filteredParameterTypes.Length switch
        {
            0 => string.Empty,
            1 => $"<{filteredParameterTypes[0]}>",
            _ => $"<({string.Join(", ", filteredParameterTypes)})>",
        };
    }

    public static string[] GetParameterTypesWithoutCancellationToken(
        this RelayCommandToGenerate relayCommandToGenerate)
    {
        var parameterTypes = relayCommandToGenerate.ParameterTypes ?? [];
        return parameterTypes
            .Where(t => !t.EndsWith(NameConstants.CancellationToken, StringComparison.Ordinal))
            .ToArray();
    }

    public static string[] GetParameterNamesWithoutCancellationToken(
        this RelayCommandToGenerate relayCommandToGenerate)
    {
        var parameterTypes = relayCommandToGenerate.ParameterTypes ?? [];
        var parameterNames = relayCommandToGenerate.ParameterNames ?? [];

        var result = new List<string>();
        for (var i = 0; i < parameterTypes.Length; i++)
        {
            if (!parameterTypes[i].EndsWith(NameConstants.CancellationToken, StringComparison.Ordinal))
            {
                result.Add(parameterNames[i]);
            }
        }

        return result.ToArray();
    }

    public static string? GetParameterValuesAsCommaSeparated(
        this RelayCommandToGenerate relayCommandToGenerate)
        => relayCommandToGenerate.ParameterValues?.Length > 0
            ? string.Join(", ", relayCommandToGenerate.ParameterValues)
            : null;

    public static string? GetParameterTypesWithoutCancellationTokenAsItemNumberArgsAsCommaSeparated(
        this RelayCommandToGenerate relayCommandToGenerate)
    {
        var filteredParameterTypes = relayCommandToGenerate.GetParameterTypesWithoutCancellationToken();
        return filteredParameterTypes.Length > 0
            ? string.Join(", ", filteredParameterTypes.Select((_, i) => $"x.Item{i + 1}"))
            : null;
    }
}