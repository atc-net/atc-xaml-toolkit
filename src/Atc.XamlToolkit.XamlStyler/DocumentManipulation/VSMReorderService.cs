#pragma warning disable MA0051 // Method is too long
namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

[SuppressMessage("Naming", "S101:Types should be named in PascalCase", Justification = "VSM is a well-known abbreviation for VisualStateManager.")]
internal sealed class VSMReorderService : IProcessElementService
{
    private readonly NameSelector vsmNode = new("VisualStateManager.VisualStateGroups", null);

    public VisualStateManagerRule Mode { get; set; } = VisualStateManagerRule.None;

    public void ProcessElement(XElement element)
    {
        if (Mode == VisualStateManagerRule.None || !element.HasElements)
        {
            return;
        }

        if (vsmNode.IsMatch(element.Name) && element.Parent is not null)
        {
            ReorderChildNodes(element.Parent);
        }
    }

    private void ReorderChildNodes(XElement element)
    {
        var nodeCollections = new List<NodeCollection>();
        var propertyElementCollection = new List<NodeCollection>();
        var vsmNodeCollection = new NodeCollection();

        var parentName = element.Name;
        var children = element.Nodes().ToList();

        NodeCollection? currentNodeCollection = null;
        var collectionAdded = false;

        if (Mode == VisualStateManagerRule.Last && children.Count > 0)
        {
            children.Remove(children[children.Count - 1]);
        }

        foreach (var child in children)
        {
            currentNodeCollection ??= new NodeCollection();
            currentNodeCollection.Nodes.Add(child);

            if (child.NodeType == XmlNodeType.Element)
            {
                var childName = ((XElement)child).Name;

                collectionAdded = false;
                if (vsmNode.IsMatch(childName))
                {
                    vsmNodeCollection = currentNodeCollection;
                    collectionAdded = true;
                }
                else if (childName.LocalName.StartsWith($"{parentName.LocalName}.", StringComparison.Ordinal))
                {
                    propertyElementCollection.Add(currentNodeCollection);
                    collectionAdded = true;
                }

                if (!collectionAdded)
                {
                    nodeCollections.Add(currentNodeCollection);
                }

                currentNodeCollection = null;
            }
        }

        if (currentNodeCollection is not null)
        {
            nodeCollections.Add(currentNodeCollection);
        }

        var newNodes = (Mode == VisualStateManagerRule.Last
            ? propertyElementCollection.SelectMany(nc => nc.Nodes)
                .Concat(nodeCollections.SelectMany(nc => nc.Nodes))
                .Concat(vsmNodeCollection.Nodes)
            : propertyElementCollection.SelectMany(nc => nc.Nodes)
                .Concat(vsmNodeCollection.Nodes)
                .Concat(nodeCollections.SelectMany(nc => nc.Nodes))).ToList();

        if (Mode == VisualStateManagerRule.Last
            && newNodes.Count > 0
            && newNodes[0] is XText firstNode
            && string.IsNullOrWhiteSpace(firstNode.Value.Trim()))
        {
            newNodes.Remove(firstNode);
        }

        element.ReplaceNodes(newNodes);
    }
}