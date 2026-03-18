namespace Atc.XamlToolkit.XamlStyler.Tests;

public sealed class MarkupExtensionParserTests
{
    [Fact]
    public void TryParse_SimpleExtension_Succeeds()
    {
        // Arrange & Act
        var success = MarkupExtensionParser.TryParse("{StaticResource MyKey}", out var graph);

        // Assert
        success.Should().BeTrue();
        graph.Should().NotBeNull();
        graph!.TypeName.Should().Be("StaticResource");
        graph.Arguments.Should().HaveCount(1);
        graph.Arguments[0].Should().BeOfType<PositionalArgument>();
    }

    [Fact]
    public void TryParse_BindingWithNamedArgs_Succeeds()
    {
        // Arrange & Act
        var success = MarkupExtensionParser.TryParse("{Binding Path=Name, Mode=TwoWay}", out var graph);

        // Assert
        success.Should().BeTrue();
        graph.Should().NotBeNull();
        graph!.TypeName.Should().Be("Binding");
        graph.Arguments.Should().HaveCount(2);
        graph.Arguments[0].Should().BeOfType<NamedArgument>();
        graph.Arguments[1].Should().BeOfType<NamedArgument>();

        var firstArg = (NamedArgument)graph.Arguments[0];
        firstArg.Name.Should().Be("Path");
        ((LiteralValue)firstArg.Value).Value.Should().Be("Name");

        var secondArg = (NamedArgument)graph.Arguments[1];
        secondArg.Name.Should().Be("Mode");
        ((LiteralValue)secondArg.Value).Value.Should().Be("TwoWay");
    }

    [Fact]
    public void TryParse_NestedExtension_Succeeds()
    {
        // Arrange & Act
        var success = MarkupExtensionParser.TryParse(
            "{Binding Converter={StaticResource Conv}}",
            out var graph);

        // Assert
        success.Should().BeTrue();
        graph.Should().NotBeNull();
        graph!.TypeName.Should().Be("Binding");
        graph.Arguments.Should().HaveCount(1);

        var namedArg = graph.Arguments[0].Should().BeOfType<NamedArgument>().Subject;
        namedArg.Name.Should().Be("Converter");
        namedArg.Value.Should().BeOfType<MarkupExtensions.Parser.MarkupExtension>();

        var nested = (MarkupExtensions.Parser.MarkupExtension)namedArg.Value;
        nested.TypeName.Should().Be("StaticResource");
        nested.Arguments.Should().HaveCount(1);
    }

    [Fact]
    public void TryParse_PositionalArg_Succeeds()
    {
        // Arrange & Act
        var success = MarkupExtensionParser.TryParse("{Binding Name}", out var graph);

        // Assert
        success.Should().BeTrue();
        graph.Should().NotBeNull();
        graph!.TypeName.Should().Be("Binding");
        graph.Arguments.Should().HaveCount(1);

        var positionalArg = graph.Arguments[0].Should().BeOfType<PositionalArgument>().Subject;
        ((LiteralValue)positionalArg.Value).Value.Should().Be("Name");
    }

    [Fact]
    public void TryParse_EmptyExtension_Succeeds()
    {
        // Arrange & Act
        var success = MarkupExtensionParser.TryParse("{x:Null}", out var graph);

        // Assert
        success.Should().BeTrue();
        graph.Should().NotBeNull();
        graph!.TypeName.Should().Be("x:Null");
        graph.Arguments.Should().BeEmpty();
    }

    [Fact]
    public void TryParse_InvalidInput_ReturnsFalse()
    {
        // Arrange & Act
        var success = MarkupExtensionParser.TryParse("plain text", out var graph);

        // Assert
        success.Should().BeFalse();
        graph.Should().BeNull();
    }

    [Fact]
    public void TryParse_QuotedString_Succeeds()
    {
        // Arrange & Act
        var success = MarkupExtensionParser.TryParse("{Binding Path='Items[0]'}", out var graph);

        // Assert
        success.Should().BeTrue();
        graph.Should().NotBeNull();
        graph!.TypeName.Should().Be("Binding");
        graph.Arguments.Should().HaveCount(1);

        var namedArg = graph.Arguments[0].Should().BeOfType<NamedArgument>().Subject;
        namedArg.Name.Should().Be("Path");
        ((LiteralValue)namedArg.Value).Value.Should().Be("'Items[0]'");
    }

    [Fact]
    public void TryParse_QuotedStringFormat_PreservesQuotes()
    {
        // Arrange & Act
        var success = MarkupExtensionParser.TryParse("{Binding StringFormat='Status: {0}'}", out var graph);

        // Assert
        success.Should().BeTrue();
        graph.Should().NotBeNull();
        graph!.TypeName.Should().Be("Binding");
        graph.Arguments.Should().HaveCount(1);

        var namedArg = graph.Arguments[0].Should().BeOfType<NamedArgument>().Subject;
        namedArg.Name.Should().Be("StringFormat");
        ((LiteralValue)namedArg.Value).Value.Should().Be("'Status: {0}'");
    }

    [Fact]
    public void TryParse_NestedRelativeSource_Succeeds()
    {
        // Arrange & Act
        var success = MarkupExtensionParser.TryParse(
            "{Binding RelativeSource={RelativeSource AncestorType=Control}, Path=Foo}",
            out var graph);

        // Assert
        success.Should().BeTrue();
        graph.Should().NotBeNull();
        graph!.TypeName.Should().Be("Binding");
        graph.Arguments.Should().HaveCount(2);

        var relSourceArg = graph.Arguments[0].Should().BeOfType<NamedArgument>().Subject;
        relSourceArg.Name.Should().Be("RelativeSource");
        relSourceArg.Value.Should().BeOfType<MarkupExtensions.Parser.MarkupExtension>();

        var nested = (MarkupExtensions.Parser.MarkupExtension)relSourceArg.Value;
        nested.TypeName.Should().Be("RelativeSource");
        nested.Arguments.Should().HaveCount(1);

        var pathArg = graph.Arguments[1].Should().BeOfType<NamedArgument>().Subject;
        pathArg.Name.Should().Be("Path");
        ((LiteralValue)pathArg.Value).Value.Should().Be("Foo");
    }

    [Fact]
    public void TryParse_DoubleNestedMarkupExtension_Succeeds()
    {
        // Arrange — real pattern from atc-wpf: three levels of nesting
        var success = MarkupExtensionParser.TryParse(
            "{Binding Source={atc:EnumToArrayBindingSource {x:Type atcClr:LeftRightType}}}",
            out var graph);

        // Assert
        success.Should().BeTrue();
        graph.Should().NotBeNull();
        graph!.TypeName.Should().Be("Binding");
        graph.Arguments.Should().HaveCount(1);

        var sourceArg = graph.Arguments[0].Should().BeOfType<NamedArgument>().Subject;
        sourceArg.Name.Should().Be("Source");
        sourceArg.Value.Should().BeOfType<MarkupExtensions.Parser.MarkupExtension>();

        var level2 = (MarkupExtensions.Parser.MarkupExtension)sourceArg.Value;
        level2.TypeName.Should().Be("atc:EnumToArrayBindingSource");
        level2.Arguments.Should().HaveCount(1);

        var level2Arg = level2.Arguments[0].Should().BeOfType<PositionalArgument>().Subject;
        level2Arg.Value.Should().BeOfType<MarkupExtensions.Parser.MarkupExtension>();

        var level3 = (MarkupExtensions.Parser.MarkupExtension)level2Arg.Value;
        level3.TypeName.Should().Be("x:Type");
        level3.Arguments.Should().HaveCount(1);
    }

    [Fact]
    public void TryParse_DoubleNestedWithPositionalArgs_Succeeds()
    {
        // Arrange — real pattern from atc-wpf: nested with additional positional argument
        var success = MarkupExtensionParser.TryParse(
            "{Binding Source={atc:EnumToArrayBindingSource {x:Type atcClr:LeftRightType}, PleaseSelect}}",
            out var graph);

        // Assert
        success.Should().BeTrue();
        graph.Should().NotBeNull();
        graph!.TypeName.Should().Be("Binding");
        graph.Arguments.Should().HaveCount(1);

        var sourceArg = graph.Arguments[0].Should().BeOfType<NamedArgument>().Subject;
        sourceArg.Name.Should().Be("Source");
        sourceArg.Value.Should().BeOfType<MarkupExtensions.Parser.MarkupExtension>();

        var level2 = (MarkupExtensions.Parser.MarkupExtension)sourceArg.Value;
        level2.TypeName.Should().Be("atc:EnumToArrayBindingSource");
        level2.Arguments.Should().HaveCount(2);

        // First arg: nested {x:Type ...}
        var level2Arg1 = level2.Arguments[0].Should().BeOfType<PositionalArgument>().Subject;
        level2Arg1.Value.Should().BeOfType<MarkupExtensions.Parser.MarkupExtension>();

        var level3 = (MarkupExtensions.Parser.MarkupExtension)level2Arg1.Value;
        level3.TypeName.Should().Be("x:Type");

        // Second arg: positional "PleaseSelect"
        var level2Arg2 = level2.Arguments[1].Should().BeOfType<PositionalArgument>().Subject;
        ((LiteralValue)level2Arg2.Value).Value.Should().Be("PleaseSelect");
    }

    [Fact]
    public void TryParse_DoubleBraceTemplateToken_ReturnsFalse()
    {
        // Arrange — {{placeholder}} tokens from template files are not markup extensions
        var success = MarkupExtensionParser.TryParse(
            "{{AtcApps.Brushes.SystemControlHighlightAltListAccentHigh.Opacity}}",
            out var graph);

        // Assert
        success.Should().BeFalse();
        graph.Should().BeNull();
    }
}