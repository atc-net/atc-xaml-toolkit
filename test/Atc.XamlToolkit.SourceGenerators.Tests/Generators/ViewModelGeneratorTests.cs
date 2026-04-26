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
    public void MissingPartial_OnObservableProperty_EmitsDiagnostic()
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

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.Contains(diagnostics, d => d.Id == "AtcXamlToolkit0002");
    }

    [Fact]
    public void MissingPartial_OnRelayCommand_EmitsDiagnostic()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public class TestViewModel : ViewModelBase
            {
                [RelayCommand]
                private void Save()
                {
                }
            }
            """;

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.Contains(diagnostics, d => d.Id == "AtcXamlToolkit0002");
    }

    [Fact]
    public void PartialClass_WithObservableProperty_DoesNotEmitMissingPartialDiagnostic()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string name;
            }
            """;

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.DoesNotContain(diagnostics, d => d.Id == "AtcXamlToolkit0002");
    }

    [Fact]
    public void ObservableProperty_OnPublicField_EmitsDiagnostic()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                public string name;
            }
            """;

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.Contains(diagnostics, d => d.Id == "AtcXamlToolkit0003");
    }

    [Fact]
    public void ObservableProperty_OnInternalField_EmitsDiagnostic()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                internal string name;
            }
            """;

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.Contains(diagnostics, d => d.Id == "AtcXamlToolkit0003");
    }

    [Fact]
    public void ObservableProperty_OnPascalCaseField_EmitsDiagnostic()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string Name;
            }
            """;

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.Contains(diagnostics, d => d.Id == "AtcXamlToolkit0004");
    }

    [Fact]
    public void ObservableProperty_OnMultiVariableDeclaration_FlagsOnlyOffendingVariables()
    {
        // Pin the per-variable diagnostic behaviour — the attribute can decorate
        // a multi-variable field declaration; only the variables that actually
        // violate naming should be flagged.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string firstName, LastName;
            }
            """;

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        var pascalCaseDiagnostics = diagnostics
            .Where(d => d.Id == "AtcXamlToolkit0004")
            .ToList();

        Assert.Single(pascalCaseDiagnostics);
        Assert.Contains(
            "LastName",
            pascalCaseDiagnostics[0].GetMessage(System.Globalization.CultureInfo.InvariantCulture),
            StringComparison.Ordinal);
    }

    [Fact]
    public void NotifyPropertyChangedFor_ReferencingNonExistentProperty_EmitsDiagnostic()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                [NotifyPropertyChangedFor("DoesNotExist")]
                private string firstName;
            }
            """;

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.Contains(diagnostics, d => d.Id == "AtcXamlToolkit0005");
    }

    [Fact]
    public void NotifyPropertyChangedFor_ReferencingExistingDeclaredProperty_DoesNotEmitDiagnostic()
    {
        // FullName is declared as a normal property — the diagnostic must
        // recognise it and stay silent.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                [NotifyPropertyChangedFor(nameof(FullName))]
                private string firstName;

                public string FullName => firstName;
            }
            """;

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.DoesNotContain(diagnostics, d => d.Id == "AtcXamlToolkit0005");
    }

    [Fact]
    public void NotifyPropertyChangedFor_ReferencingGeneratedObservableProperty_DoesNotEmitDiagnostic()
    {
        // 'LastName' will be generated by [ObservableProperty] on the
        // 'lastName' field — the diagnostic must look at fields-that-will-
        // become-properties, not just declared ones.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                [NotifyPropertyChangedFor(nameof(LastName))]
                private string firstName;

                [ObservableProperty]
                private string lastName;
            }
            """;

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.DoesNotContain(diagnostics, d => d.Id == "AtcXamlToolkit0005");
    }

    [Fact]
    public void NotifyPropertyChangedFor_MultipleReferences_OnlyMissingOneEmitsDiagnostic()
    {
        // [NotifyPropertyChangedFor("A", "B")] — only the missing one is flagged.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                [NotifyPropertyChangedFor(nameof(FullName), "DoesNotExist")]
                private string firstName;

                public string FullName => firstName;
            }
            """;

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        var matched = diagnostics.Where(d => d.Id == "AtcXamlToolkit0005").ToList();
        Assert.Single(matched);
        Assert.Contains(
            "DoesNotExist",
            matched[0].GetMessage(System.Globalization.CultureInfo.InvariantCulture),
            StringComparison.Ordinal);
    }

    [Fact]
    public void ObservableProperty_OnValidPrivateCamelCaseField_DoesNotEmitFieldDiagnostics()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string name;
            }
            """;

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.DoesNotContain(diagnostics, d => d.Id == "AtcXamlToolkit0003");
        Assert.DoesNotContain(diagnostics, d => d.Id == "AtcXamlToolkit0004");
    }

    [Fact]
    public void NonPartial_WithoutGeneratorAttributes_DoesNotEmitMissingPartialDiagnostic()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public class PlainOldClass
            {
                public string Name { get; set; } = string.Empty;
            }
            """;

        var (_, diagnostics) = RunGenerator<ViewModelGenerator>(inputCode);

        Assert.DoesNotContain(diagnostics, d => d.Id == "AtcXamlToolkit0002");
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

    [Fact]
    public void PartialClass_SplitAcrossFiles_AggregatesObservablePropertiesFromAllParts()
    {
        const string inputCode_PartA =
            """
            namespace TestNamespace;

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string firstName;
            }
            """;

        const string inputCode_PartB =
            """
            namespace TestNamespace;

            public partial class TestViewModel
            {
                [ObservableProperty]
                private string lastName;
            }
            """;

        const string expectedCode =
            """
            // <auto-generated>
            #nullable enable

            namespace TestNamespace;

            public partial class TestViewModel
            {
                public string FirstName
                {
                    get => firstName;
                    set
                    {
                        if (firstName == value)
                        {
                            return;
                        }

                        firstName = value;
                        RaisePropertyChanged(nameof(FirstName));
                    }
                }

                public string LastName
                {
                    get => lastName;
                    set
                    {
                        if (lastName == value)
                        {
                            return;
                        }

                        lastName = value;
                        RaisePropertyChanged(nameof(LastName));
                    }
                }
            }

            #nullable disable
            """;

        var generatorResult = RunGenerator<ViewModelGenerator>(
            inputCode_PartA,
            inputCode_PartB);

        AssertGeneratorRunResultAsEqual(expectedCode, generatorResult);
    }
}