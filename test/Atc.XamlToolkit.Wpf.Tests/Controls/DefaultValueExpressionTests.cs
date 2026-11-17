namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Runs the code the source generator emits for <c>DefaultValueExpression</c> against real WPF.
/// </summary>
public sealed class DefaultValueExpressionTests
{
    [Fact]
    public void AMethodCall_IsTheDefaultValue_OfABrushProperty()
        => StaThreadRunner.Run(() =>
        {
            var sut = new DefaultValueExpressionControl();

            var brush = Assert.IsType<System.Windows.Media.SolidColorBrush>(sut.GridBrush);
            Assert.True(brush.IsFrozen);
            Assert.Equal(System.Windows.Media.Color.FromRgb(40, 40, 40), brush.Color);
        });

    [Fact]
    public void AnExpression_IsNotQuoted_ForAStringProperty()
        => StaThreadRunner.Run(() =>
        {
            var sut = new DefaultValueExpressionControl();

            Assert.Equal("#FF000000", sut.ColorText);
        });

    [Fact]
    public void AGenericMethodCall_IsTheDefaultValue()
        => StaThreadRunner.Run(() =>
        {
            var sut = new DefaultValueExpressionControl();

            Assert.Same(Array.Empty<object>(), sut.Items);
        });

    [Fact]
    public void FlagsNone_GivesFrameworkPropertyMetadata_AndNoFlagsGivesPropertyMetadata()
        => StaThreadRunner.Run(() =>
        {
            var withFlagsNone = DefaultValueExpressionControl.CountProperty.GetMetadata(typeof(DefaultValueExpressionControl));
            var withoutFlags = DefaultValueExpressionControl.StepProperty.GetMetadata(typeof(DefaultValueExpressionControl));

            Assert.Equal(typeof(System.Windows.FrameworkPropertyMetadata), withFlagsNone.GetType());
            Assert.Equal(typeof(System.Windows.PropertyMetadata), withoutFlags.GetType());
            Assert.Equal(3, withFlagsNone.DefaultValue);
        });
}