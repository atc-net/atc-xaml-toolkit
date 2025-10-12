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

        var dtoTypeName = dtoTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var isRecord = dtoTypeSymbol.IsRecord;
        var hasCustomToString = HasCustomToString(dtoTypeSymbol);
        var properties = dtoTypeSymbol.ExtractProperties();
        var methods = dtoTypeSymbol.ExtractMethods();

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