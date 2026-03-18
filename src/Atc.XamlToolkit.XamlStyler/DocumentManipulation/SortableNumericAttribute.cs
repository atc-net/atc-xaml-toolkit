namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

internal sealed class SortableNumericAttribute : ISortableAttribute
{
    private readonly double value;

    public SortableNumericAttribute(
        string? value,
        double defaultValue)
    {
        this.value = value is not null && double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var parsed)
            ? parsed
            : defaultValue;
    }

    public static bool operator ==(SortableNumericAttribute? left, SortableNumericAttribute? right) =>
        left is null ? right is null : left.Equals(right);

    public static bool operator !=(SortableNumericAttribute? left, SortableNumericAttribute? right) =>
        !(left == right);

    public static bool operator <(SortableNumericAttribute? left, SortableNumericAttribute? right) =>
        left is null ? right is not null : left.CompareTo(right) < 0;

    public static bool operator >(SortableNumericAttribute? left, SortableNumericAttribute? right) =>
        left is not null && left.CompareTo(right) > 0;

    public static bool operator <=(SortableNumericAttribute? left, SortableNumericAttribute? right) =>
        left is null || left.CompareTo(right) <= 0;

    public static bool operator >=(SortableNumericAttribute? left, SortableNumericAttribute? right) =>
        left is null ? right is null : left.CompareTo(right) >= 0;

    public int CompareTo(object? obj)
    {
        if (obj is SortableNumericAttribute other)
        {
            return value.CompareTo(other.value);
        }

        return 0;
    }

    public override bool Equals(object? obj) =>
        obj is SortableNumericAttribute other && value.Equals(other.value);

    public override int GetHashCode() => value.GetHashCode();
}