namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class TypeSymbolExtensions
{
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