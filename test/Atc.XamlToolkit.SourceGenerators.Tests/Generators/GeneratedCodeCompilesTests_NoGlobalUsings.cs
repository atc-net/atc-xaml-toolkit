namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Compile tests for a consumer project without global usings: the generated files
/// must not depend on namespaces that only the user's own files import.
/// </summary>
public sealed class GeneratedCodeCompilesTests_NoGlobalUsings : GeneratorTestBase
{
    [Fact]
    public void ViewModel_WithCommandsAndCopiedAttributes_Compiles()
    {
        const string inputCode =
            """
            using System.ComponentModel.DataAnnotations;
            using System.Threading;
            using System.Threading.Tasks;
            using Atc.XamlToolkit.Command;
            using Atc.XamlToolkit.Mvvm;
            using TestNamespace.Models;

            namespace TestNamespace.Models
            {
                public enum Kind
                {
                    None,
                    Special,
                }

                [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Field)]
                public sealed class KindAttribute : System.Attribute
                {
                    public KindAttribute(Kind kind)
                    {
                    }
                }

                public class Person
                {
                    [Required]
                    [Kind(Kind.Special)]
                    public string Name { get; set; } = string.Empty;
                }
            }

            namespace TestNamespace
            {
                public partial class TestViewModel : ViewModelBase
                {
                    [ObservableProperty]
                    [Required]
                    [Kind(Kind.Special)]
                    private string name = string.Empty;

                    [RelayCommand]
                    private void Save()
                    {
                    }

                    [RelayCommand(SupportsCancellation = true)]
                    private Task LoadAsync(CancellationToken cancellationToken) => Task.CompletedTask;

                    [RelayCommand(AutoSetIsBusy = true, ExecuteOnBackgroundThread = true)]
                    private void Crunch(int count)
                    {
                    }

                    [RelayCommand(AutoSetIsBusy = true)]
                    private Task OpenAsync(string path, CancellationToken cancellationToken) => Task.CompletedTask;
                }

                [ObservableDtoViewModel(typeof(Person))]
                public partial class PersonViewModel : ViewModelBase
                {
                    [RelayCommand(ExecuteOnBackgroundThread = true)]
                    private Task SaveAsync() => Task.CompletedTask;
                }

                public partial class SaveBehavior
                {
                    [RelayCommand(ExecuteOnBackgroundThread = true)]
                    private void Save()
                    {
                    }
                }
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);
    }

    [Fact]
    public void AvaloniaControl_WithStyledAndAttachedProperties_Compiles()
    {
        const string inputCode =
            """
            using System;
            using Atc.XamlToolkit.Controls.Attributes;
            using Avalonia;
            using Avalonia.Controls;
            using Avalonia.Media;
            using TestNamespace.Models;

            namespace TestNamespace.Models
            {
                public class Person
                {
                }
            }

            namespace TestNamespace
            {
                [StyledProperty<Person>("Owner")]
                [StyledProperty<Guid>("Id")]
                [StyledProperty<Color>("Accent", DefaultValue = "Blue")]
                [StyledProperty<int>("Size", DefaultValue = 1, Category = "Layout", Description = "The size.", PropertyChangedCallback = nameof(OnSizeChanged))]
                public partial class MyControl : UserControl
                {
                    [StyledProperty]
                    private System.Collections.Generic.List<Person>? people;

                    private static void OnSizeChanged(MyControl sender, AvaloniaPropertyChangedEventArgs e)
                    {
                    }

                    public string Describe() => $"{Owner}{Id}{Accent}{Size}{People}";
                }

                [AttachedProperty<bool>("IsMarked")]
                public partial class MarkBehavior : AvaloniaObject
                {
                }
            }
            """;

        AssertCompilesWithAllGeneratorsWithoutGlobalUsings(inputCode);
    }
}