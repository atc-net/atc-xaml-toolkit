namespace Atc.XamlToolkit.Metadata;

/// <summary>
/// Specifies a hint to the UI editor about which control type to use for editing a property.
/// </summary>
public enum EditorHint
{
    /// <summary>
    /// The editor should use its default control for the property type.
    /// </summary>
    Default = 0,

    /// <summary>
    /// The property should be edited with a slider control.
    /// </summary>
    Slider,

    /// <summary>
    /// The property should be edited with a color picker control.
    /// </summary>
    ColorPicker,

    /// <summary>
    /// The property should be edited with a file path picker control.
    /// </summary>
    FilePath,

    /// <summary>
    /// The property should be edited with a directory path picker control.
    /// </summary>
    DirectoryPath,

    /// <summary>
    /// The property should be edited with a multi-line text control.
    /// </summary>
    MultiLineText,

    /// <summary>
    /// The property should be edited with a password input control.
    /// </summary>
    Password,

    /// <summary>
    /// The property should be displayed as read-only.
    /// </summary>
    ReadOnly,
}