namespace Atc.XamlToolkit.XamlStyler.Tests;

public sealed class InlineContentAndXmlnsMentionTests
{
    private static string Style(string xaml)
        => new XamlStylerService(new XamlStylerOptions()).StyleDocument(xaml);

    [Theory]
    [InlineData("<TextBlock><Image /> <Run>a</Run></TextBlock>", "<Image /> <Run>a</Run>")]
    [InlineData("<TextBlock><TextBlock Text=\"a\" /> <Run>b</Run></TextBlock>", "<TextBlock Text=\"a\" /> <Run>b</Run>")]
    public void SpaceBetweenAnElementAndAFollowingInlineElement_InInlineContent_IsKept(
        string xaml,
        string expectedPart)
    {
        // The content of a TextBlock is text: a space between two of its children is shown.
        var result = Style(xaml);

        result.Should().Contain(expectedPart);
    }

    [Fact]
    public void SpaceBetweenBlockElementsOnOneLine_OutsideInlineContent_IsStillDropped()
    {
        var result = Style("<StackPanel><Button /> <Button /></StackPanel>");

        result.Should().NotContain("<Button /> ");
    }

    [Theory]
    [InlineData("SelectableTextBlock")]
    [InlineData("local:OutlinedTextBlock")]
    public void TypesNamedAfterAnInlineCollection_KeepTheirInlineContentTogether(
        string elementName)
    {
        // A type derived from TextBlock is by convention named ...TextBlock.
        var xaml = $"<{elementName} xmlns:local=\"urn:x\">a<Run>b</Run>c</{elementName}>";

        var result = Style(xaml);

        result.Should().Contain("a<Run>b</Run>");
    }

    [Theory]
    [InlineData("<TextBlock Text=\"use xmlns:a='b' here\" />", "use xmlns:a='b' here")]
    [InlineData("<TextBlock Text='use xmlns:a=\"b\" here' />", "use xmlns:a=&quot;b&quot; here")]
    public void AttributeValue_ThatLooksLikeAnXmlnsDeclaration_IsLeftAlone(
        string xaml,
        string expectedValue)
    {
        var result = Style(xaml);

        result.Should().Contain(expectedValue);
        result.Should().NotContain("[a]");
    }

    [Fact]
    public void Comment_ThatLooksLikeAnXmlnsDeclaration_IsLeftAlone()
    {
        var result = Style("<Grid><!-- xmlns:foo='bar' --></Grid>");

        result.Should().Contain("xmlns:foo='bar'");
    }

    [Theory]
    [InlineData("<Grid xmlns:a='urn:a'><a:Thing /></Grid>")]
    [InlineData("<Grid\n    xmlns:a = \"urn:a\"\n    xmlns:b='urn:b'><a:Thing b:X=\"1\" /></Grid>")]
    public void RealXmlnsDeclarations_AreStillRecognised(string xaml)
    {
        var result = Style(xaml);

        result.Should().Contain("xmlns:a=\"urn:a\"");
        result.Should().NotContain("[a]");
    }
}