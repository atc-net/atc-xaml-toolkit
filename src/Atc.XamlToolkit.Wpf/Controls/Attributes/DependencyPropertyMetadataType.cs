namespace Atc.XamlToolkit.Controls.Attributes;

/// <summary>
/// The metadata class the source generator creates for a dependency or attached property.
/// </summary>
public enum DependencyPropertyMetadataType
{
    /// <summary>
    /// The generator chooses: <c>FrameworkPropertyMetadata</c> when <c>Flags</c>,
    /// <c>DefaultUpdateSourceTrigger</c> or <c>IsAnimationProhibited</c> is set, otherwise
    /// <c>PropertyMetadata</c>.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// <c>System.Windows.PropertyMetadata</c>.
    /// </summary>
    PropertyMetadata = 1,

    /// <summary>
    /// <c>System.Windows.UIPropertyMetadata</c>.
    /// </summary>
    UIPropertyMetadata = 2,

    /// <summary>
    /// <c>System.Windows.FrameworkPropertyMetadata</c>, also when no flag is set.
    /// </summary>
    FrameworkPropertyMetadata = 3,
}