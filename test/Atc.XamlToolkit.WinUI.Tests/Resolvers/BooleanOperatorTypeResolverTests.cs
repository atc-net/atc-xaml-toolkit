namespace Atc.XamlToolkit.WinUI.Tests.Resolvers;

using Atc.XamlToolkit.Resolvers;

public sealed class BooleanOperatorTypeResolverTests
{
    /// <summary>
    /// Test matrix:
    /// ┌────────────┬──────────┬───────────┬───────────┐
    /// │  value     │ fallback │ expected  │ allow‑lst │
    /// ├────────────┼──────────┼───────────┼───────────┤
    /// │ null       │  OR      │ OR        │ null      │
    /// │ "AND"      │  OR      │ AND       │ null      │
    /// │ "and"      │  OR      │ AND       │ null      │
    /// │ enum OR    │  AND     │ OR        │ null      │
    /// │ "invalid"  │  AND     │ AND       │ null      │
    /// │ enum OR    │  AND     │ OR        │ { OR }    │
    /// │ enum AND   │  OR      │ OR        │ { OR }    │
    /// └────────────┴──────────┴───────────┴───────────┘
    /// </summary>
    public static IEnumerable<object?[]> ResolveOperatorData =>
    [
        [null,                    BooleanOperatorType.OR,  BooleanOperatorType.OR,  null],
        ["AND",                   BooleanOperatorType.OR,  BooleanOperatorType.AND, null],
        ["and",                   BooleanOperatorType.OR,  BooleanOperatorType.AND, null],
        [BooleanOperatorType.OR,  BooleanOperatorType.AND, BooleanOperatorType.OR,  null],
        ["invalid",               BooleanOperatorType.AND, BooleanOperatorType.AND, null],
        [BooleanOperatorType.OR,  BooleanOperatorType.AND, BooleanOperatorType.OR,  new[] { BooleanOperatorType.OR }],
        [BooleanOperatorType.AND, BooleanOperatorType.OR,  BooleanOperatorType.OR,  new[] { BooleanOperatorType.OR }],
    ];

    [Theory]
    [MemberData(nameof(ResolveOperatorData))]
    public void ResolveOperator_ReturnsExpected(
        object? value,
        BooleanOperatorType fallback,
        BooleanOperatorType expected,
        BooleanOperatorType[]? allowList)
    {
        // act
        var result = BooleanOperatorTypeResolver.Resolve(value, fallback, allowList);

        // assert
        Assert.Equal(expected, result);
    }
}