namespace Atc.XamlToolkit.Wpf.Tests.Controls;

/// <summary>
/// Runs the code the source generator emits for <c>[TemplatePartField]</c> against real WPF.
/// </summary>
public sealed class TemplatePartFieldTests
{
    private const string TemplateStart =
        "<ControlTemplate xmlns='http://schemas.microsoft.com/winfx/2006/xaml/presentation' " +
        "xmlns:x='http://schemas.microsoft.com/winfx/2006/xaml'><Grid>";

    private const string TemplateEnd = "</Grid></ControlTemplate>";

    [Fact]
    public void ApplyingTheTemplate_AssignsThePartsToTheirFields()
        => StaThreadRunner.Run(() =>
        {
            var sut = new TemplatePartsControl
            {
                Template = ParseTemplate("<Border x:Name='PART_Overlay' /><Button x:Name='PART_CloseButton' />"),
            };

            sut.ApplyTemplate();

            Assert.NotNull(sut.OverlayBorder);
            Assert.NotNull(sut.CloseButton);
        });

    [Fact]
    public void ApplyingATemplateWithoutAnOptionalPart_LeavesItsFieldNull()
        => StaThreadRunner.Run(() =>
        {
            var sut = new TemplatePartsControl
            {
                Template = ParseTemplate("<Button x:Name='PART_CloseButton' />"),
            };

            sut.ApplyTemplate();

            Assert.Null(sut.OverlayBorder);
            Assert.NotNull(sut.CloseButton);
        });

    [Fact]
    public void ApplyingATemplateWithoutARequiredPart_Throws()
        => StaThreadRunner.Run(() =>
        {
            var sut = new TemplatePartsControl
            {
                Template = ParseTemplate("<Border x:Name='PART_Overlay' />"),
            };

            var exception = Assert.Throws<InvalidOperationException>(() => sut.ApplyTemplate());

            Assert.Contains("PART_CloseButton", exception.Message, StringComparison.Ordinal);
        });

    [Fact]
    public void TheClass_DeclaresItsTemplateParts()
    {
        var templateParts = typeof(TemplatePartsControl)
            .GetCustomAttributes(typeof(System.Windows.TemplatePartAttribute), inherit: false)
            .Cast<System.Windows.TemplatePartAttribute>()
            .ToDictionary(x => x.Name, x => x.Type, StringComparer.Ordinal);

        Assert.Equal(2, templateParts.Count);
        Assert.Equal(typeof(System.Windows.Controls.Border), templateParts["PART_Overlay"]);
        Assert.Equal(typeof(System.Windows.Controls.Button), templateParts["PART_CloseButton"]);
    }

    private static System.Windows.Controls.ControlTemplate ParseTemplate(
        string content)
        => (System.Windows.Controls.ControlTemplate)System.Windows.Markup.XamlReader.Parse(TemplateStart + content + TemplateEnd);
}