namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Runs the code the source generator emits for <c>GenerateCoerceHook = true</c> against real WPF.
/// </summary>
public sealed class CoerceHookTests
{
    [Fact]
    public void TheTypedCoerceMethod_DecidesTheEffectiveValue()
        => StaThreadRunner.Run(() =>
        {
            var sut = new CoerceHookControl();

            sut.Maximum = -5;
            Assert.Equal(0, sut.Maximum);

            sut.Maximum = 7;
            Assert.Equal(7, sut.Maximum);
        });

    [Fact]
    public void ANullableProperty_CanBeCoercedFromAndToNull()
        => StaThreadRunner.Run(() =>
        {
            var sut = new CoerceHookControl();

            sut.Title = "  Printer  ";
            Assert.Equal("Printer", sut.Title);

            sut.Title = null;
            Assert.Null(sut.Title);
        });

    [Fact]
    public void ANamedCoerceCallback_RunsBeforeTheTypedMethod()
        => StaThreadRunner.Run(() =>
        {
            var sut = new CoerceHookControl();

            sut.Steps = 3;
            Assert.Equal(6, sut.Steps);

            sut.Steps = 8;
            Assert.Equal(10, sut.Steps);
        });

    [Fact]
    public void AnAttachedProperty_IsCoercedWithItsTargetObject()
        => StaThreadRunner.Run(() =>
        {
            var button = new System.Windows.Controls.Button();
            var border = new System.Windows.Controls.Border();

            CoerceHookAttach.SetZoomFactor(button, 9);
            CoerceHookAttach.SetZoomFactor(border, 9);

            Assert.Equal(4, CoerceHookAttach.GetZoomFactor(button));
            Assert.Equal(9, CoerceHookAttach.GetZoomFactor(border));
        });
}