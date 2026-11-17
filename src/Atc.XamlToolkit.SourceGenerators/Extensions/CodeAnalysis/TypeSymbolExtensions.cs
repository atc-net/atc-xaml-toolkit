namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class TypeSymbolExtensions
{
    private const string ToolkitNamespace = "Atc.XamlToolkit";

    // The type as it is written in generated code: C# keywords for the built-in types,
    // no namespace (the generated file imports it), but containing types kept.
    private static readonly SymbolDisplayFormat GeneratedCodeTypeNameFormat = new(
        globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
        typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypes,
        genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
        miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes | SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    /// <summary>
    /// Gets the name of the type as it should be written in generated code.
    /// </summary>
    /// <param name="typeSymbol">The type.</param>
    /// <returns>The type name without namespace, including containing types, type arguments and array ranks.</returns>
    public static string ToGeneratedCodeTypeName(this ITypeSymbol typeSymbol)
        => typeSymbol.TypeKind == TypeKind.Error
            ? typeSymbol.ToDisplayString().EnsureCSharpAliasIfNeeded() // unresolved: only the written name is known
            : typeSymbol.ToDisplayString(GeneratedCodeTypeNameFormat);

    /// <summary>
    /// Adds the namespaces that must be imported to write the type by its simple name:
    /// the namespace of the type itself, of its type arguments and of an array's element type.
    /// </summary>
    /// <param name="typeSymbol">The type to inspect.</param>
    /// <param name="namespaces">The set the namespaces are added to.</param>
    public static void AddRequiredNamespacesTo(
        this ITypeSymbol typeSymbol,
        ISet<string> namespaces)
    {
        switch (typeSymbol)
        {
            case IArrayTypeSymbol arrayType:
                arrayType.ElementType.AddRequiredNamespacesTo(namespaces);
                return;
            case INamedTypeSymbol namedType:
                foreach (var typeArgument in namedType.TypeArguments)
                {
                    typeArgument.AddRequiredNamespacesTo(namespaces);
                }

                break;
        }

        // Keyword types (int, string, T?, ...) and unresolved types need no import.
        if (typeSymbol.TypeKind is TypeKind.Error or TypeKind.TypeParameter ||
            typeSymbol.OriginalDefinition.SpecialType != SpecialType.None ||
            typeSymbol.ContainingNamespace is not { IsGlobalNamespace: false } containingNamespace)
        {
            return;
        }

        namespaces.Add(containingNamespace.ToDisplayString());
    }

    /// <summary>
    /// Determines whether the type is declared in an <c>Atc.XamlToolkit</c> namespace.
    /// A type that cannot be resolved is accepted, because only its simple name is known.
    /// </summary>
    /// <param name="typeSymbol">The type to check.</param>
    /// <returns>False for a resolved type of another library; otherwise true.</returns>
    public static bool IsToolkitTypeOrUnresolved(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeKind == TypeKind.Error ||
            typeSymbol.ContainingNamespace is null)
        {
            return true;
        }

        var namespaceName = typeSymbol.ContainingNamespace.ToDisplayString();

        return namespaceName.Equals(ToolkitNamespace, StringComparison.Ordinal) ||
               namespaceName.StartsWith(ToolkitNamespace + ".", StringComparison.Ordinal);
    }

    /// <summary>
    /// Tells whether the type is a <c>Nullable&lt;T&gt;</c> value type.
    /// Returns <c>null</c> when the type cannot be resolved, so the caller has to guess.
    /// </summary>
    /// <param name="typeSymbol">The type to check.</param>
    /// <returns>True for <c>Nullable&lt;T&gt;</c>, false for other resolved types, null when unresolved.</returns>
    public static bool? IsNullableValueTypeOrUnknown(
        this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeKind == TypeKind.Error)
        {
            return null;
        }

        if (typeSymbol.OriginalDefinition.SpecialType != SpecialType.System_Nullable_T)
        {
            return false;
        }

        if (typeSymbol is INamedTypeSymbol { TypeArguments.Length: 1 } nullableType &&
            nullableType.TypeArguments[0].TypeKind != TypeKind.Error)
        {
            return true;
        }

        return null;
    }

    /// <summary>
    /// Determines whether two values of the type can be compared with the <c>==</c> operator
    /// in generated code. Structs without a user-defined equality operator and unconstrained
    /// type parameters cannot.
    /// </summary>
    /// <param name="typeSymbol">The type to check.</param>
    /// <returns><see langword="true"/> if <c>==</c> compiles for the type; otherwise <see langword="false"/>.</returns>
    public static bool SupportsEqualityOperator(this ITypeSymbol typeSymbol)
    {
        if (typeSymbol.TypeKind == TypeKind.TypeParameter)
        {
            return false;
        }

        if (!typeSymbol.IsValueType)
        {
            return true;
        }

        if (typeSymbol is INamedTypeSymbol { OriginalDefinition.SpecialType: SpecialType.System_Nullable_T } nullableType)
        {
            return nullableType.TypeArguments[0].SupportsEqualityOperator();
        }

        if (typeSymbol.SpecialType != SpecialType.None ||
            typeSymbol.TypeKind == TypeKind.Enum)
        {
            return true;
        }

        if (typeSymbol is INamedTypeSymbol { IsTupleType: true } tupleType)
        {
            return tupleType.TupleElements.All(x => x.Type.SupportsEqualityOperator());
        }

        return typeSymbol
            .GetMembers(WellKnownMemberNames.EqualityOperatorName)
            .OfType<IMethodSymbol>()
            .Any();
    }
}