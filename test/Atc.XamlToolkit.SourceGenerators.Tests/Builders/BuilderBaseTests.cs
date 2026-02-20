namespace Atc.XamlToolkit.SourceGenerators.Tests.Builders;

public sealed class BuilderBaseTests
{
    private sealed class TestBuilder : BuilderBase;

    [Fact]
    public void GetUniqueVariableName_ReturnsIncrementingNames()
    {
        var builder = new TestBuilder();

        var name0 = builder.GetUniqueVariableName("var");
        var name1 = builder.GetUniqueVariableName("var");
        var name2 = builder.GetUniqueVariableName("temp");

        Assert.Equal("var0", name0);
        Assert.Equal("var1", name1);
        Assert.Equal("temp2", name2);
    }

    [Fact]
    public void GetUniqueVariableName_IndependentPerInstance()
    {
        var builder1 = new TestBuilder();
        var builder2 = new TestBuilder();

        var name1 = builder1.GetUniqueVariableName("x");
        var name2 = builder2.GetUniqueVariableName("x");

        Assert.Equal("x0", name1);
        Assert.Equal("x0", name2);
    }

    [Fact]
    public void IncreaseIndent_IncreasesIndentLevel()
    {
        var builder = new TestBuilder();

        Assert.Equal(0, builder.IndentLevel);

        builder.IncreaseIndent();
        Assert.Equal(1, builder.IndentLevel);

        builder.IncreaseIndent();
        Assert.Equal(2, builder.IndentLevel);
    }

    [Fact]
    public void DecreaseIndent_DecreasesIndentLevel()
    {
        var builder = new TestBuilder();
        builder.IncreaseIndent();
        builder.IncreaseIndent();

        var result = builder.DecreaseIndent();

        Assert.True(result);
        Assert.Equal(1, builder.IndentLevel);
    }

    [Fact]
    public void DecreaseIndent_AtZero_ReturnsFalse()
    {
        var builder = new TestBuilder();

        var result = builder.DecreaseIndent();

        Assert.False(result);
        Assert.Equal(0, builder.IndentLevel);
    }

    [Fact]
    public void IncreaseIndent_AppliesIndentToAppendLine()
    {
        var builder = new TestBuilder();
        builder.IncreaseIndent();
        builder.AppendLine("test");

        var output = builder.ToString();

        Assert.StartsWith("    test", output, StringComparison.Ordinal);
    }

    [Fact]
    public void IncreaseIndent_MultipleLevel_AppliesCorrectIndent()
    {
        var builder = new TestBuilder();
        builder.IncreaseIndent();
        builder.IncreaseIndent();
        builder.IncreaseIndent();
        builder.AppendLine("test");

        var output = builder.ToString();

        Assert.StartsWith("            test", output, StringComparison.Ordinal);
    }

    [Fact]
    public void IndentCache_WorksBeyondCacheLimit()
    {
        var builder = new TestBuilder();
        for (var i = 0; i < 20; i++)
        {
            builder.IncreaseIndent();
        }

        Assert.Equal(20, builder.IndentLevel);

        builder.AppendLine("test");
        var output = builder.ToString();

        // 20 levels * 4 spaces = 80 spaces
        Assert.StartsWith(new string(' ', 80) + "test", output, StringComparison.Ordinal);
    }
}