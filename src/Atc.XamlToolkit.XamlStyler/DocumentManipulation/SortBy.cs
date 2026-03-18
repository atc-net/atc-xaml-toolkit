namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

internal sealed class SortBy : NameSelector
{
    private readonly Func<XElement, string> defaultValue;

    public SortBy(
        string name,
        string? @namespace,
        bool isNumeric)
        : base(name, @namespace)
    {
        IsNumeric = isNumeric;
        defaultValue = isNumeric
            ? element => element.Name.LocalName.ContainsOrdinal('.') ? "-32768" : "-32767"
            : _ => string.Empty;
    }

    public SortBy(
        string name,
        bool isNumeric)
        : base(name)
    {
        IsNumeric = isNumeric;
        defaultValue = isNumeric
            ? element => element.Name.LocalName.ContainsOrdinal('.') ? "-32768" : "-32767"
            : _ => string.Empty;
    }

    public bool IsNumeric { get; }

    public ISortableAttribute GetValue(XElement element)
    {
        var attribute = element.Attributes().FirstOrDefault(a => IsMatch(a.Name));
        var value = attribute?.Value;

        return IsNumeric
            ? new SortableNumericAttribute(
                value,
                double.Parse(defaultValue(element), CultureInfo.InvariantCulture))
            : new SortableStringAttribute(value ?? defaultValue(element));
    }
}