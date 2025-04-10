namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class AttributeDataExtensions
{
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    public static IDictionary<string, string?> ExtractConstructorArgumentValues(
        this AttributeData attributeData)
    {
        if (attributeData.ConstructorArguments.Length == 0 &&
            attributeData.NamedArguments.Length == 0)
        {
            // Syntax check
            if (attributeData.ApplicationSyntaxReference is not null)
            {
                return attributeData
                    .ApplicationSyntaxReference
                    .GetSyntax()
                    .ToFullString()
                    .ExtractAttributeConstructorParameters();
            }
        }
        else
        {
            // Runtime check
            return RunTimeExtractConstructorArgumentValues(attributeData);
        }

        return new Dictionary<string, string?>(StringComparer.Ordinal);
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

    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static Dictionary<string, string?> RunTimeExtractConstructorArgumentValues(
        AttributeData attributeData)
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
                    if (attributeData.ApplicationSyntaxReference is null)
                    {
                        continue;
                    }

                    var dictionary = attributeData
                        .ApplicationSyntaxReference
                        .GetSyntax()
                        .ToFullString()
                        .ExtractAttributeConstructorParameters();

                    if (dictionary.TryGetValue(arg.Key, out var value))
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

        return result;
    }
}