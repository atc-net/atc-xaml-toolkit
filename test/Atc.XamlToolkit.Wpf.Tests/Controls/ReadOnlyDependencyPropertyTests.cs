namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Runs the code the source generator emits for <c>IsReadOnly = true</c> against real WPF.
/// </summary>
public sealed class ReadOnlyDependencyPropertyTests
{
    [Fact]
    public void TheProperty_IsRegisteredAsReadOnly_WithItsDefaultValue()
        => StaThreadRunner.Run(() =>
        {
            var sut = new ReadOnlyPropertyControl();

            Assert.True(ReadOnlyPropertyControl.SummaryTextProperty.ReadOnly);
            Assert.True(ReadOnlyPropertyControl.SelectedCountProperty.ReadOnly);
            Assert.Null(sut.SummaryText);
            Assert.Equal(3, sut.SelectedCount);
        });

    [Fact]
    public void TheControlItself_CanSetTheProperty()
        => StaThreadRunner.Run(() =>
        {
            var sut = new ReadOnlyPropertyControl();

            sut.Select(count: 2, summary: "2 selected");

            Assert.Equal(2, sut.SelectedCount);
            Assert.Equal("2 selected", sut.SummaryText);
        });

    [Fact]
    public void SettingThePropertyFromOutside_Throws()
        => StaThreadRunner.Run(() =>
        {
            var sut = new ReadOnlyPropertyControl();

            Assert.Throws<InvalidOperationException>(
                () => sut.SetValue(ReadOnlyPropertyControl.SummaryTextProperty, "from outside"));
        });

    [Fact]
    public void TheClrProperty_HasAPrivateSetter()
    {
        var property = typeof(ReadOnlyPropertyControl).GetProperty(nameof(ReadOnlyPropertyControl.SummaryText))!;

        Assert.True(property.GetMethod!.IsPublic);
        Assert.True(property.SetMethod!.IsPrivate);
    }

    [Fact]
    public void AnAttachedProperty_IsReadOnlyFromOutside_AndWritableByItsOwner()
        => StaThreadRunner.Run(() =>
        {
            var button = new System.Windows.Controls.Button();

            Assert.True(ReadOnlyPropertyAttach.IsFilteredProperty.ReadOnly);
            Assert.False(ReadOnlyPropertyAttach.GetIsFiltered(button));

            ReadOnlyPropertyAttach.MarkAsFiltered(button);

            Assert.True(ReadOnlyPropertyAttach.GetIsFiltered(button));
            Assert.Throws<InvalidOperationException>(
                () => button.SetValue(ReadOnlyPropertyAttach.IsFilteredProperty, false));
        });
}