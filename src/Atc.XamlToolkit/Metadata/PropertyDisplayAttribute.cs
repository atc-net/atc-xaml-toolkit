// ReSharper disable RedundantAttributeUsageProperty
namespace Atc.XamlToolkit.Metadata;

/// <summary>
/// Specifies display metadata for a property, including its friendly name, group, sort order, and description.
/// Properties without this attribute are excluded from dynamic property editors (opt-in model).
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
[SuppressMessage("Design", "CA1019:Define accessors for attribute arguments", Justification = "GroupName and Order are intentionally settable as both positional and named parameters.")]
public sealed class PropertyDisplayAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyDisplayAttribute"/> class.
    /// </summary>
    public PropertyDisplayAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyDisplayAttribute"/> class.
    /// </summary>
    /// <param name="displayName">The friendly display name for the property.</param>
    public PropertyDisplayAttribute(string displayName)
    {
        DisplayName = displayName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyDisplayAttribute"/> class.
    /// </summary>
    /// <param name="displayName">The friendly display name for the property.</param>
    /// <param name="groupName">The group name for categorization.</param>
    public PropertyDisplayAttribute(
        string displayName,
        string groupName)
    {
        DisplayName = displayName;
        GroupName = groupName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyDisplayAttribute"/> class.
    /// </summary>
    /// <param name="displayName">The friendly display name for the property.</param>
    /// <param name="groupName">The group name for categorization.</param>
    /// <param name="order">The sort order within the group.</param>
    public PropertyDisplayAttribute(
        string displayName,
        string groupName,
        int order)
    {
        DisplayName = displayName;
        GroupName = groupName;
        Order = order;
    }

    /// <summary>
    /// Gets the friendly display name for the property.
    /// Falls back to the property name if <see langword="null"/>.
    /// </summary>
    public string? DisplayName { get; }

    /// <summary>
    /// Gets or sets the group name for categorization.
    /// Defaults to "General" when not specified.
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// Gets or sets the sort order within the group.
    /// Lower values appear first. Defaults to <see cref="int.MaxValue"/>.
    /// </summary>
    public int Order { get; set; } = int.MaxValue;

    /// <summary>
    /// Gets or sets the description text, typically shown as a tooltip.
    /// </summary>
    public string? Description { get; set; }

    /// <inheritdoc />
    public override string ToString()
        => $"{nameof(DisplayName)}: {DisplayName}, {nameof(GroupName)}: {GroupName}, {nameof(Order)}: {Order}, {nameof(Description)}: {Description}";
}