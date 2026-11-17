namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Runs the code the source generator emits for <c>MetadataType</c> and related options
/// against real WPF.
/// </summary>
public sealed class MetadataTypeTests
{
    private static Type MetadataTypeOf(DependencyProperty property)
        => property.GetMetadata(typeof(MetadataTypeControl)).GetType();

    [Fact]
    public void TheMetadataClass_IsTheOneAskedFor()
    {
        Assert.Equal(typeof(UIPropertyMetadata), MetadataTypeOf(MetadataTypeControl.UiCountProperty));
        Assert.Equal(typeof(UIPropertyMetadata), MetadataTypeOf(MetadataTypeControl.UiFixedProperty));
        Assert.Equal(typeof(UIPropertyMetadata), MetadataTypeOf(MetadataTypeControl.UiBrushProperty));
        Assert.Equal(typeof(PropertyMetadata), MetadataTypeOf(MetadataTypeControl.PlainCountProperty));
        Assert.Equal(typeof(FrameworkPropertyMetadata), MetadataTypeOf(MetadataTypeControl.FrameworkCountProperty));
        Assert.Equal(typeof(PropertyMetadata), MetadataTypeOf(MetadataTypeControl.AutoCountProperty));
    }

    [Fact]
    public void DefaultValuesAndCallbacks_WorkWithEveryMetadataClass()
        => StaThreadRunner.Run(() =>
        {
            var sut = new MetadataTypeControl();

            Assert.Equal(3, sut.UiCount);
            Assert.Equal(0d, sut.UiFixed);
            Assert.Null(sut.UiBrush);
            Assert.Equal(4, sut.PlainCount);
            Assert.Equal(5, sut.FrameworkCount);

            sut.UiCount = 9;
            sut.PlainCount = -1;

            Assert.Equal(1, sut.UiCountChanges);
            Assert.Equal(0, sut.PlainCount);
        });

    [Fact]
    public void IsAnimationProhibited_IsCarriedByUIPropertyMetadata()
    {
        var metadata = (UIPropertyMetadata)MetadataTypeControl.UiFixedProperty.GetMetadata(typeof(MetadataTypeControl));

        Assert.True(metadata.IsAnimationProhibited);
    }

    [Fact]
    public void AnAttachedProperty_GetsTheMetadataClassAskedFor()
        => StaThreadRunner.Run(() =>
        {
            var panel = new System.Windows.Controls.StackPanel();

            Assert.Equal(typeof(UIPropertyMetadata), MetadataTypeAttach.SpacingProperty.DefaultMetadata.GetType());
            Assert.Equal(2d, MetadataTypeAttach.GetSpacing(panel));
        });

    [Fact]
    public void BrowsableForTargetType_PutsTheAttributeOnTheGetMethodOnly()
    {
        var withOption = typeof(MetadataTypeAttach).GetMethod(nameof(MetadataTypeAttach.GetSpacing))!;
        var setMethod = typeof(MetadataTypeAttach).GetMethod(nameof(MetadataTypeAttach.SetSpacing))!;
        var withoutOption = typeof(MetadataTypeAttach).GetMethod(nameof(MetadataTypeAttach.GetPadding))!;

        var attribute = Assert.Single(withOption.GetCustomAttributes(typeof(AttachedPropertyBrowsableForTypeAttribute), inherit: false));
        Assert.Equal(typeof(System.Windows.Controls.Panel), ((AttachedPropertyBrowsableForTypeAttribute)attribute).TargetType);
        Assert.Empty(setMethod.GetCustomAttributes(typeof(AttachedPropertyBrowsableForTypeAttribute), inherit: false));
        Assert.Empty(withoutOption.GetCustomAttributes(typeof(AttachedPropertyBrowsableForTypeAttribute), inherit: false));
    }

    [Fact]
    public void AnAttachedProperty_CanShareAnExistingProperty()
        => StaThreadRunner.Run(() =>
        {
            var button = new System.Windows.Controls.Button();

            Assert.Same(System.Windows.Documents.TextElement.FontSizeProperty, MetadataTypeAttach.FontSizeProperty);

            MetadataTypeAttach.SetFontSize(button, 21);

            Assert.Equal(21d, MetadataTypeAttach.GetFontSize(button));
            Assert.Equal(21d, button.FontSize);
        });

    [Fact]
    public void AReadOnlyProperty_OnAReadonlyField_HasNoSetter_AndIsWrittenThroughTheKey()
        => StaThreadRunner.Run(() =>
        {
            var sut = new MetadataTypeControl();
            var property = typeof(MetadataTypeControl).GetProperty(nameof(MetadataTypeControl.SummaryText))!;

            sut.Summarize("3 selected");

            Assert.Null(property.SetMethod);
            Assert.True(MetadataTypeControl.SummaryTextProperty.ReadOnly);
            Assert.Equal("3 selected", sut.SummaryText);
        });

    [Fact]
    public void AddOwnerOf_WithFlagsAndNoDefaultValue_KeepsTheDefaultOfTheSharedProperty()
        => StaThreadRunner.Run(() =>
        {
            var sut = new AddOwnerFlagsControl();
            var metadata = Assert.IsType<FrameworkPropertyMetadata>(
                AddOwnerFlagsControl.MaxDropDownHeightProperty.GetMetadata(typeof(AddOwnerFlagsControl)));

            Assert.True(metadata.AffectsMeasure);
            Assert.Equal(new System.Windows.Controls.ComboBox().MaxDropDownHeight, sut.MaxDropDownHeight);
            Assert.NotEqual(0d, sut.MaxDropDownHeight);
        });
}