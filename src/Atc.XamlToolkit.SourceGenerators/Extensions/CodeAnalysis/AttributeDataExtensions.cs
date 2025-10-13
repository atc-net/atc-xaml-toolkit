// ReSharper disable ConvertIfStatementToSwitchStatement
namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class AttributeDataExtensions
{
    public static IDictionary<string, string?> ExtractConstructorArgumentValues(
        this AttributeData attributeData)
    {
        if (attributeData.ConstructorArguments.Length == 0 &&
            attributeData.NamedArguments.Length == 0)
        {
            // Syntax check
            return attributeData.SyntaxExtractConstructorArgumentValues();
        }

        // Runtime check
        return attributeData.RunTimeExtractConstructorArgumentValues();
    }

    public static string ExtractClassFirstArgumentType(
        this AttributeData propertyAttribute,
        ref object? defaultValue)
    {
        var type = "int";
        if (propertyAttribute.AttributeClass is not { TypeArguments.Length: 1 })
        {
            return type;
        }

        var typeSymbol = propertyAttribute.AttributeClass.TypeArguments[0];
        type = typeSymbol.ToDisplayString().EnsureCSharpAliasIfNeeded();

        if (type == "bool")
        {
            defaultValue = "false";
        }

        return type;
    }

    /// <summary>
    /// Extracts the UseIsDirty value from an attribute, handling both runtime and syntax modes.
    /// </summary>
    /// <param name="attribute">The attribute data to extract from.</param>
    /// <param name="inheritFromViewModel">Whether the class inherits from ViewModelBase (which has IsDirty property).</param>
    /// <param name="defaultValue">The default value to use if UseIsDirty is not specified.</param>
    /// <returns>True if UseIsDirty should be enabled; otherwise false.</returns>
    public static bool ExtractUseIsDirtyValue(
        this AttributeData attribute,
        bool inheritFromViewModel,
        bool defaultValue = true)
    {
        if (!inheritFromViewModel)
        {
            return false;
        }

        // Try runtime mode first
        var useIsDirtyArg = attribute.NamedArguments.FirstOrDefault(na => na.Key == NameConstants.UseIsDirty);
        if (useIsDirtyArg is { Key: NameConstants.UseIsDirty, Value.Value: bool boolValue })
        {
            return boolValue;
        }

        // Fall back to syntax mode (unit test scenario)
        var argumentValues = attribute.ExtractConstructorArgumentValues();
        if (argumentValues.TryGetValue(NameConstants.UseIsDirty, out var useIsDirtyValue) &&
            bool.TryParse(useIsDirtyValue, out var parsedValue))
        {
            return parsedValue;
        }

        return defaultValue;
    }

    private static IDictionary<string, string?> SyntaxExtractConstructorArgumentValues(
        this AttributeData attributeData) =>
        attributeData.ApplicationSyntaxReference is not null
            ? attributeData
                .ApplicationSyntaxReference
                .GetSyntax()
                .ToFullString()
                .ExtractAttributeConstructorParameters()
            : new Dictionary<string, string?>(StringComparer.Ordinal);

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    [SuppressMessage("Design", "S1871:Either merge this branch with the identical", Justification = "OK.")]
    private static Dictionary<string, string?> RunTimeExtractConstructorArgumentValues(
        this AttributeData attributeData)
    {
        var result = new Dictionary<string, string?>(StringComparer.Ordinal);

        var arrayIndex = 0;
        foreach (var arg in attributeData.ConstructorArguments)
        {
            if (arg.Kind == TypedConstantKind.Array)
            {
                foreach (var typedConstant in arg.Values)
                {
                    if (typedConstant.Value is null)
                    {
                        continue;
                    }

                    arrayIndex++;
                    result.Add(
                        arrayIndex.ToString(CultureInfo.InvariantCulture),
                        typedConstant.Value.ToString());
                }
            }
            else if (arg.Value is not null)
            {
                result.Add(
                    NameConstants.Name,
                    arg.Value.ToString());
            }
        }

        foreach (var arg in attributeData.NamedArguments)
        {
            if (arg.Value.Kind == TypedConstantKind.Array)
            {
                foreach (var typedConstant in arg.Value.Values)
                {
                    if (typedConstant.Value is null)
                    {
                        continue;
                    }

                    if (result.ContainsKey(arg.Key))
                    {
                        result[arg.Key] += $", {typedConstant.Value}";
                    }
                    else
                    {
                        result.Add(
                            arg.Key,
                            typedConstant.Value.ToString());
                    }
                }
            }
            else
            {
                if (arg.Value.Kind == TypedConstantKind.Enum)
                {
                    // Syntax check for enum - since arg.Value.Value will be an integer
                    var dictionary = attributeData.SyntaxExtractConstructorArgumentValues();
                    if (dictionary is not null &&
                        dictionary.TryGetValue(arg.Key, out var value))
                    {
                        result.Add(
                            arg.Key,
                            value);
                    }
                }
                else
                {
                    if (arg is { Key: NameConstants.DefaultValue, Value.Kind: TypedConstantKind.Error })
                    {
                        var dictionary = attributeData.SyntaxExtractConstructorArgumentValues();
                        if (dictionary is not null &&
                            dictionary.TryGetValue(arg.Key, out var value))
                        {
                            result.Add(
                                arg.Key,
                                value);
                        }
                    }
                    else
                    {
                        result.Add(
                            arg.Key,
                            arg.Value.Value?.ToString());
                    }
                }
            }
        }

        return result;
    }
}