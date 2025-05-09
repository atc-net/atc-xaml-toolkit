namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
public sealed partial class ViewModelGeneratorTests : GeneratorTestBase
{
    [Fact]
    public void ClassAccessor_Invalid()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string name;
            }
            """;

        var generatorResult = RunGenerator<ViewModelGenerator>(inputCode);

        AssertGeneratorRunResultIsEmpty(generatorResult);
    }

    [Fact]
    public void MultiFiles4_ObservableProperty_Name_And_RelayCommand_NoParameter()
    {
        const string inputCode_MyViewModelBase =
            """
            namespace TestNamespace;

            public class MyViewModelBase : ViewModelBase
            {
            }
            """;

        const string inputCode_TestViewModel_Base =
            """
            namespace TestNamespace;

            public partial class TestViewModel : MyViewModelBase
            {
            }
            """;

        const string inputCode_TestViewModel_ObservableProperties =
            """
            namespace TestNamespace;

            public partial class TestViewModel
            {
                [ObservableProperty]
                private string name;
            }
            """;

        const string inputCode_TestViewModel_RelayCommands =
            """
            namespace TestNamespace;

            public partial class TestViewModel
            {
                [RelayCommand]
                private void Save()
                {
                }
            }
            """;

        const string expectedCode =
            """
            // <auto-generated>
            #nullable enable
            using Atc.XamlToolkit.Command;

            namespace TestNamespace;

            public partial class TestViewModel
            {
                private IRelayCommand? saveCommand;

                public IRelayCommand SaveCommand => saveCommand ??= new RelayCommand(Save);

                public string Name
                {
                    get => name;
                    set
                    {
                        if (name == value)
                        {
                            return;
                        }

                        name = value;
                        RaisePropertyChanged(nameof(Name));
                    }
                }
            }

            #nullable disable
            """;

        var generatorResult = RunGenerator<ViewModelGenerator>(
            inputCode_MyViewModelBase,
            inputCode_TestViewModel_Base,
            inputCode_TestViewModel_ObservableProperties,
            inputCode_TestViewModel_RelayCommands);

        AssertGeneratorRunResultAsEqual(expectedCode, generatorResult);
    }
}