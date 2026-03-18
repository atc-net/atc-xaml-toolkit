namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

internal sealed class AttributeRemovalService : IProcessElementService
{
    private List<AttributeSelector>? initializedAttributes;

    public List<NameSelector> NodeNames { get; } = new();

    public List<XNamespace> NamespaceDeclarations { get; } = new();

    public List<AttributeSelector> Attributes { get; } = new();

    public bool IsEnabled { get; set; } = true;

    public void Initialize(XElement element)
    {
        initializedAttributes = Attributes
            .Select(a => new AttributeSelector(
                a.Name,
                element.GetNamespaceOfPrefix(a.Namespace ?? string.Empty)?.NamespaceName ?? a.Namespace,
                a.Value))
            .ToList();
    }

    public void ProcessElement(XElement element)
    {
        if (initializedAttributes is null)
        {
            throw new InvalidOperationException("AttributeRemovalService not initialized.");
        }

        if (!IsEnabled || !element.HasAttributes)
        {
            return;
        }

        if (NodeNames.Count != 0 && !NodeNames.Any(ns => ns.IsMatch(element.Name)))
        {
            return;
        }

        var elementAttributeList = element.Attributes().ToList();
        var removedAttributeList = new List<XAttribute>();

        foreach (var elementAttribute in elementAttributeList)
        {
            if (elementAttribute.IsNamespaceDeclaration)
            {
                if (NamespaceDeclarations.Any(ns => ns.NamespaceName == elementAttribute.Name.ToString()))
                {
                    removedAttributeList.Add(elementAttribute);
                }
            }
            else
            {
                foreach (var attribute in initializedAttributes)
                {
                    if (attribute.IsMatch(elementAttribute)
                        && (string.IsNullOrEmpty(attribute.Value)
                            || string.Equals(attribute.Value, elementAttribute.Value, StringComparison.Ordinal)))
                    {
                        removedAttributeList.Add(elementAttribute);
                        break;
                    }
                }
            }
        }

        foreach (var removedAttribute in removedAttributeList)
        {
            elementAttributeList.Remove(removedAttribute);
        }

        element.ReplaceAttributes(elementAttributeList);
    }
}