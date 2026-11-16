namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Compiles the generated code together with the input - with all three generators
/// running, as in a consumer project - instead of only comparing text.
/// </summary>
public sealed class GeneratedCodeCompilesTests : GeneratorTestBase
{
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