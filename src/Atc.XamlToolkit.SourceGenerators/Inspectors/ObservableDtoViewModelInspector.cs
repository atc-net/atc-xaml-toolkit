// ReSharper disable InvertIf
namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

internal static class ObservableDtoViewModelInspector
{
    internal static ObservableDtoViewModelInspectorResult Inspect(
        Compilation compilation,
        INamedTypeSymbol viewModelClassSymbol,
        bool inheritFromViewModel)
    {
        var attribute = viewModelClassSymbol
            .GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name
                is NameConstants.ObservableDtoViewModelAttribute
                or NameConstants.ObservableDtoViewModel);

        if (attribute is null)
        {
            return new ObservableDtoViewModelInspectorResult(null, false, false, true, null, null);
        }

        INamedTypeSymbol? dtoTypeSymbol = null;

        // Try runtime mode first: ConstructorArguments will have the actual INamedTypeSymbol
        if (attribute.ConstructorArguments.Length > 0)
        {
            var firstArg = attribute.ConstructorArguments[0];
            if (firstArg is { Kind: TypedConstantKind.Type, Value: INamedTypeSymbol namedType })
            {
                dtoTypeSymbol = namedType;
            }
            else if (firstArg.Value is INamedTypeSymbol namedTypeSymbol)
            {
                dtoTypeSymbol = namedTypeSymbol;
            }
        }

        // If runtime mode didn't work (unit test scenario), try syntax mode
        dtoTypeSymbol ??= ExtractDtoTypeFromSyntax(compilation, attribute);

        if (dtoTypeSymbol is null)
        {
            return new ObservableDtoViewModelInspectorResult(
                dtoTypeName: null,
                isDtoRecord: false,
                hasCustomToString: false,
                useIsDirty: true,
                properties: null,
                methods: null);
        }

        var useIsDirty = ExtractUseIsDirtyValue(attribute, inheritFromViewModel);
        var ignoreProperties = ExtractIgnorePropertiesValue(attribute);
        var ignoreMethods = ExtractIgnoreMethodsValue(attribute);

        var dtoTypeName = dtoTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var isRecord = dtoTypeSymbol.IsRecord;
        var hasCustomToString = HasCustomToString(dtoTypeSymbol);
        var properties = dtoTypeSymbol.ExtractProperties(ignoreProperties);
        var methods = dtoTypeSymbol.ExtractMethods(ignoreMethods);

        return new ObservableDtoViewModelInspectorResult(
            dtoTypeName,
            isRecord,
            hasCustomToString,
            useIsDirty,
            properties,
            methods);
    }

    private static INamedTypeSymbol? ExtractDtoTypeFromSyntax(
        Compilation compilation,
        AttributeData attribute)
    {
        var argumentValues = attribute.ExtractConstructorArgumentValues();
        if (!argumentValues.TryGetValue(NameConstants.DtoType, out var typeofValue) &&
            !argumentValues.TryGetValue(NameConstants.Name, out typeofValue))
        {
            return null;
        }

        var typeName = ExtractTypeNameFromTypeofExpression(typeofValue);
        return !string.IsNullOrWhiteSpace(typeName)
            ? LookupTypeInCompilation(compilation, typeName)
            : null;
    }

    private static string? ExtractTypeNameFromTypeofExpression(
        string? typeofValue)
    {
        if (string.IsNullOrWhiteSpace(typeofValue))
        {
            return null;
        }

        // Handle "typeof(Person)" format
        if (typeofValue!.StartsWith("typeof(", StringComparison.Ordinal) &&
            typeofValue.EndsWith(")", StringComparison.Ordinal))
        {
            return typeofValue.Substring(7, typeofValue.Length - 8).Trim();
        }

        // If it's just "Person", return as-is
        return typeofValue;
    }

    private static INamedTypeSymbol? LookupTypeInCompilation(
        Compilation compilation,
        string? typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return null;
        }

        // Try to get the type symbol from the compilation
        // First try as fully qualified name
        var typeSymbol = compilation.GetTypeByMetadataName(typeName!);
        if (typeSymbol is not null)
        {
            return typeSymbol;
        }

        // If that doesn't work, search through all assemblies
        // This handles cases where the type is in the source being compiled
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);
            var root = syntaxTree.GetRoot();

            // Look for class declarations with matching name
            var classDeclaration = root
                .DescendantNodes()
                .OfType<ClassDeclarationSyntax>()
                .FirstOrDefault(c => c.Identifier.Text == typeName);

            if (classDeclaration is not null)
            {
                var symbol = semanticModel.GetDeclaredSymbol(classDeclaration);
                if (symbol is not null)
                {
                    return symbol;
                }
            }

            // Look for record declarations with matching name
            var recordDeclaration = root
                .DescendantNodes()
                .OfType<RecordDeclarationSyntax>()
                .FirstOrDefault(r => r.Identifier.Text == typeName);

            if (recordDeclaration is not null)
            {
                var symbol = semanticModel.GetDeclaredSymbol(recordDeclaration);
                if (symbol is not null)
                {
                    return symbol;
                }
            }
        }

        return null;
    }

    private static bool ExtractUseIsDirtyValue(
        AttributeData attribute,
        bool inheritFromViewModel)
    {
        if (!inheritFromViewModel)
        {
            return inheritFromViewModel;
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

        return inheritFromViewModel;
    }

    private static List<string> ExtractIgnorePropertiesValue(
        AttributeData attribute)
    {
        // Try runtime mode first
        var ignorePropertiesArg = attribute.NamedArguments.FirstOrDefault(na => na.Key == NameConstants.IgnoreProperties);
        if (ignorePropertiesArg is { Key: NameConstants.IgnoreProperties, Value.Kind: TypedConstantKind.Array })
        {
            return ignorePropertiesArg.Value.Values
                .Where(v => v.Value is string)
                .Select(v => (string)v.Value!)
                .ToList();
        }

        // Fall back to syntax mode (unit test scenario)
        var argumentValues = attribute.ExtractConstructorArgumentValues();
        if (argumentValues.TryGetValue(NameConstants.IgnoreProperties, out var ignorePropertiesValue) &&
            ignorePropertiesValue is not null)
        {
            // Parse array syntax like "[nameof(Person.Age)]" or "[\"Age\"]"
            return ParseArrayString(ignorePropertiesValue);
        }

        return [];
    }

    private static List<string> ExtractIgnoreMethodsValue(
        AttributeData attribute)
    {
        // Try runtime mode first
        var ignoreMethodsArg = attribute.NamedArguments.FirstOrDefault(na => na.Key == NameConstants.IgnoreMethods);
        if (ignoreMethodsArg is { Key: NameConstants.IgnoreMethods, Value.Kind: TypedConstantKind.Array })
        {
            return ignoreMethodsArg.Value.Values
                .Where(v => v.Value is string)
                .Select(v => (string)v.Value!)
                .ToList();
        }

        // Fall back to syntax mode (unit test scenario)
        var argumentValues = attribute.ExtractConstructorArgumentValues();
        if (argumentValues.TryGetValue(NameConstants.IgnoreMethods, out var ignoreMethodsValue) &&
            ignoreMethodsValue is not null)
        {
            // Parse array syntax like "[nameof(Person.GetFullName)]" or "[\"GetFullName\"]"
            return ParseArrayString(ignoreMethodsValue);
        }

        return [];
    }

    private static List<string> ParseArrayString(string arrayString)
    {
        if (string.IsNullOrWhiteSpace(arrayString))
        {
            return [];
        }

        var result = new List<string>();

        // Remove brackets and split by comma
        var cleaned = arrayString.Trim('[', ']', ' ');
        if (string.IsNullOrWhiteSpace(cleaned))
        {
            return [];
        }

        var parts = cleaned.Split(',');
        foreach (var part in parts)
        {
            var trimmed = part.Trim();

            // Handle nameof(Type.Property) or nameof(Type.Method)
            if (trimmed.StartsWith("nameof(", StringComparison.Ordinal) &&
                trimmed.EndsWith(")", StringComparison.Ordinal))
            {
                var nameofContent = trimmed.Substring(7, trimmed.Length - 8);

                // Extract the last part after the dot
                var lastDot = nameofContent.LastIndexOf('.');
                var propertyName = lastDot >= 0
                    ? nameofContent.Substring(lastDot + 1)
                    : nameofContent;
                result.Add(propertyName.Trim());
            }

            // Handle direct string literals like "Age" or "GetFullName"
            else if (trimmed.StartsWith("\"", StringComparison.Ordinal) &&
                     trimmed.EndsWith("\"", StringComparison.Ordinal))
            {
                result.Add(trimmed.Trim('"'));
            }
            else
            {
                // Just add as-is if no special format
                result.Add(trimmed);
            }
        }

        return result;
    }

    private static bool HasCustomToString(
        INamedTypeSymbol typeSymbol)
    {
        // Check if the type has a ToString method override
        var toStringMethod = typeSymbol
            .GetMembers("ToString")
            .OfType<IMethodSymbol>()
            .FirstOrDefault(m => m.Parameters.Length == 0 &&
                                m.ReturnType.SpecialType == SpecialType.System_String);

        if (toStringMethod is null)
        {
            return false;
        }

        // If the method is NOT declared in the current type (inherited), it's not custom
        if (!SymbolEqualityComparer.Default.Equals(toStringMethod.ContainingType, typeSymbol))
        {
            return false;
        }

        // Check if the method is explicitly declared (not compiler-generated, e.g., records)
        return !toStringMethod.IsImplicitlyDeclared;
    }
}