namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Regressions and defects in the view-model and DTO generators found in review.
/// </summary>
public sealed class GeneratorTests_ViewModelReviewFindings : GeneratorTestBase
{
    private const string Usings =
        """
        using System.Threading;
        using System.Threading.Tasks;
        using Atc.XamlToolkit.Command;
        using Atc.XamlToolkit.Mvvm;

        namespace TestNamespace;

        """;

    [Fact]
    public void GeneratedFile_EndsWithTheCompleteNullableDirective()
    {
        // The last line break was removed by cutting two characters, which on a platform
        // with a one-character line break also cut the last letter of the directive.
        const string inputCode = Usings +
            """
            public partial class Vm : ViewModelBase
            {
                [ObservableProperty]
                private string name = string.Empty;
            }
            """;

        var generatedCode = Assert.Single(GetSourcesGeneratedForAvaloniaConsumer(inputCode));

        Assert.EndsWith("#nullable disable", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void AsyncRelayCommand_InAWinUIControl_DoesNotCallOnPropertyChanged()
    {
        // A control has no OnPropertyChanged; only a view model can re-raise IsExecuting.
        const string inputCode =
            """
            namespace Microsoft.UI.Xaml
            {
                public class DependencyObject
                {
                }
            }

            namespace Microsoft.UI.Xaml.Controls
            {
                public class UserControl : Microsoft.UI.Xaml.DependencyObject
                {
                }
            }

            namespace TestNamespace
            {
                using Microsoft.UI.Xaml.Controls;

                public partial class MyControl : UserControl
                {
                    [RelayCommand]
                    private Task LoadAsync(CancellationToken cancellationToken) => Task.CompletedTask;
                }
            }
            """;

        var generatedCode = string.Join(
            Environment.NewLine,
            RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources.Select(x => x.SourceText.ToString()));

        Assert.Contains("LoadAsyncCommand", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("OnPropertyChanged", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void DtoMethod_DefaultValuesThatNeedASuffixOrAName_AreForwardedAsWritten()
    {
        const string inputCode = Usings +
            """
            public sealed class Dto
            {
                public string Name { get; set; } = string.Empty;

                public void M1(decimal m = 1.5m) { }

                public void M2(float f = 1.5f) { }

                public void M3(decimal? m = 2.5m, float? f = 2.5f) { }

                public void M4(double d = double.NaN, float f = float.NegativeInfinity) { }

                public void M5(double d = double.PositiveInfinity, long l = long.MaxValue, uint u = 3u) { }

                public void M6(double d = 1.5, decimal whole = 2m, char c = 'x', string s = "a\"b") { }
            }

            [ObservableDtoViewModel(typeof(Dto))]
            public partial class DtoVm : ViewModelBase
            {
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = string.Join(Environment.NewLine, GetSourcesGeneratedForAvaloniaConsumer(inputCode));

        Assert.Contains("decimal m = 1.5m", generatedCode, StringComparison.Ordinal);
        Assert.Contains("float f = 1.5f", generatedCode, StringComparison.Ordinal);
        Assert.Contains("double d = double.NaN", generatedCode, StringComparison.Ordinal);
        Assert.Contains("float f = float.NegativeInfinity", generatedCode, StringComparison.Ordinal);
        Assert.Contains("double d = double.PositiveInfinity", generatedCode, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("typeof(NoProperties)", ": ViewModelBase")]
    [InlineData("typeof(int)", ": ViewModelBase")]
    [InlineData("typeof(Person[])", ": ViewModelBase")]
    public void DtoViewModel_ForWhichTheDtoGeneratorEmitsNothing_KeepsItsOwnPropertiesAndCommands(
        string dtoType,
        string baseList)
    {
        var inputCode = Usings +
            $$"""
            public sealed class Person
            {
                public string Name { get; set; } = string.Empty;
            }

            public sealed class NoProperties
            {
                public void Reset() { }
            }

            [ObservableDtoViewModel({{dtoType}})]
            public partial class Vm {{baseList}}
            {
                [ObservableProperty]
                private int a;

                [RelayCommand]
                private void Go() { }

                public void Use()
                {
                    A = 1;
                    GoCommand.Execute(null);
                }
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);
    }

    [Fact]
    public void DtoViewModel_WithItsOwnMembers_GetsEachMemberOnce()
    {
        const string inputCode = Usings +
            """
            public sealed class Person
            {
                public string Name { get; set; } = string.Empty;
            }

            [ObservableDtoViewModel(typeof(Person))]
            public partial class Vm : ViewModelBase
            {
                [ObservableProperty]
                private int a;

                [RelayCommand]
                private void Go() { }

                public void Use()
                {
                    A = 1;
                    Name = "x";
                    GoCommand.Execute(null);
                }
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);
    }

    [Fact]
    public void DtoProperty_WithASetterTheViewModelCanCall_IsWritable()
    {
        // 'internal set' is callable from a view model in the same assembly.
        const string inputCode = Usings +
            """
            public sealed class Dto
            {
                public string Name { get; internal set; } = string.Empty;

                public string Code { get; private set; } = string.Empty;

                public string Title { get; protected internal set; } = string.Empty;
            }

            [ObservableDtoViewModel(typeof(Dto))]
            public partial class DtoVm : ViewModelBase
            {
                public void Use()
                {
                    Name = "x";
                    Title = "y";
                    _ = Code;
                }
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = string.Join(Environment.NewLine, GetSourcesGeneratedForAvaloniaConsumer(inputCode));

        Assert.Contains("public string Code => ", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void FileLocalClass_DoesNotStopTheGeneratorForOtherClasses()
    {
        // A file-local type cannot have a partial declaration in a generated file. It used
        // to make the generator throw, which removed the generated code of every class.
        const string inputCode = Usings +
            """
            file partial class FileVm : ViewModelBase
            {
                [ObservableProperty]
                private int a;
            }

            public partial class Vm : ViewModelBase
            {
                [ObservableProperty]
                private int b;

                public void Use() => B = 1;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);
    }

    [Fact]
    public void ClassInAKeywordNamedNamespace_IsGenerated()
    {
        const string inputCode =
            """
            using Atc.XamlToolkit.Mvvm;

            namespace TestNamespace.@event;

            public partial class Vm : ViewModelBase
            {
                [ObservableProperty]
                private int b;

                public void Use() => B = 1;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);
    }

    [Fact]
    public void ValueTaskCommands_ForOverloadsOfOneMethod_EachGetAnAdapter()
    {
        const string inputCode = Usings +
            """
            public partial class Vm : ViewModelBase
            {
                [RelayCommand("LoadInt")]
                private ValueTask Load(int a) => ValueTask.CompletedTask;

                [RelayCommand("LoadString")]
                private ValueTask Load(string a) => ValueTask.CompletedTask;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);
    }

    [Theory]
    [InlineData("Protected", "private set")]
    [InlineData("PrivateProtected", "private set")]
    [InlineData("ProtectedInternal", "internal set")]
    [InlineData("Internal", "internal set")]
    public void ObservableProperty_ProtectedSetterInASealedClass_IsNarrowedToWhatItMeansThere(
        string enumValue,
        string expectedSetter)
    {
        // 'protected' in a sealed class is reported by the compiler (CS0628); it means private there.
        var inputCode = Usings +
            $$"""
            public sealed partial class Vm : ViewModelBase
            {
                [ObservableProperty(SetterAccessModifier = Atc.XamlToolkit.PropertyAccessModifier.{{enumValue}})]
                private int a;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = string.Join(Environment.NewLine, GetSourcesGeneratedForAvaloniaConsumer(inputCode));

        Assert.Contains(expectedSetter, generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StyledProperty_ProtectedMembersInASealedClass_AreNarrowedToWhatTheyMeanThere()
    {
        const string inputCode =
            """
            using Atc.XamlToolkit;
            using Atc.XamlToolkit.Controls.Attributes;
            using Avalonia.Controls;

            namespace TestNamespace;

            public sealed partial class MyControl : UserControl
            {
                [StyledProperty(AccessModifier = PropertyAccessModifier.Protected)]
                private int a;

                [StyledProperty(SetterAccessModifier = PropertyAccessModifier.ProtectedInternal)]
                private int b;
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);

        var generatedCode = string.Join(Environment.NewLine, GetSourcesGeneratedForAvaloniaConsumer(inputCode));

        Assert.Contains("private static readonly StyledProperty<int> AProperty", generatedCode, StringComparison.Ordinal);
        Assert.Contains("private int A", generatedCode, StringComparison.Ordinal);
        Assert.Contains("internal set => SetValue(BProperty, value);", generatedCode, StringComparison.Ordinal);
    }
}