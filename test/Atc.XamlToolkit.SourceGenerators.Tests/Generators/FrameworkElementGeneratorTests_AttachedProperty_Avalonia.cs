namespace Atc.XamlToolkit.SourceGenerators.Tests.Generators;

[SuppressMessage("Design", "MA0048:File name must match type name", Justification = "OK.")]
public sealed partial class FrameworkElementGeneratorTests
{
    [Theory]
    [InlineData(
        """
        [AttachedProperty<bool>("IsDraggable", DefaultValue = false, PropertyChangedCallback = nameof(OnIsDraggableChanged))]
        public partial class DragBehavior : Avalonia.AvaloniaObject
        {
        """)]
    [InlineData(
        """
        public partial class DragBehavior : Avalonia.AvaloniaObject
        {
            [AttachedProperty(PropertyChangedCallback = nameof(OnIsDraggableChanged))]
            private static bool isDraggable;

        """)]
    public void AttachedProperty_OnAvalonia_PropertyChangedCallback_IsRegistered(
        string classStart)
    {
        var inputCode =
            $$"""
              using Atc.XamlToolkit.Controls.Attributes;

              namespace TestNamespace;

              {{classStart}}
                  // A hand-written static constructor must stay possible.
                  static DragBehavior()
                  {
                  }

                  private static void OnIsDraggableChanged(
                      Avalonia.AvaloniaObject d,
                      Avalonia.AvaloniaPropertyChangedEventArgs e)
                  {
                  }
              }
              """;

        AssertCompilesWithAllGenerators(inputCode);

        // An Avalonia type in the compilation selects the Avalonia platform.
        const string avaloniaStub =
            """
            namespace Avalonia
            {
                public class AvaloniaProperty { }
                public class AvaloniaObject { }
                public class AvaloniaPropertyChangedEventArgs { }
            }
            """;

        var generatedCode = RunGenerator<FrameworkElementGenerator>(avaloniaStub, inputCode)
            .GeneratorResult
            .GeneratedSources[0]
            .SourceText
            .ToString();

        Assert.Contains(
            "IsDraggableProperty.Changed.AddClassHandler<AvaloniaObject>(OnIsDraggableChanged)",
            generatedCode,
            StringComparison.Ordinal);
    }
}