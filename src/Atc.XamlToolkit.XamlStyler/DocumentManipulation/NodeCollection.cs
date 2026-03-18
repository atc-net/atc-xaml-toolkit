namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

internal sealed class NodeCollection : IComparable<NodeCollection>, IComparable, IEquatable<NodeCollection>
{
    public List<XNode> Nodes { get; } = new();

    public int BlockIndex { get; set; }

    private Collection<ISortableAttribute>? sortAttributeValues;

    public void SetSortAttributeValues(Collection<ISortableAttribute> values)
    {
        sortAttributeValues = values;
    }

    public static bool operator ==(
        NodeCollection? left,
        NodeCollection? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(
        NodeCollection? left,
        NodeCollection? right) =>
        !(left == right);

    public static bool operator <(
        NodeCollection? left,
        NodeCollection? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator >(
        NodeCollection? left,
        NodeCollection? right) =>
        left is not null && left.CompareTo(right) > 0;

    public static bool operator <=(
        NodeCollection? left,
        NodeCollection? right) =>
        left is null || left.CompareTo(right) <= 0;

    public static bool operator >=(
        NodeCollection? left,
        NodeCollection? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;

    public int CompareTo(NodeCollection? other)
    {
        if (other is null)
        {
            return 1;
        }

        var blockResult = BlockIndex.CompareTo(other.BlockIndex);
        if (blockResult != 0)
        {
            return blockResult;
        }

        if (sortAttributeValues is null || other.sortAttributeValues is null)
        {
            return 0;
        }

        var minCount = Math.Min(sortAttributeValues.Count, other.sortAttributeValues.Count);
        for (var i = 0; i < minCount; i++)
        {
            var result = sortAttributeValues[i].CompareTo(other.sortAttributeValues[i]);
            if (result != 0)
            {
                return result;
            }
        }

        return 0;
    }

    public int CompareTo(object? obj) =>
        CompareTo(obj as NodeCollection);

    public bool Equals(NodeCollection? other) =>
        other is not null && CompareTo(other) == 0;

    public override bool Equals(object? obj) =>
        Equals(obj as NodeCollection);

    public override int GetHashCode() =>
        BlockIndex.GetHashCode();
}