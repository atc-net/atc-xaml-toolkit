namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Runs the code the source generator emits for <c>[DefaultStyleKey]</c> against real WPF.
/// </summary>
public sealed class DefaultStyleKeyTests
{
    [Fact]
    public void Control_UsesItsOwnTypeAsDefaultStyleKey()
        => StaThreadRunner.Run(() =>
        {
            var sut = new StyleKeyedControl();

            Assert.Equal(typeof(StyleKeyedControl), sut.ExposedDefaultStyleKey);
        });

    [Fact]
    public void Control_UsesTheGivenTypeAsDefaultStyleKey()
        => StaThreadRunner.Run(() =>
        {
            var sut = new RestyledButton();

            Assert.Equal(typeof(System.Windows.Controls.Button), sut.ExposedDefaultStyleKey);
        });

    [Fact]
    public void ControlWithItsOwnStaticConstructor_UsesItsOwnTypeAsDefaultStyleKey()
        => StaThreadRunner.Run(() =>
        {
            var sut = new StyleKeyedControlWithStaticConstructor();

            Assert.Equal(typeof(StyleKeyedControlWithStaticConstructor), sut.ExposedDefaultStyleKey);
            Assert.False(sut.Focusable);
        });
}