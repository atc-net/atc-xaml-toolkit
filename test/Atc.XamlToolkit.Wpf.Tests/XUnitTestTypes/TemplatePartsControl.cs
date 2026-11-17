namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

public partial class TemplatePartsControl : System.Windows.Controls.Control
{
    private const string PartCloseButton = "PART_CloseButton";

    [Atc.XamlToolkit.Controls.Attributes.TemplatePartField("PART_Overlay")]
    private System.Windows.Controls.Border? overlayBorder;

    [Atc.XamlToolkit.Controls.Attributes.TemplatePartField(PartCloseButton, IsRequired = true)]
    private System.Windows.Controls.Button closeButton = null!;

    public System.Windows.Controls.Border? OverlayBorder => overlayBorder;

    public System.Windows.Controls.Button CloseButton => closeButton;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        ResolveTemplateParts();
    }
}