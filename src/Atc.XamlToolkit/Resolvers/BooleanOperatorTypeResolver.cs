// ReSharper disable GrammarMistakeInComment
namespace Atc.XamlToolkit.Resolvers;

/// <summary>
/// Resolver methods for working with <see cref="BooleanOperatorType"/> values.
/// </summary>
/// <remarks>
/// <para>
/// The main entry point, <see cref="Resolve"/>, converts an arbitrary
/// <see langword="object" /> (enum, <see cref="string"/>, or <see langword="null" />) into a valid
/// <see cref="BooleanOperatorType"/>, falling back to a caller‑supplied
/// default when conversion fails or when the result is not whitelisted.
/// </para>
/// </remarks>
public static class BooleanOperatorTypeResolver
{
    /// <summary>
    /// The default set of boolean operators accepted when the caller
    /// does not provide an explicit allow‑list.
    /// </summary>
    private static readonly BooleanOperatorType[] DefaultBooleanOperatorTypes =
    [
        BooleanOperatorType.AND,
        BooleanOperatorType.OR,
    ];

    /// <summary>
    /// Attempts to resolve <paramref name="value"/> into a
    /// <see cref="BooleanOperatorType"/>.
    /// </summary>
    /// <param name="value">
    /// An enum value, a case‑insensitive string, or <see langword="null" />.
    /// </param>
    /// <param name="fallbackOperatorType">
    /// The value returned when <paramref name="value"/> cannot be resolved or
    /// is not contained in <paramref name="allowedBooleanOperatorTypes"/>.
    /// </param>
    /// <param name="allowedBooleanOperatorTypes">
    /// Optional subset of valid results.  When <see langword="null" />, the default
    /// allow‑list (<see cref="BooleanOperatorType.AND"/> and
    /// <see cref="BooleanOperatorType.OR"/>) is used.
    /// </param>
    /// <returns>
    /// A resolved <see cref="BooleanOperatorType"/> or
    /// <paramref name="fallbackOperatorType"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// var op = BooleanOperatorTypeHelper.ResolveOperator(
    ///              "and",
    ///              fallbackOperatorType: BooleanOperatorType.OR);
    /// // op == BooleanOperatorType.AND
    /// </code>
    /// </example>
    public static BooleanOperatorType Resolve(
        object? value,
        BooleanOperatorType fallbackOperatorType = BooleanOperatorType.AND,
        BooleanOperatorType[]? allowedBooleanOperatorTypes = null)
    {
        if (value is null)
        {
            return fallbackOperatorType;
        }

        allowedBooleanOperatorTypes ??= DefaultBooleanOperatorTypes;

        return value switch
        {
            BooleanOperatorType op when allowedBooleanOperatorTypes.Contains(op) => op,
            string s when Enum.TryParse(s, ignoreCase: true, out BooleanOperatorType parsed) && allowedBooleanOperatorTypes.Contains(parsed) => parsed,
            _ => fallbackOperatorType,
        };
    }
}