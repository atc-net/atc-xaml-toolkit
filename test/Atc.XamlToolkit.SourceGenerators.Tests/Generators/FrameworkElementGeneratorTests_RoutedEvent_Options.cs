namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class FrameworkElementGeneratorTests
{
    private static string GenerateRoutedEventCode(string inputCode)
        => Assert.Single(RunGenerator<FrameworkElementGenerator>(inputCode).GeneratorResult.GeneratedSources)
            .SourceText
            .ToString()
            .EnsureEnvironmentNewLines();

    [Fact]
    public void RoutedEvent_GenerateDocumentation_DocumentsTheFieldAndTheEvent()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class CustomButton : Button
            {
                [RoutedEvent(GenerateDocumentation = true)]
                private static readonly RoutedEvent tap;
            }
            """;

        var generatedCode = GenerateRoutedEventCode(inputCode);

        Assert.Contains(
            """
                /// <summary>
                /// Identifies the <see cref="Tap"/> routed event.
                /// </summary>
                public static readonly RoutedEvent TapEvent = EventManager.RegisterRoutedEvent(
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
        Assert.Contains(
            """
                /// <summary>
                /// Occurs when the Tap routed event is raised.
                /// </summary>
                public event RoutedEventHandler Tap
            """.EnsureEnvironmentNewLines(),
            generatedCode,
            StringComparison.Ordinal);
    }

    [Fact]
    public void RoutedEvent_AssemblyGenerateDocumentationDefault_DocumentsTheFieldAndTheEvent()
    {
        const string inputCode =
            """
            [assembly: GenerateDocumentationDefault]

            namespace TestNamespace;

            public partial class CustomButton : Button
            {
                [RoutedEvent]
                private static readonly RoutedEvent tap;
            }
            """;

        var generatedCode = GenerateRoutedEventCode(inputCode);

        Assert.Contains("/// Identifies the <see cref=\"Tap\"/> routed event.", generatedCode, StringComparison.Ordinal);
        Assert.Contains("/// Occurs when the Tap routed event is raised.", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void RoutedEvent_AssemblyGenerateDocumentationDefault_CanBeTurnedOffPerEvent()
    {
        const string inputCode =
            """
            [assembly: GenerateDocumentationDefault]

            namespace TestNamespace;

            public partial class CustomButton : Button
            {
                [RoutedEvent(GenerateDocumentation = false)]
                private static readonly RoutedEvent tap;
            }
            """;

        Assert.DoesNotContain("///", GenerateRoutedEventCode(inputCode), StringComparison.Ordinal);
    }

    [Fact]
    public void RoutedEvent_GenerateRaiseMethod_RoutedEventHandler()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class CustomButton : Button
            {
                [RoutedEvent(GenerateRaiseMethod = true)]
                private static readonly RoutedEvent tap;
            }
            """;

        Assert.Contains(
            """
                protected void RaiseTap()
                    => RaiseEvent(new RoutedEventArgs(TapEvent, this));
            """.EnsureEnvironmentNewLines(),
            GenerateRoutedEventCode(inputCode),
            StringComparison.Ordinal);
    }

    [Fact]
    public void RoutedEvent_GenerateRaiseMethod_RoutedPropertyChangedEventHandler()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class CustomButton : Button
            {
                [RoutedEvent(HandlerType = typeof(RoutedPropertyChangedEventHandler<int?>), GenerateRaiseMethod = true)]
                private static readonly RoutedEvent valueChanged;
            }
            """;

        Assert.Contains(
            """
                protected void RaiseValueChanged(int? oldValue, int? newValue)
                    => RaiseEvent(new RoutedPropertyChangedEventArgs<int?>(oldValue, newValue, ValueChangedEvent));
            """.EnsureEnvironmentNewLines(),
            GenerateRoutedEventCode(inputCode),
            StringComparison.Ordinal);
    }

    [Fact]
    public void RoutedEvent_GenerateRaiseMethod_EventHandlerOfArgs()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class DrivePicker : UserControl
            {
                [RoutedEvent(HandlerType = typeof(EventHandler<DeviceErrorRoutedEventArgs>), GenerateRaiseMethod = true)]
                private static readonly RoutedEvent deviceError;
            }
            """;

        Assert.Contains(
            """
                protected void RaiseDeviceError(DeviceErrorRoutedEventArgs e)
                {
                    e.RoutedEvent = DeviceErrorEvent;
                    RaiseEvent(e);
                }
            """.EnsureEnvironmentNewLines(),
            GenerateRoutedEventCode(inputCode),
            StringComparison.Ordinal);
    }

    [Fact]
    public void RoutedEvent_GenerateRaiseMethod_SealedClass_IsPrivate()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public sealed partial class CustomButton : Button
            {
                [RoutedEvent(GenerateRaiseMethod = true)]
                private static readonly RoutedEvent tap;
            }
            """;

        Assert.Contains("    private void RaiseTap()", GenerateRoutedEventCode(inputCode), StringComparison.Ordinal);
    }

    [Fact]
    public void RoutedEvent_GenerateRaiseMethod_CustomDelegate_GeneratesNoRaiseMethod()
    {
        // The constructor of a custom event args type cannot be guessed.
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class CustomButton : Button
            {
                [RoutedEvent(HandlerType = typeof(CustomButtonChangedRoutedEventHandler), GenerateRaiseMethod = true)]
                private static readonly RoutedEvent changed;
            }
            """;

        var generatedCode = GenerateRoutedEventCode(inputCode);

        Assert.Contains("public event CustomButtonChangedRoutedEventHandler Changed", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("Raise", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void RoutedEvent_WithoutOptions_GeneratesNoRaiseMethodAndNoDocumentation()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class CustomButton : Button
            {
                [RoutedEvent]
                private static readonly RoutedEvent tap;
            }
            """;

        var generatedCode = GenerateRoutedEventCode(inputCode);

        Assert.DoesNotContain("Raise", generatedCode, StringComparison.Ordinal);
        Assert.DoesNotContain("///", generatedCode, StringComparison.Ordinal);
    }

    [Fact]
    public void RoutedEvent_AddOwnerOf_SharesTheExistingEvent()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public partial class DrivePicker : UserControl
            {
                [RoutedEvent(AddOwnerOf = typeof(DevicePickerEvents), HandlerType = typeof(EventHandler<DeviceErrorRoutedEventArgs>))]
                private static readonly RoutedEvent deviceError;
            }
            """;

        const string expectedCode =
            """
            // <auto-generated>
            #nullable enable
            using System.Windows;

            namespace TestNamespace;

            public partial class DrivePicker
            {
                public static readonly RoutedEvent DeviceErrorEvent = DevicePickerEvents.DeviceErrorEvent.AddOwner(typeof(DrivePicker));

                public event EventHandler<DeviceErrorRoutedEventArgs> DeviceError
                {
                    add => AddHandler(DeviceErrorEvent, value);
                    remove => RemoveHandler(DeviceErrorEvent, value);
                }
            }

            #nullable disable
            """;

        var generatorResult = RunGenerator<FrameworkElementGenerator>(inputCode);

        AssertGeneratorRunResultAsEqual(expectedCode, generatorResult);
    }

    [Fact]
    public void RoutedEvent_InAStaticClass_GeneratesAddAndRemoveHandlerMethods()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class DataGridFilter
            {
                [RoutedEvent(HandlerType = typeof(DataGridFilterChangedEventHandler), GenerateRaiseMethod = true)]
                private static readonly RoutedEvent filterChanged;
            }
            """;

        const string expectedCode =
            """
            // <auto-generated>
            #nullable enable
            using System.Windows;

            namespace TestNamespace;

            public static partial class DataGridFilter
            {
                public static readonly RoutedEvent FilterChangedEvent = EventManager.RegisterRoutedEvent(
                    name: "FilterChanged",
                    routingStrategy: RoutingStrategy.Bubble,
                    handlerType: typeof(DataGridFilterChangedEventHandler),
                    ownerType: typeof(DataGridFilter));

                public static void AddFilterChangedHandler(DependencyObject element, DataGridFilterChangedEventHandler handler)
                {
                    if (element is UIElement uiElement)
                    {
                        uiElement.AddHandler(FilterChangedEvent, handler);
                    }
                    else if (element is ContentElement contentElement)
                    {
                        contentElement.AddHandler(FilterChangedEvent, handler);
                    }
                }

                public static void RemoveFilterChangedHandler(DependencyObject element, DataGridFilterChangedEventHandler handler)
                {
                    if (element is UIElement uiElement)
                    {
                        uiElement.RemoveHandler(FilterChangedEvent, handler);
                    }
                    else if (element is ContentElement contentElement)
                    {
                        contentElement.RemoveHandler(FilterChangedEvent, handler);
                    }
                }
            }

            #nullable disable
            """;

        var generatorResult = RunGenerator<FrameworkElementGenerator>(inputCode);

        AssertGeneratorRunResultAsEqual(expectedCode, generatorResult);
    }

    [Fact]
    public void RoutedEvent_InAStaticClass_GenerateDocumentation_DocumentsTheMembers()
    {
        const string inputCode =
            """
            namespace TestNamespace;

            public static partial class DataGridFilter
            {
                [RoutedEvent(GenerateDocumentation = true)]
                private static readonly RoutedEvent filterChanged;
            }
            """;

        var generatedCode = GenerateRoutedEventCode(inputCode);

        Assert.Contains("/// Identifies the FilterChanged attached routed event.", generatedCode, StringComparison.Ordinal);
        Assert.Contains("/// Adds a handler for the FilterChanged attached routed event.", generatedCode, StringComparison.Ordinal);
        Assert.Contains("/// Removes a handler for the FilterChanged attached routed event.", generatedCode, StringComparison.Ordinal);
    }
}