// ReSharper disable ConvertIfStatementToSwitchStatement
namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class AttributeDataExtensions
{
    /// <summary>
    /// Gets the simple name of the attribute class when it is one of this toolkit's attributes.
    /// Other libraries use the same simple names (e.g. <c>ObservableProperty</c>,
    /// <c>RelayCommand</c>); their attributes return <c>null</c> so they never match.
    /// </summary>
    /// <param name="attributeData">The attribute data.</param>
    /// <returns>The attribute class name, or null for an attribute of another library.</returns>
    public static string? GetToolkitAttributeName(
        this AttributeData attributeData)
        => attributeData.AttributeClass is { } attributeClass &&
           attributeClass.IsToolkitTypeOrUnresolved()
            ? attributeClass.Name
            : null;

    /// <summary>
    /// Adds the namespaces that must be imported to copy the attribute as written:
    /// the namespace of the attribute class and of the types and enums used as arguments.
    /// </summary>
    /// <param name="attributeData">The attribute data.</param>
    /// <param name="namespaces">The set the namespaces are added to.</param>
    public static void AddRequiredNamespacesTo(
        this AttributeData attributeData,
        ISet<string> namespaces)
    {
        if (attributeData.AttributeClass is not { TypeKind: not TypeKind.Error } attributeClass)
        {
            return;
        }

        attributeClass.AddRequiredNamespacesTo(namespaces);

        foreach (var argument in attributeData.ConstructorArguments)
        {
            AddRequiredNamespacesTo(argument, namespaces);
        }

        foreach (var argument in attributeData.NamedArguments)
        {
            AddRequiredNamespacesTo(argument.Value, namespaces);
        }
    }

    private static void AddRequiredNamespacesTo(
        TypedConstant argument,
        ISet<string> namespaces)
    {
        switch (argument.Kind)
        {
            case TypedConstantKind.Array when !argument.IsNull:
                foreach (var element in argument.Values)
                {
                    AddRequiredNamespacesTo(element, namespaces);
                }

                break;
            case TypedConstantKind.Type when argument.Value is ITypeSymbol typeValue:
                typeValue.AddRequiredNamespacesTo(namespaces);
                break;
            case TypedConstantKind.Enum when argument.Type is not null:
                argument.Type.AddRequiredNamespacesTo(namespaces);
                break;
        }
    }

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
        ref object? defaultValue,
        out bool? isNullableValueType,
        ISet<string> requiredNamespaces)
    {
        isNullableValueType = null;

        var type = "int";
        if (propertyAttribute.AttributeClass is not { TypeArguments.Length: 1 })
        {
            return type;
        }

        var typeSymbol = propertyAttribute.AttributeClass.TypeArguments[0];
        isNullableValueType = typeSymbol.IsNullableValueTypeOrUnknown();
        typeSymbol.AddRequiredNamespacesTo(requiredNamespaces);
        type = typeSymbol
            .ToDisplayString()
            .EnsureCSharpAliasIfNeeded();

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

    /// <summary>
    /// Replaces the bound string and char values that are emitted inside a literal
    /// (Category, Description, and DefaultValue of a string or char property) with their
    /// escaped form, so the argument values have the same shape as the source text.
    /// </summary>
    /// <param name="attributeData">The attribute data the values were extracted from.</param>
    /// <param name="argumentValues">The extracted argument values to update.</param>
    /// <param name="propertyType">The type of the property the attribute describes.</param>
    public static void EscapeLiteralArgumentValues(
        this AttributeData attributeData,
        IDictionary<string, string?> argumentValues,
        string propertyType)
    {
        var type = propertyType.RemoveNullableSuffix();

        foreach (var namedArgument in attributeData.NamedArguments)
        {
            if (namedArgument.Value.Kind != TypedConstantKind.Primitive ||
                !argumentValues.ContainsKey(namedArgument.Key))
            {
                continue;
            }

            var isDefaultValue = namedArgument.Key == NameConstants.DefaultValue;

            string? literal = null;
            if (namedArgument.Value.Value is string stringValue &&
                (namedArgument.Key is NameConstants.Category or NameConstants.Description ||
                 (isDefaultValue && type == "string")))
            {
                literal = SymbolDisplay.FormatLiteral(stringValue, quote: true);
            }
            else if (namedArgument.Value.Value is char charValue &&
                     isDefaultValue &&
                     type == "char")
            {
                literal = SymbolDisplay.FormatLiteral(charValue, quote: true);
            }

            if (literal is not null)
            {
                // Drop the quotes - they are added again where the value is emitted.
                argumentValues[namedArgument.Key] = literal.Substring(1, literal.Length - 2);
            }
        }
    }

    // Numbers must not be formatted with the culture of the build machine.
    private static string? ToInvariantString(object? value)
        => value is IFormattable formattable
            ? formattable.ToString(format: null, CultureInfo.InvariantCulture)
            : value?.ToString();

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
                        ToInvariantString(typedConstant.Value));
                }
            }
            else if (arg.Value is not null)
            {
                result.Add(
                    NameConstants.Name,
                    ToInvariantString(arg.Value));
            }
        }

        // Cache the syntax fallback so it's computed at most once per attribute
        IDictionary<string, string?>? syntaxFallback = null;

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
                        result[arg.Key] += $", {ToInvariantString(typedConstant.Value)}";
                    }
                    else
                    {
                        result.Add(
                            arg.Key,
                            ToInvariantString(typedConstant.Value));
                    }
                }
            }
            else
            {
                if (arg.Value.Kind == TypedConstantKind.Enum)
                {
                    // Syntax check for enum - since arg.Value.Value will be an integer
                    syntaxFallback ??= attributeData.SyntaxExtractConstructorArgumentValues();
                    if (syntaxFallback.TryGetValue(arg.Key, out var value))
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
                        syntaxFallback ??= attributeData.SyntaxExtractConstructorArgumentValues();
                        if (syntaxFallback.TryGetValue(arg.Key, out var value))
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
                            ToInvariantString(arg.Value.Value));
                    }
                }
            }
        }

        return result;
    }
}