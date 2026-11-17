namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Problems found while converting a consumer library, checked in a real project.
/// </summary>
public sealed class ConsumerFindingsTests
{
    private const System.Reflection.BindingFlags AllInstance =
        System.Reflection.BindingFlags.Instance |
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.NonPublic;

    [Fact]
    public void AReadOnlyProperty_WithAnExplicitPublicSetter_HasAPublicSetterThatWritesThroughTheKey()
        => StaThreadRunner.Run(() =>
        {
            var property = typeof(ConsumerFindingsControl).GetProperty(nameof(ConsumerFindingsControl.IsDragging))!;
            var sut = new ConsumerFindingsControl { IsDragging = true };

            Assert.True(property.SetMethod!.IsPublic);
            Assert.True(ConsumerFindingsControl.IsDraggingProperty.ReadOnly);
            Assert.True(sut.IsDragging);
        });

    [Fact]
    public void PropertyName_NamesTheProperty_SoTheFieldNameIsFreeForLocals()
        => StaThreadRunner.Run(() =>
        {
            var sut = new ConsumerFindingsControl();

            Assert.Equal("State", ConsumerFindingsControl.StateProperty.Name);
            Assert.Equal(1, sut.NextState());
            Assert.Equal(1, sut.State);
        });

    [Fact]
    public void PropertyName_OnAnAttachedProperty_NamesThePropertyAndItsAccessors()
        => StaThreadRunner.Run(() =>
        {
            var button = new System.Windows.Controls.Button();

            ConsumerFindingsAttach.SetDock(button, System.Windows.Controls.Dock.Right);

            Assert.Equal("Dock", ConsumerFindingsAttach.DockProperty.Name);
            Assert.Equal(System.Windows.Controls.Dock.Right, ConsumerFindingsAttach.GetDock(button));
        });

    [Fact]
    public void TheRaiseMethodAccessModifier_IsTheAccessibilityOfTheRaiseMethod()
    {
        var method = typeof(ConsumerFindingsControl).GetMethod("RaiseOpened", AllInstance)!;

        Assert.True(method.IsAssembly);
    }

    [Fact]
    public void INotifyPropertyChanged_OnASealedClass_HasPrivateHelpers_AndRaisesTheEvent()
    {
        var sut = new SealedNotifyingItem();
        var changed = new List<string?>();
        sut.PropertyChanged += (_, e) => changed.Add(e.PropertyName);

        sut.IsChecked = true;
        sut.Label = "Printer";

        Assert.Equal([nameof(SealedNotifyingItem.IsChecked), nameof(SealedNotifyingItem.Label)], changed);
        Assert.True(typeof(SealedNotifyingItem).GetMethod("RaisePropertyChanged", AllInstance)!.IsPrivate);
        Assert.True(typeof(SealedNotifyingItem).GetMethod("OnPropertyChanged", AllInstance)!.IsPrivate);
    }
}