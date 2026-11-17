namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

/// <summary>
/// Compile tests for classes that are nested, generic or declared in the global namespace.
/// </summary>
public sealed class GeneratedCodeCompilesTests_TypeShapes : GeneratorTestBase
{
    private const string AvaloniaGlobalUsings =
        """
        global using System.ComponentModel;
        global using Atc.XamlToolkit.Controls.Attributes;
        global using Avalonia;
        """;

    [Fact]
    public void ClassesInGlobalNamespace_Compile()
    {
        const string inputCode =
            """
            public class Person
            {
                public string Name { get; set; } = string.Empty;
            }

            public partial class TestViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string name = string.Empty;

                [RelayCommand]
                private Task SaveAsync() => Task.CompletedTask;
            }

            [INotifyPropertyChanged]
            public partial class Item
            {
                [ObservableProperty]
                private int count;
            }

            [ObservableDtoViewModel(typeof(Person))]
            public partial class PersonViewModel : ViewModelBase
            {
            }

            [StyledProperty<int>("Size", DefaultValue = 1)]
            public partial class SizeControl : Avalonia.Controls.UserControl
            {
                [RelayCommand]
                private void Grow() => Size++;
            }

            public static class Consumer
            {
                public static string Use(TestViewModel viewModel, Item item, PersonViewModel person, SizeControl control)
                    => $"{viewModel.Name}{viewModel.SaveAsyncCommand}{item.Count}{person.Name}{control.Size}{control.GrowCommand}";
            }
            """;

        AssertCompilesWithAllGenerators(AvaloniaGlobalUsings, inputCode);
    }

    [Fact]
    public void NestedClasses_Compile()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public class Person
            {
                public string Name { get; set; } = string.Empty;
            }

            public partial class Outer
            {
                public partial class InnerViewModel : ViewModelBase
                {
                    [ObservableProperty]
                    private string name = string.Empty;

                    [RelayCommand]
                    private Task SaveAsync() => Task.CompletedTask;
                }

                [INotifyPropertyChanged]
                internal partial class Item
                {
                    [ObservableProperty]
                    private int count;
                }

                [ObservableDtoViewModel(typeof(Person))]
                private partial class PersonViewModel : ViewModelBase
                {
                }

                [StyledProperty<int>("Size", DefaultValue = 1)]
                protected partial class SizeControl : Avalonia.Controls.UserControl
                {
                    [RelayCommand]
                    private void Grow() => Size++;
                }

                public static string Use()
                    => $"{new InnerViewModel().Name}{new InnerViewModel().SaveAsyncCommand}{new Item().Count}{new PersonViewModel(new Person()).Name}{new SizeControl().Size}{new SizeControl().GrowCommand}";
            }

            public partial struct OuterStruct
            {
                public partial class InnerViewModel : ViewModelBase
                {
                    [ObservableProperty]
                    private string name = string.Empty;
                }
            }

            public partial record OuterRecord
            {
                public partial class InnerViewModel : ViewModelBase
                {
                    [ObservableProperty]
                    private string name = string.Empty;
                }
            }

            public static partial class Level1<TKey>
            {
                public partial class Level2
                {
                    public partial class InnerViewModel : ViewModelBase
                    {
                        [ObservableProperty]
                        private TKey? key;
                    }
                }
            }

            public static class Consumer
            {
                public static string Use()
                    => $"{new OuterStruct.InnerViewModel().Name}{new OuterRecord.InnerViewModel().Name}{new Level1<int>.Level2.InnerViewModel().Key}";
            }
            """;

        AssertCompilesWithAllGenerators(AvaloniaGlobalUsings, inputCode);
    }

    [Fact]
    public void GenericClasses_Compile()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public class Person
            {
                public string Name { get; set; } = string.Empty;
            }

            public partial class ItemViewModel<T> : ViewModelBase
                where T : class
            {
                [ObservableProperty]
                private T? item;

                [RelayCommand]
                private void Select(T? value) => Item = value;
            }

            // The same name with another arity must not collide with the generic class.
            public partial class ItemViewModel : ViewModelBase
            {
                [ObservableProperty]
                private string name = string.Empty;
            }

            [INotifyPropertyChanged]
            public partial class Pair<TFirst, TSecond>
            {
                [ObservableProperty]
                private TFirst? first;

                [ObservableProperty]
                private TSecond? second;
            }

            [ObservableDtoViewModel(typeof(Person))]
            public partial class PersonViewModel<TTag> : ViewModelBase
            {
                public TTag? Tag { get; set; }
            }

            [StyledProperty<int>("Size", DefaultValue = 1, PropertyChangedCallback = nameof(OnSizeChanged))]
            public partial class SizeControl<T> : Avalonia.Controls.UserControl
            {
                private static void OnSizeChanged(SizeControl<T> sender, AvaloniaPropertyChangedEventArgs e)
                {
                }
            }

            public static class Consumer
            {
                public static string Use()
                    => $"{new ItemViewModel<string>().Item}{new ItemViewModel<string>().SelectCommand}{new ItemViewModel().Name}{new Pair<int, string>().First}{new PersonViewModel<int>(new Person()).Name}{new SizeControl<int>().Size}";
            }
            """;

        AssertCompilesWithAllGenerators(AvaloniaGlobalUsings, inputCode);
    }
}