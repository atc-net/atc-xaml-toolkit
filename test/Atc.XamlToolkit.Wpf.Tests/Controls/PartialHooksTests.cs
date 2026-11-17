namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Runs the code the source generator emits for <c>GeneratePartialHooks = true</c>
/// on dependency and attached properties against real WPF.
/// </summary>
public sealed class PartialHooksTests
{
    [Fact]
    public void SettingADependencyProperty_CallsTheTypedHook_WithOldAndNewValue()
        => StaThreadRunner.Run(() =>
        {
            var sut = new PartialHooksControl();

            sut.Count = 5;
            sut.Count = 8;

            Assert.Equal(["Count 0->5", "Count 5->8"], sut.Calls);
        });

    [Fact]
    public void SettingTheSameValue_DoesNotCallTheHook()
        => StaThreadRunner.Run(() =>
        {
            var sut = new PartialHooksControl { Count = 5 };
            sut.Calls.Clear();

            sut.Count = 5;

            Assert.Empty(sut.Calls);
        });

    [Fact]
    public void APropertyWithItsOwnCallbackAndAHook_CallsBoth_TheCallbackFirst()
        => StaThreadRunner.Run(() =>
        {
            var sut = new PartialHooksControl();

            sut.Title = "Hello";

            Assert.Equal(["Title static callback", "Title null->Hello"], sut.Calls);
        });

    [Fact]
    public void AHookThatIsNotImplemented_IsSimplyNotCalled()
        => StaThreadRunner.Run(() =>
        {
            var sut = new PartialHooksControl();

            sut.IsActive = true;

            Assert.True(sut.IsActive);
            Assert.Empty(sut.Calls);
        });

    [Fact]
    public void SettingAnAttachedProperty_CallsTheStaticHook_WithTheTargetObject()
        => StaThreadRunner.Run(() =>
        {
            PartialHooksAttach.Calls.Clear();
            var button = new System.Windows.Controls.Button();

            PartialHooksAttach.SetIsHighlighted(button, value: true);

            Assert.Equal(["Button False->True"], PartialHooksAttach.Calls);
        });
}