namespace Atc.XamlToolkit.SourceGenerators.Tests.Extensions;

public sealed class CompilationExtensionsTests
{
    private static CSharpCompilation CreateEmptyCompilation()
        => CSharpCompilation.Create(
            "TestAssembly",
            [],
            [MetadataReference.CreateFromFile(typeof(object).Assembly.Location)],
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

    [Fact]
    public void GetXamlPlatform_NoReferences_DefaultsToWpf()
    {
        var compilation = CreateEmptyCompilation();

        var result = compilation.GetXamlPlatform();

        Assert.Equal(XamlPlatform.Wpf, result);
    }

    [Fact]
    public void GetXamlPlatform_SameCompilation_ReturnsCachedResult()
    {
        var compilation = CreateEmptyCompilation();

        var result1 = compilation.GetXamlPlatform();
        var result2 = compilation.GetXamlPlatform();

        Assert.Equal(result1, result2);
    }

    [Fact]
    public void GetXamlPlatform_DifferentCompilations_EachGetOwnResult()
    {
        var compilation1 = CreateEmptyCompilation();
        var compilation2 = CreateEmptyCompilation();

        var result1 = compilation1.GetXamlPlatform();
        var result2 = compilation2.GetXamlPlatform();

        // Both default to WPF since no platform references
        Assert.Equal(XamlPlatform.Wpf, result1);
        Assert.Equal(XamlPlatform.Wpf, result2);
    }
}