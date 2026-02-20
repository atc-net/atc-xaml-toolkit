namespace Atc.XamlToolkit.SourceGenerators.Tests.Extensions;

public sealed class StringExtensionsTests
{
    [Theory]
    [InlineData("string", "string")]
    [InlineData("string?", "string")]
    [InlineData("IList<string>", "IList<string>")]
    [InlineData("IList<string>?", "IList<string>")]
    public void RemoveNullableSuffix(
        string input,
        string expected)
    {
        var result = input.RemoveNullableSuffix();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("m_fieldName", "fieldName")]
    [InlineData("_fieldName", "fieldName")]
    [InlineData("fieldName", "fieldName")]
    [InlineData("m_", "")]
    [InlineData("_", "")]
    public void StripPrefixFromField_WorksCorrectly(
        string input,
        string expected)
    {
        var result = input.RemovePrefixFromField();
        Assert.Equal(expected, result);
    }

    [Theory]
    [MemberData(nameof(GetExtractAttributeConstructorParametersTestData))]
    public void ExtractAttributeConstructorParameters(
        string input,
        Dictionary<string, string?> expected)
    {
        var result = input.ExtractAttributeConstructorParameters();
        Assert.NotNull(result);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", "Hello")]
    [InlineData("Hello", "Hello")]
    [InlineData("a", "A")]
    [InlineData("A", "A")]
    [InlineData("", "")]
    [InlineData("myProperty", "MyProperty")]
    [InlineData("123numeric", "123numeric")]
    public void EnsureFirstCharacterToUpper_WorksCorrectly(
        string input,
        string expected)
    {
        var result = Atc.XamlToolkit.SourceGenerators.Extensions.StringExtensions.EnsureFirstCharacterToUpper(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EnsureFirstCharacterToUpper_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Atc.XamlToolkit.SourceGenerators.Extensions.StringExtensions.EnsureFirstCharacterToUpper(null!));
    }

    [Theory]
    [InlineData("Hello", "hello")]
    [InlineData("hello", "hello")]
    [InlineData("A", "a")]
    [InlineData("a", "a")]
    [InlineData("", "")]
    [InlineData("MyProperty", "myProperty")]
    [InlineData("123numeric", "123numeric")]
    public void EnsureFirstCharacterToLower_WorksCorrectly(
        string input,
        string expected)
    {
        var result = Atc.XamlToolkit.SourceGenerators.Extensions.StringExtensions.EnsureFirstCharacterToLower(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void EnsureFirstCharacterToLower_NullInput_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => Atc.XamlToolkit.SourceGenerators.Extensions.StringExtensions.EnsureFirstCharacterToLower(null!));
    }

    [Theory]
    [InlineData("string", "string")]
    [InlineData("string?", "string")]
    [InlineData("int", "int")]
    [InlineData("int?", "int?")]
    [InlineData("bool?", "bool?")]
    [InlineData("Guid?", "Guid?")]
    [InlineData("DateTime?", "DateTime?")]
    [InlineData("MyCustomClass?", "MyCustomClass")]
    [InlineData("SomeRefType?", "SomeRefType")]
    [InlineData("Thickness?", "Thickness?")]
    [InlineData("", "")]
    public void TrimNullableForTypeOf_WorksCorrectly(
        string input,
        string expected)
    {
        var result = input.TrimNullableForTypeOf();
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("bool", true)]
    [InlineData("int", true)]
    [InlineData("double", true)]
    [InlineData("Guid", true)]
    [InlineData("DateTime", true)]
    [InlineData("Thickness", true)]
    [InlineData("Color", true)]
    [InlineData("HorizontalAlignment", true)]
    [InlineData("string", false)]
    [InlineData("MyCustomClass", false)]
    [InlineData("object", false)]
    [InlineData("", false)]
    [InlineData("IList<int>", true)]
    [InlineData("IList<string>", false)]
    [InlineData("bool?", true)]
    public void IsKnownValueType_WorksCorrectly(
        string input,
        bool expected)
    {
        var result = input.IsKnownValueType();
        Assert.Equal(expected, result);
    }

    public static IEnumerable<object[]> GetExtractAttributeConstructorParametersTestData()
    {
        yield return [
            "[RelayCommand]",
            new Dictionary<string, string?>(StringComparer.Ordinal),
        ];

        yield return [
            "[RelayCommand(\"MyCommand\")]",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "Name", "MyCommand" },
            },
        ];

        yield return [
            "[RelayCommand(\"MyCommand\", CanExecute = nameof(MyCanExecute))]",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "Name", "MyCommand" },
                { "CanExecute", "nameof(MyCanExecute)" },
            },
        ];

        yield return [
            "[RelayCommand(CanExecute = nameof(MyCanExecute))]",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "CanExecute", "nameof(MyCanExecute)" },
            },
        ];

        yield return [
            "[RelayCommand(\"MyTestLeft\", ParameterValues = [LeftTopRightBottomType.Left, 1])]",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "Name", "MyTestLeft" },
                { "ParameterValues", "LeftTopRightBottomType.Left, 1" },
            },
        ];

        yield return [
            "[ObservableProperty(nameof(MyName), DependentPropertyNames = [nameof(FullName), nameof(Age)])]",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "Name", "MyName" },
                { "DependentPropertyNames", "nameof(FullName), nameof(Age)" },
            },
        ];

        yield return [
            "[ObservableProperty(BeforeChangedCallback = \"DoStuffA();\", AfterChangedCallback = \"EntrySelected?.Invoke(this, selectedEntry); DoStuffB();\")]",
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                { "BeforeChangedCallback", "DoStuffA();" },
                { "AfterChangedCallback", "EntrySelected?.Invoke(this, selectedEntry); DoStuffB();" },
            },
        ];
    }
}