// ReSharper disable InvertIf
namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

internal static class ObservableDtoViewModelInspector
{
    internal static ObservableDtoViewModelInspectorResult Inspect(
        Compilation compilation,
        INamedTypeSymbol viewModelClassSymbol)
    {
        var attribute = viewModelClassSymbol
            .GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name
                is NameConstants.ObservableDtoViewModelAttribute
                or NameConstants.ObservableDtoViewModel);

        if (attribute is null)
        {
            return new ObservableDtoViewModelInspectorResult(null, false, null);
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
            return new ObservableDtoViewModelInspectorResult(null, false, null);
        }

        var dtoTypeName = dtoTypeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var isRecord = dtoTypeSymbol.IsRecord;
        var properties = dtoTypeSymbol.ExtractProperties();

        return new ObservableDtoViewModelInspectorResult(
            dtoTypeName,
            isRecord,
            properties);
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
}