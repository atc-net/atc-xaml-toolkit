namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class ViewModelGeneratorTests
{
    [Fact]
    public void ObservableDtoViewModel_SameClassNameDifferentNamespaces()
    {
        const string inputCode =
            """
             namespace TestNamespace.A
             {
                 public class Person
                 {
                     public string? FirstName { get; set; }
                 }

                 [ObservableDtoViewModel(typeof(Person))]
                 public partial class PersonViewModel : ViewModelBase
                 {
                 }
             }

             namespace TestNamespace.B
             {
                 public class User
                 {
                     public string? UserName { get; set; }
                 }

                 [ObservableDtoViewModel(typeof(User))]
                 public partial class PersonViewModel : ViewModelBase
                 {
                 }
             }
             """;

        var (generatorResult, _) = RunGenerator<ObservableDtoViewModelGenerator>(inputCode);

        // Debug: Print all generated files
        // Should generate 2 files - one for each namespace
        Assert.Equal(2, generatorResult.GeneratedSources.Length);

        // Check first file (TestNamespace.A.PersonViewModel.g.cs)
        var firstSource = generatorResult.GeneratedSources.First(s => s.HintName == "TestNamespace.A.PersonViewModel.g.cs");
        var firstSourceText = firstSource.SourceText.ToString();
        Assert.Contains("namespace TestNamespace.A", firstSourceText, StringComparison.Ordinal);
        Assert.Contains("public string? FirstName", firstSourceText, StringComparison.Ordinal);
        Assert.DoesNotContain("UserName", firstSourceText, StringComparison.Ordinal);

        // Check second file (TestNamespace.B.PersonViewModel.g.cs)
        var secondSource = generatorResult.GeneratedSources.First(s => s.HintName == "TestNamespace.B.PersonViewModel.g.cs");
        var secondSourceText = secondSource.SourceText.ToString();
        Assert.Contains("namespace TestNamespace.B", secondSourceText, StringComparison.Ordinal);
        Assert.Contains("public string? UserName", secondSourceText, StringComparison.Ordinal);
        Assert.DoesNotContain("FirstName", secondSourceText, StringComparison.Ordinal);
    }
}


