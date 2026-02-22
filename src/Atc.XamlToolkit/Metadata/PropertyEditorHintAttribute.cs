namespace Atc.XamlToolkit.Metadata;

/// <summary>
/// Specifies a hint to the UI editor about which control type to use for editing a property,
/// overriding the default control selection based on the property's type.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class PropertyEditorHintAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyEditorHintAttribute"/> class.
    /// </summary>
    /// <param name="hint">The editor hint.</param>
    public PropertyEditorHintAttribute(EditorHint hint)
    {
        Hint = hint;
    }

    /// <summary>
    /// Gets the editor hint for the property.
    /// </summary>
    public EditorHint Hint { get; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(Hint)}: {Hint}";
}