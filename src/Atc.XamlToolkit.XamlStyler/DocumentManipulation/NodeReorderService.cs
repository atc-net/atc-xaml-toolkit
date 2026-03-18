#pragma warning disable MA0051 // Method is too long
namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

internal sealed class NodeReorderService : IProcessElementService
{
    private readonly List<NameSelector> ignoredNodeNames = new()
    {
        new NameSelector("VisualStateManager.VisualStateGroups", null),
    };

    public bool IsEnabled { get; set; } = true;

    public List<NameSelector> ParentNodeNames { get; } = new();

    public List<NameSelector> ChildNodeNames { get; } = new();

    public List<SortBy> SortByAttributes { get; } = new();

    public void ProcessElement(XElement element)
    {
        if (!IsEnabled || !element.HasElements)
        {
            return;
        }

        if (ParentNodeNames.Any(ns => ns.IsMatch(element.Name)))
        {
            ReorderChildNodes(element);
        }
    }

    private void ReorderChildNodes(XElement element)
    {
        var nodeCollections = new List<NodeCollection>();
        var children = element.Nodes();

        var inMatchingChildBlock = false;
        var childBlockIndex = 0;

        NodeCollection? currentNodeCollection = null;

        foreach (var child in children)
        {
            if (currentNodeCollection is null)
            {
                currentNodeCollection = new NodeCollection();
                nodeCollections.Add(currentNodeCollection);
            }

            if (child.NodeType == XmlNodeType.Element)
            {
                var childElement = (XElement)child;

                var isMatchingChild = ChildNodeNames.Any(ns => ns.IsMatch(childElement.Name))
                    && !ignoredNodeNames.Any(ns => ns.IsMatch(childElement.Name));

                if (!isMatchingChild || !inMatchingChildBlock)
                {
                    childBlockIndex++;
                    inMatchingChildBlock = isMatchingChild;
                }

                if (isMatchingChild)
                {
                    currentNodeCollection.SetSortAttributeValues(
                        new Collection<ISortableAttribute>(
                            SortByAttributes.Select(s => s.GetValue(childElement)).ToArray()));
                }

                currentNodeCollection.BlockIndex = childBlockIndex;
            }

            currentNodeCollection.Nodes.Add(child);

            if (child.NodeType == XmlNodeType.Element)
            {
                currentNodeCollection = null;
            }
        }

        if (currentNodeCollection is not null)
        {
            currentNodeCollection.BlockIndex = childBlockIndex + 1;
        }

        nodeCollections = nodeCollections.OrderBy(nc => nc).ToList();
        element.ReplaceNodes(nodeCollections.SelectMany(nc => nc.Nodes));
    }
}