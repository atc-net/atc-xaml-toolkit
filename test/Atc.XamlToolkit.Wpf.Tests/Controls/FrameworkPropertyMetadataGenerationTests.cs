namespace Atc.XamlToolkit.Wpf.Tests.Controls;

public sealed class FrameworkPropertyMetadataGenerationTests
{
    [Fact]
    public void Flags_IsAnimationProhibited()
    {
        var metadata = GetControlMetadata(FrameworkPropertyMetadataTestControl.FlagsAndIsAnimationProhibitedProperty);

        Assert.True(metadata.BindsTwoWayByDefault);
        Assert.True(metadata.Journal);
        Assert.True(metadata.IsAnimationProhibited);
        Assert.Equal(UpdateSourceTrigger.PropertyChanged, metadata.DefaultUpdateSourceTrigger);
        Assert.Null(metadata.PropertyChangedCallback);
        Assert.Null(metadata.CoerceValueCallback);
    }

    [Fact]
    public void Flags_IsAnimationProhibited_DefaultUpdateSourceTrigger()
    {
        var metadata = GetControlMetadata(FrameworkPropertyMetadataTestControl.FlagsAndIsAnimationProhibitedAndUpdateSourceTriggerProperty);

        Assert.Equal(0m, metadata.DefaultValue);
        Assert.True(metadata.BindsTwoWayByDefault);
        Assert.True(metadata.Journal);
        Assert.True(metadata.IsAnimationProhibited);
        Assert.Equal(UpdateSourceTrigger.LostFocus, metadata.DefaultUpdateSourceTrigger);
    }

    [Fact]
    public void Flags_DefaultUpdateSourceTrigger()
    {
        var metadata = GetControlMetadata(FrameworkPropertyMetadataTestControl.FlagsAndUpdateSourceTriggerProperty);

        Assert.True(metadata.BindsTwoWayByDefault);
        Assert.False(metadata.IsAnimationProhibited);
        Assert.Equal(UpdateSourceTrigger.LostFocus, metadata.DefaultUpdateSourceTrigger);
    }

    [Fact]
    public void Flags_CoerceValueCallback()
    {
        var metadata = GetControlMetadata(FrameworkPropertyMetadataTestControl.FlagsAndCoerceValueCallbackProperty);

        Assert.True(metadata.AffectsMeasure);
        Assert.NotNull(metadata.CoerceValueCallback);
        Assert.Null(metadata.PropertyChangedCallback);
    }

    [Fact]
    public void PropertyChangedCallback_DefaultUpdateSourceTrigger()
    {
        var metadata = GetControlMetadata(FrameworkPropertyMetadataTestControl.PropertyChangedCallbackAndUpdateSourceTriggerProperty);

        Assert.NotNull(metadata.PropertyChangedCallback);
        Assert.False(metadata.BindsTwoWayByDefault);
        Assert.Equal(UpdateSourceTrigger.PropertyChanged, metadata.DefaultUpdateSourceTrigger);
    }

    [Fact]
    public void IsAnimationProhibited_DefaultUpdateSourceTrigger()
    {
        var metadata = GetControlMetadata(FrameworkPropertyMetadataTestControl.IsAnimationProhibitedAndUpdateSourceTriggerProperty);

        Assert.True(metadata.IsAnimationProhibited);
        Assert.Equal(UpdateSourceTrigger.LostFocus, metadata.DefaultUpdateSourceTrigger);
    }

    [Fact]
    public void IsAnimationProhibited()
    {
        var metadata = GetControlMetadata(FrameworkPropertyMetadataTestControl.IsAnimationProhibitedOnlyProperty);

        Assert.True(metadata.IsAnimationProhibited);
    }

    [Fact]
    public void CoerceValueCallback()
    {
        var metadata = FrameworkPropertyMetadataTestControl.CoerceValueCallbackOnlyProperty
            .GetMetadata(typeof(FrameworkPropertyMetadataTestControl));

        Assert.NotNull(metadata.CoerceValueCallback);
        Assert.Null(metadata.PropertyChangedCallback);
    }

    [Fact]
    public void AttachedProperty_Flags_IsAnimationProhibited()
    {
        var metadata = GetAttachedMetadata(FrameworkPropertyMetadataTestAttach.FlagsAndIsAnimationProhibitedProperty);

        Assert.True(metadata.BindsTwoWayByDefault);
        Assert.True(metadata.IsAnimationProhibited);
    }

    [Fact]
    public void AttachedProperty_IsAnimationProhibited_DefaultUpdateSourceTrigger()
    {
        var metadata = GetAttachedMetadata(FrameworkPropertyMetadataTestAttach.IsAnimationProhibitedAndUpdateSourceTriggerProperty);

        Assert.True(metadata.IsAnimationProhibited);
        Assert.Equal(UpdateSourceTrigger.LostFocus, metadata.DefaultUpdateSourceTrigger);
    }

    private static FrameworkPropertyMetadata GetControlMetadata(
        DependencyProperty property)
        => Assert.IsType<FrameworkPropertyMetadata>(
            property.GetMetadata(typeof(FrameworkPropertyMetadataTestControl)));

    private static FrameworkPropertyMetadata GetAttachedMetadata(
        DependencyProperty property)
        => Assert.IsType<FrameworkPropertyMetadata>(
            property.GetMetadata(typeof(DependencyObject)));
}