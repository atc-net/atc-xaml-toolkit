namespace Atc.XamlToolkit.Wpf.Tests.XUnitTestTypes;

/// <summary>
/// The members every picker in the tests has, declared once.
/// </summary>
/// <typeparam name="TInfo">The type of the selected item.</typeparam>
[Atc.XamlToolkit.Controls.Attributes.DependencyPropertyTemplate]
[SuppressMessage("Major Code Smell", "S2743:Static fields should not be used in generic types", Justification = "The fields only declare routed events for the controls that use the template.")]
internal abstract class TemplatePickerMembers<TInfo>
{
    /// <summary>
    /// Occurs when the selected item changes.
    /// </summary>
    [Atc.XamlToolkit.Controls.Attributes.RoutedEvent(
        HandlerType = typeof(System.Windows.RoutedPropertyChangedEventHandler<>),
        GenerateRaiseMethod = true)]
    private static readonly System.Windows.RoutedEvent valueChanged;

    [Atc.XamlToolkit.Controls.Attributes.RoutedEvent(GenerateRaiseMethod = true)]
    private static readonly System.Windows.RoutedEvent refreshed;

    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(
        Flags = System.Windows.FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
        GeneratePartialHooks = true)]
    private TInfo value;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(DefaultValue = true)]
    private bool showRefreshButton;

    [Atc.XamlToolkit.Controls.Attributes.DependencyProperty(DefaultValue = "")]
    private string watermarkText;
}