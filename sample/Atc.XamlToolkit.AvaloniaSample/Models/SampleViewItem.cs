namespace Atc.XamlToolkit.AvaloniaSample.Models;

/// <summary>
/// Represents an item in the sample view tree structure.
/// </summary>
public class SampleViewItem
{
    public SampleViewItem(string name, string? relativePath = null, Type? viewType = null)
    {
        Name = name;
        RelativePath = relativePath;
        ViewType = viewType;
        Children = new List<SampleViewItem>();
    }

    /// <summary>
    /// Gets the display name of the item.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets the relative path within the project (for folders).
    /// </summary>
    public string? RelativePath { get; }

    /// <summary>
    /// Gets the Type of the view (for leaf nodes).
    /// </summary>
    public Type? ViewType { get; }

    /// <summary>
    /// Gets the child items.
    /// </summary>
    public IList<SampleViewItem> Children { get; }

    /// <summary>
    /// Gets a value indicating whether this is a folder (has children or no view type).
    /// </summary>
    public bool IsFolder => ViewType is null;

    public override string ToString() => Name;
}