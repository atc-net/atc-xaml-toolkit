namespace Atc.XamlToolkit.XamlStyler.Model;

internal sealed class AttributeInfo
{
    public AttributeOrderRule OrderRule { get; }

    public string Name { get; }

    public string Value { get; }

    public bool AttributeHasIgnoredNamespace { get; }

    public string AttributeNameWithoutNamespace { get; }

    public MarkupExtension? MarkupExtension { get; }

    public bool IsMarkupExtension => MarkupExtension is not null;

    public AttributeInfo(
        string name,
        string value,
        bool attributeHasIgnoredNamespace,
        string attributeNameWithoutNamespace,
        AttributeOrderRule orderRule,
        MarkupExtension? markupExtension)
    {
        Name = name;
        Value = value;
        AttributeHasIgnoredNamespace = attributeHasIgnoredNamespace;
        AttributeNameWithoutNamespace = attributeNameWithoutNamespace;
        OrderRule = orderRule;
        MarkupExtension = markupExtension;
    }
}