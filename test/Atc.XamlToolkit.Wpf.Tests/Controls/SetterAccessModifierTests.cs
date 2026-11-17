namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Checks the code the source generator emits for <c>SetterAccessModifier</c> in a real project.
/// </summary>
public sealed class SetterAccessModifierTests
{
    [Fact]
    public void DependencyProperty_HasTheRequestedSetterAccessibility()
    {
        var total = typeof(SetterAccessControl).GetProperty(nameof(SetterAccessControl.Total))!;
        var step = typeof(SetterAccessControl).GetProperty(nameof(SetterAccessControl.Step))!;

        Assert.True(total.GetMethod!.IsPublic);
        Assert.True(total.SetMethod!.IsPrivate);
        Assert.True(step.SetMethod!.IsPublic);
    }

    [Fact]
    public void DependencyProperty_WithAPrivateSetter_CanStillBeSetByTheControlItself()
        => StaThreadRunner.Run(() =>
        {
            var sut = new SetterAccessControl();

            sut.Increase();
            sut.Increase();

            Assert.Equal(2, sut.Total);
        });

    [Fact]
    public void DependencyProperty_WithAPrivateSetter_IsStillAnOrdinaryWritableDependencyProperty()
        => StaThreadRunner.Run(() =>
        {
            var sut = new SetterAccessControl();

            sut.SetValue(SetterAccessControl.TotalProperty, 7);

            Assert.False(SetterAccessControl.TotalProperty.ReadOnly);
            Assert.Equal(7, sut.Total);
        });

    [Fact]
    public void ObservableProperty_HasTheRequestedSetterAccessibility()
    {
        var name = typeof(SetterAccessViewModel).GetProperty(nameof(SetterAccessViewModel.Name))!;
        var count = typeof(SetterAccessViewModel).GetProperty(nameof(SetterAccessViewModel.Count))!;
        var isActive = typeof(SetterAccessViewModel).GetProperty(nameof(SetterAccessViewModel.IsActive))!;

        Assert.True(name.GetMethod!.IsPublic);
        Assert.True(name.SetMethod!.IsAssembly);
        Assert.True(count.SetMethod!.IsPrivate);
        Assert.True(isActive.SetMethod!.IsPublic);
    }

    [Fact]
    public void ObservableProperty_WithANonPublicSetter_StillRaisesPropertyChanged()
    {
        var sut = new SetterAccessViewModel();
        var changed = new List<string?>();
        sut.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        sut.Increase();
        sut.Name = "Printer";

        Assert.Equal([nameof(SetterAccessViewModel.Count), nameof(SetterAccessViewModel.Name)], changed);
    }
}