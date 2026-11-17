namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class TypeSymbolExtensions
{
    private const string ToolkitNamespace = "Atc.XamlToolkit";

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