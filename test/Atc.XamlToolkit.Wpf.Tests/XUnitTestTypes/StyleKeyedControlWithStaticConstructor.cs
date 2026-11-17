namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

[Atc.XamlToolkit.Controls.Attributes.DefaultStyleKey]
public partial class StyleKeyedControlWithStaticConstructor : System.Windows.Controls.Control
{
    [SuppressMessage("Performance", "CA1810:Initialize reference type static fields inline", Justification = "The test is about a class that has a static constructor of its own.")]
    static StyleKeyedControlWithStaticConstructor()
    {
        OverrideDefaultStyleKey();
        FocusableProperty.OverrideMetadata(
            typeof(StyleKeyedControlWithStaticConstructor),
            new System.Windows.FrameworkPropertyMetadata(defaultValue: false));
    }

    public object ExposedDefaultStyleKey => DefaultStyleKey;
}