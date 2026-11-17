namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Runs the code the source generator emits for <c>AddOwnerOf</c> against real WPF.
/// </summary>
public sealed class AddOwnerDependencyPropertyTests
{
    [Fact]
    public void TheProperty_IsTheSharedProperty_NotANewOne()
        => StaThreadRunner.Run(() =>
        {
            Assert.Same(System.Windows.Controls.ComboBox.MaxDropDownHeightProperty, AddOwnerControl.MaxDropDownHeightProperty);
            Assert.Same(System.Windows.Controls.TextBox.TextAlignmentProperty, AddOwnerControl.TextAlignmentProperty);
            Assert.Same(System.Windows.Controls.Primitives.TextBoxBase.IsReadOnlyProperty, AddOwnerControl.IsReadOnlyProperty);
        });

    [Fact]
    public void WithoutMetadata_TheDefaultValueOfTheSharedProperty_IsKept()
        => StaThreadRunner.Run(() =>
        {
            var sut = new AddOwnerControl();
            var comboBox = new System.Windows.Controls.ComboBox();

            Assert.Equal(comboBox.MaxDropDownHeight, sut.MaxDropDownHeight);
            Assert.NotEqual(0d, sut.MaxDropDownHeight);
        });

    [Fact]
    public void TheClrProperty_ReadsAndWritesTheSharedProperty()
        => StaThreadRunner.Run(() =>
        {
            var sut = new AddOwnerControl { MaxDropDownHeight = 123 };

            Assert.Equal(123d, sut.GetValue(System.Windows.Controls.ComboBox.MaxDropDownHeightProperty));
        });

    [Fact]
    public void ATypedChangeHook_RunsForTheNewOwner_AndKeepsTheSharedDefault()
        => StaThreadRunner.Run(() =>
        {
            var sut = new AddOwnerControl();

            Assert.Equal(System.Windows.TextAlignment.Left, sut.TextAlignment);

            sut.TextAlignment = System.Windows.TextAlignment.Right;

            Assert.Equal(["TextAlignment:Left->Right"], sut.Changes);
        });

    [Fact]
    public void ADefaultValue_IsOverriddenForTheNewOwnerOnly()
        => StaThreadRunner.Run(() =>
        {
            var sut = new AddOwnerControl();

            Assert.Equal(5d, sut.Maximum);
            Assert.True(sut.IsReadOnly);
            Assert.False(new System.Windows.Controls.TextBox().IsReadOnly);
        });

    [Fact]
    public void ANamedCallback_WithDefaultValueAndFlags_IsUsedForTheNewOwner()
        => StaThreadRunner.Run(() =>
        {
            var sut = new AddOwnerControl();

            sut.IsReadOnly = false;

            Assert.Equal(["IsReadOnly:True->False"], sut.Changes);
            var metadata = Assert.IsType<System.Windows.FrameworkPropertyMetadata>(
                AddOwnerControl.IsReadOnlyProperty.GetMetadata(typeof(AddOwnerControl)));
            Assert.True(metadata.Inherits);
        });
}