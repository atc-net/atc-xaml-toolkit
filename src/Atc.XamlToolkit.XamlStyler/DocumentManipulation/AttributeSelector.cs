namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

public sealed class AttributeSelector : NameSelector
{
    private Wildcard? valueRegex;
    private string? value;

    public string? Value
    {
        get => value;
        set
        {
            this.value = value;
            valueRegex = this.value is not null ? new Wildcard(this.value) : null;
        }
    }

    public AttributeSelector(
        string name,
        string? value)
        : base(name)
    {
        Value = value;
    }

    public AttributeSelector(
        string? name,
        string? @namespace,
        string? value)
        : base(name, @namespace)
    {
        Value = value;
    }

    public bool IsMatch(XAttribute attribute)
    {
        if (attribute is null)
        {
            throw new ArgumentNullException(nameof(attribute));
        }

        if (valueRegex is not null && !valueRegex.IsMatch(attribute.Value))
        {
            return false;
        }

        return IsMatch(attribute.Name);
    }

    public override string ToString()
    {
        var prefix = Namespace is not null ? $"{Namespace}:" : string.Empty;
        return $"{prefix}{Name}={Value}";
    }
}