namespace Atc.XamlToolkit.Metadata;

/// <summary>
/// Represents the resolved metadata for a single property, combining information
/// from <see cref="PropertyDisplayAttribute"/>, <see cref="PropertyRangeAttribute"/>,
/// <see cref="PropertyEditorHintAttribute"/>, and other sources.
/// </summary>
public sealed class PropertyMetadataInfo
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyMetadataInfo"/> class.
    /// </summary>
    /// <param name="propertyName">The CLR property name.</param>
    /// <param name="displayName">The friendly display name.</param>
    /// <param name="propertyType">The type of the property.</param>
    public PropertyMetadataInfo(
        string propertyName,
        string displayName,
        Type propertyType)
    {
        PropertyName = propertyName;
        DisplayName = displayName;
        PropertyType = propertyType;
    }

    /// <summary>
    /// Gets the CLR property name.
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// Gets the friendly display name for the property.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the type of the property.
    /// </summary>
    public Type PropertyType { get; }

    /// <summary>
    /// Gets or sets the group name for categorization.
    /// Defaults to "General".
    /// </summary>
    public string GroupName { get; set; } = "General";

    /// <summary>
    /// Gets or sets the description text, typically shown as a tooltip.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the sort order within the group.
    /// </summary>
    public int Order { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets a value indicating whether the property is read-only.
    /// </summary>
    public bool IsReadOnly { get; set; }

    /// <summary>
    /// Gets or sets the minimum allowed value for numeric properties.
    /// </summary>
    public double? RangeMinimum { get; set; }

    /// <summary>
    /// Gets or sets the maximum allowed value for numeric properties.
    /// </summary>
    public double? RangeMaximum { get; set; }

    /// <summary>
    /// Gets or sets the step increment for numeric properties.
    /// </summary>
    public double? RangeStep { get; set; }

    /// <summary>
    /// Gets or sets the editor hint for this property.
    /// </summary>
    public EditorHint EditorHint { get; set; }

    /// <summary>
    /// Gets a value indicating whether this property has a defined range.
    /// </summary>
    public bool HasRange => RangeMinimum.HasValue && RangeMaximum.HasValue;

    /// <inheritdoc />
    public override string ToString()
        => $"{PropertyName} ({DisplayName}) [{GroupName}]";
}