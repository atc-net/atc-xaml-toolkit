namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

internal sealed class SortableStringAttribute : ISortableAttribute
{
    private readonly string value;

    public SortableStringAttribute(string value)
    {
        this.value = value;
    }

    public static bool operator ==(SortableStringAttribute? left, SortableStringAttribute? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(SortableStringAttribute? left, SortableStringAttribute? right) =>
        !(left == right);

    public static bool operator <(SortableStringAttribute? left, SortableStringAttribute? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator >(SortableStringAttribute? left, SortableStringAttribute? right) =>
        left is not null && left.CompareTo(right) > 0;

    public static bool operator <=(SortableStringAttribute? left, SortableStringAttribute? right) =>
        left is null || left.CompareTo(right) <= 0;

    public static bool operator >=(SortableStringAttribute? left, SortableStringAttribute? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;

    public int CompareTo(object? obj)
    {
        if (obj is SortableStringAttribute other)
        {
            return string.Compare(value, other.value, StringComparison.Ordinal);
        }

        return 0;
    }

    public override bool Equals(object? obj) =>
        obj is SortableStringAttribute other
        && string.Equals(value, other.value, StringComparison.Ordinal);

    public override int GetHashCode() =>
        StringComparer.Ordinal.GetHashCode(value);
}