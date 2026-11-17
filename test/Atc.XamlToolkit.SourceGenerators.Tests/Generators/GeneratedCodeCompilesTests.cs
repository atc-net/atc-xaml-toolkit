namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Compiles the generated code together with the input - with all three generators
/// running, as in a consumer project - instead of only comparing text.
/// </summary>
public sealed class GeneratedCodeCompilesTests : GeneratorTestBase
{
    // The generated framework element code relies on these usings of the consumer project.
    private const string AvaloniaGlobalUsings =
        """
        global using System.ComponentModel;
        global using Avalonia;
        """;

    [Fact]
    public void ViewModel_WithObservablePropertyAndCommands_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string name = string.Empty;

                [RelayCommand(CanExecute = nameof(CanSave))]
                private void Save()
                {
                }

                private bool CanSave() => true;

                [RelayCommand]
                private Task LoadAsync(CancellationToken cancellationToken) => Task.CompletedTask;
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void RelayCommand_InvertCanExecute_WithCanExecuteMethod_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [RelayCommand(CanExecute = nameof(IsLocked), InvertCanExecute = true)]
                private void Save()
                {
                }

                [RelayCommand(CanExecute = nameof(IsItemLocked), InvertCanExecute = true)]
                private void Delete(string item)
                {
                }

                [RelayCommand(CanExecute = nameof(IsLocked), InvertCanExecute = true)]
                private Task LoadAsync() => Task.CompletedTask;

                [RelayCommand(CanExecute = nameof(IsItemLocked), InvertCanExecute = true)]
                private Task OpenAsync(string item) => Task.CompletedTask;

                private bool IsLocked() => false;

                private bool IsItemLocked(string item) => false;
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void INotifyPropertyChanged_SameClassNameInTwoNamespaces_Compiles()
    {
        const string inputCode =
            """
            namespace First
            {
                [INotifyPropertyChanged]
                public partial class Item
                {
                    [ObservableProperty]
                    private string name = string.Empty;
                }
            }

            namespace Second
            {
                [INotifyPropertyChanged]
                public partial class Item
                {
                    [ObservableProperty]
                    private int count;
                }
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void FrameworkElement_RelayCommandWithAutoSetIsBusy_UsesThePlatformOfTheProject()
    {
        // The compilation references Avalonia, so the generated dispatcher code must be Avalonia code.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class SaveBehavior
            {
                public bool IsBusy { get; set; }

                [RelayCommand(AutoSetIsBusy = true)]
                private Task SaveAsync() => Task.CompletedTask;
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void ObservableProperty_StructWithoutEqualityOperator_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public struct Money
            {
                public decimal Amount;
            }

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private Money price;

                [ObservableProperty]
                private Money? discount;

                [ObservableProperty]
                private (int Row, int Column) cell;
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void ObservableDtoViewModel_StructPropertyWithoutEqualityOperator_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public struct Money
            {
                public decimal Amount;
            }

            public class Order
            {
                public Money Total { get; set; }
            }

            [ObservableDtoViewModel(typeof(Order))]
            public partial class OrderViewModel : ViewModelBase
            {
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void ObservableDtoViewModel_WithCustomPropertyAndCommand_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public class Person
            {
                public string Name { get; set; } = string.Empty;
            }

            [ObservableDtoViewModel(typeof(Person))]
            public partial class PersonViewModel : ViewModelBase
            {
                [ObservableProperty]
                private bool isSelectedInList;

                [RelayCommand]
                private void Save()
                {
                }
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void ViewModel_NamedAsHelper_WithRelayCommand_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class SaveHelper : ViewModelBase
            {
                [ObservableProperty]
                private string name = string.Empty;

                [RelayCommand]
                private void Save()
                {
                }
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void RelayCommand_ParameterValues_WithCanExecute_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public enum Direction
            {
                Left,
                Right,
            }

            public partial class TestViewModel : ViewModelBase
            {
                public bool IsReady { get; set; }

                [RelayCommand("TurnLeft", CanExecute = nameof(CanTurn), ParameterValues = [Direction.Left, 1])]
                [RelayCommand("TurnRight", CanExecute = nameof(CanTurn), InvertCanExecute = true, ParameterValues = [Direction.Right, 2])]
                private void Turn(Direction direction, int steps)
                {
                }

                private bool CanTurn(Direction direction, int steps) => true;

                [RelayCommand("SayHello", ParameterValue = "say \"hi\", twice")]
                [RelayCommand("SayBye", CanExecute = nameof(IsReady), ParameterValue = "bye")]
                private void Say(string text)
                {
                }

                [RelayCommand("Enable", ParameterValue = true)]
                private void SetEnabled(bool value)
                {
                }

                [RelayCommand("Half", ParameterValues = [0.5, 'x', "a, b"])]
                private Task ScaleAsync(double factor, char unit, string label) => Task.CompletedTask;
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Theory]
    [InlineData("ExecuteOnBackgroundThread = true", "void", "")]
    [InlineData("ExecuteOnBackgroundThread = true", "Task", "")]
    [InlineData("ExecuteOnBackgroundThread = true", "Task", ", CancellationToken cancellationToken")]
    [InlineData("AutoSetIsBusy = true", "void", "")]
    [InlineData("AutoSetIsBusy = true", "Task", "")]
    [InlineData("AutoSetIsBusy = true", "Task", ", CancellationToken cancellationToken")]
    [InlineData("AutoSetIsBusy = true, ExecuteOnBackgroundThread = true", "void", "")]
    [InlineData("AutoSetIsBusy = true, ExecuteOnBackgroundThread = true", "Task", "")]
    [InlineData("AutoSetIsBusy = true, ExecuteOnBackgroundThread = true", "Task", ", CancellationToken cancellationToken")]
    [InlineData("SupportsCancellation = true", "Task", "")]
    [InlineData("SupportsCancellation = true", "Task", ", CancellationToken cancellationToken")]
    [InlineData("SupportsCancellation = true, AutoSetIsBusy = true", "Task", ", CancellationToken cancellationToken")]
    public void RelayCommand_TwoParameters_Compiles(
        string attributeArguments,
        string returnType,
        string cancellationTokenParameter)
    {
        var body = returnType == "void" ? "{ }" : "=> Task.CompletedTask;";

        var inputCode =
            $$"""
              namespace TestNamespace;

              public partial class TestViewModel : ViewModelBase
              {
                  [RelayCommand({{attributeArguments}})]
                  private {{returnType}} Move(int column, string row{{cancellationTokenParameter}}) {{body}}
              }
              """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void ObservableDtoViewModel_CommandWithTwoParametersOnBackgroundThread_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public class Person
            {
                public string Name { get; set; } = string.Empty;
            }

            [ObservableDtoViewModel(typeof(Person))]
            public partial class PersonViewModel : ViewModelBase
            {
                [RelayCommand(ExecuteOnBackgroundThread = true)]
                private Task Move(int column, string row) => Task.CompletedTask;
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void RelayCommand_ValueTaskMethods_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public class Person
            {
                public string Name { get; set; } = string.Empty;
            }

            public partial class TestViewModel : ViewModelBase
            {
                [RelayCommand]
                private ValueTask Save() => ValueTask.CompletedTask;

                [RelayCommand(CanExecute = nameof(CanOpen))]
                private ValueTask<int> Open(string path) => ValueTask.FromResult(1);

                private bool CanOpen(string path) => true;

                [RelayCommand(SupportsCancellation = true, AutoSetIsBusy = true)]
                private async ValueTask Load(string path, CancellationToken cancellationToken)
                    => await Task.Delay(1, cancellationToken);

                [RelayCommand(ExecuteOnBackgroundThread = true)]
                private ValueTask Move(int column, int row) => ValueTask.CompletedTask;

                [RelayCommand("FirstPage", ParameterValue = 1)]
                [RelayCommand("SecondPage", ParameterValue = 2)]
                private ValueTask GoToPage(int page) => ValueTask.CompletedTask;
            }

            public partial class SaveBehavior
            {
                [RelayCommand]
                private ValueTask Save(CancellationToken cancellationToken) => ValueTask.CompletedTask;
            }

            [ObservableDtoViewModel(typeof(Person))]
            public partial class PersonViewModel : ViewModelBase
            {
                [RelayCommand]
                private ValueTask Save() => ValueTask.CompletedTask;
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void RelayCommand_CanExecuteOnPrefixedObservableField_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private bool _canSave;

                [ObservableProperty]
                private bool m_canOpen;

                [RelayCommand(CanExecute = nameof(CanSave))]
                private void Save()
                {
                }

                [RelayCommand(CanExecute = nameof(CanOpen))]
                private void Open(string path)
                {
                }
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void RelayCommand_CanExecuteOnInheritedProperty_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public abstract partial class BaseViewModel : ViewModelBase
            {
                [ObservableProperty]
                private bool canOpen;

                public bool CanSave { get; set; }
            }

            public partial class TestViewModel : BaseViewModel
            {
                [RelayCommand(CanExecute = nameof(CanSave))]
                private void Save()
                {
                }

                [RelayCommand(CanExecute = nameof(CanOpen))]
                private void Open()
                {
                }

                [RelayCommand(CanExecute = nameof(IsBusy))]
                private void Refresh()
                {
                }
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void ObservableDtoViewModel_InitOnlyAndNonPublicSetters_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public class Person
            {
                public string Name { get; set; } = string.Empty;

                public int Id { get; init; }

                public int Age { get; private set; }

                public string Code { get; protected set; } = string.Empty;
            }

            [ObservableDtoViewModel(typeof(Person))]
            public partial class PersonViewModel : ViewModelBase
            {
            }

            public static class Consumer
            {
                public static string Use(PersonViewModel viewModel)
                {
                    viewModel.Name = "x";
                    return $"{viewModel.Id} {viewModel.Age} {viewModel.Code}";
                }
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);
    }

    [Fact]
    public void ObservableDtoViewModel_MethodsWithSpecialSignatures_Compiles()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public enum Unit
            {
                None,
                Meter,
            }

            public class Person
            {
                public string Name { get; set; } = string.Empty;

                public T Convert<T>(T value) where T : struct => value;

                public bool TryGetAge(out int age)
                {
                    age = 1;
                    return true;
                }

                public void Swap(ref int left, ref int right) => (left, right) = (right, left);

                public int Sum(in int first, params int[] rest) => first + rest.Length;

                public string Format(string prefix = "p", int width = 3, double scale = 0.5, Unit unit = Unit.Meter, string? suffix = null, bool upper = true, char pad = ' ', System.DateTime when = default) => prefix;

                public override int GetHashCode() => 1;

                public override bool Equals(object? obj) => true;
            }

            [ObservableDtoViewModel(typeof(Person))]
            public partial class PersonViewModel : ViewModelBase
            {
            }

            public static class Consumer
            {
                public static string Use(PersonViewModel viewModel)
                {
                    var left = 1;
                    var right = 2;
                    viewModel.Swap(ref left, ref right);
                    viewModel.TryGetAge(out var age);
                    return viewModel.Format() + viewModel.Sum(in age, 1, 2, 3);
                }
            }
            """;

        AssertCompilesWithAllGenerators(inputCode);

        var generatedCode = RunGenerator<ObservableDtoViewModelGenerator>(inputCode)
            .GeneratorResult
            .GeneratedSources[0]
            .SourceText
            .ToString();

        // Forwarding these without 'override' would hide the inherited members (CS0114).
        Assert.DoesNotContain("GetHashCode", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("Equals(", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void StyledProperty_DefaultValuesWithEscapesAndChars_Compiles()
    {
        const string inputCode =
            """
            using Atc.XamlToolkit.Controls.Attributes;

            namespace TestNamespace;

            [StyledProperty<string>("Greeting", DefaultValue = "say \"hi\" to c:\\temp", Category = "The \"common\" ones", Description = "A \"greeting\".")]
            [StyledProperty<char>("Separator", DefaultValue = ';')]
            [StyledProperty<char>("Quote", DefaultValue = '\'')]
            [StyledProperty<long>("Total", DefaultValue = 5)]
            public partial class MyControl : Avalonia.Controls.UserControl
            {
                public string Describe() => $"{Greeting}{Separator}{Quote}{Total}";
            }
            """;

        AssertCompilesWithAllGenerators(AvaloniaGlobalUsings, inputCode);
    }

    [Fact]
    public void StyledProperty_NegativeDefaultValues_CompileOnAnyBuildMachineCulture()
    {
        const string inputCode =
            """
            using Atc.XamlToolkit.Controls.Attributes;

            namespace TestNamespace;

            [StyledProperty<int>("Offset", DefaultValue = -1)]
            [StyledProperty<double>("Scale", DefaultValue = -1.5)]
            [StyledProperty<decimal>("Amount", DefaultValue = -2.5)]
            public partial class MyControl : Avalonia.Controls.UserControl
            {
                public string Describe() => $"{Offset}{Scale}{Amount}";
            }
            """;

        var originalCulture = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            // sv-SE formats negative numbers with U+2212 and uses a decimal comma.
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("sv-SE");

            AssertCompilesWithAllGenerators(AvaloniaGlobalUsings, inputCode);
        }
        finally
        {
            System.Globalization.CultureInfo.CurrentCulture = originalCulture;
        }
    }

    [Fact]
    public void CompileWithAllGenerators_ReportsCompileErrors()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                private UnknownType field;
            }
            """;

        var problems = CompileWithAllGenerators(inputCode);

        Assert.Contains(problems, x => x.Contains("CS0246", StringComparison.Ordinal));
    }
}