namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

internal sealed class DocumentManipulationService
{
    private readonly IXamlStylerOptions options;
    private readonly List<IProcessElementService> processElementServices;

    public bool AllowProcessing => !options.SuppressProcessing;

    public DocumentManipulationService(IXamlStylerOptions options)
    {
        this.options = options;
        processElementServices = new List<IProcessElementService>
        {
            new VSMReorderService { Mode = options.ReorderVSM },
            new FormatThicknessService(
                options.ThicknessStyle,
                options.ThicknessAttributes),
            GetReorderGridChildrenService(),
            GetReorderCanvasChildrenService(),
            GetReorderSettersService(),
        };
    }

    public string ManipulateDocument(XDocument xDocument)
    {
        var xmlDeclaration = xDocument.Declaration?.ToString() ?? string.Empty;
        var rootElement = xDocument.Root;

        if (rootElement is not null)
        {
            if (options.RemoveDesignTimeReferences)
            {
                processElementServices.Add(GetRemoveDesignTimeReferencesService(rootElement));
            }

            HandleNode(rootElement);
        }

        return xmlDeclaration + xDocument;
    }

    private static AttributeRemovalService GetRemoveDesignTimeReferencesService(
        XElement element)
    {
        var removalService = new AttributeRemovalService();
        removalService.NamespaceDeclarations.Add(XNamespace.Get($"{{{XNamespace.Xmlns.NamespaceName}}}d"));
        removalService.NamespaceDeclarations.Add(XNamespace.Get($"{{{XNamespace.Xmlns.NamespaceName}}}mc"));
        removalService.Attributes.Add(new AttributeSelector("*", "d", null));
        removalService.Attributes.Add(new AttributeSelector("*", "mc", null));
        removalService.Initialize(element);
        return removalService;
    }

    private NodeReorderService GetReorderGridChildrenService()
    {
        var reorderService = new NodeReorderService { IsEnabled = options.ReorderGridChildren };
        reorderService.ParentNodeNames.Add(new NameSelector("Grid", null));
        reorderService.ChildNodeNames.Add(new NameSelector(null, null));
        reorderService.SortByAttributes.Add(new SortBy("Grid.Row", null, true));
        reorderService.SortByAttributes.Add(new SortBy("Grid.Column", null, true));
        return reorderService;
    }

    private NodeReorderService GetReorderCanvasChildrenService()
    {
        var reorderService = new NodeReorderService { IsEnabled = options.ReorderCanvasChildren };
        reorderService.ParentNodeNames.Add(new NameSelector("Canvas", null));
        reorderService.ChildNodeNames.Add(new NameSelector(null, null));
        reorderService.SortByAttributes.Add(new SortBy("Canvas.Left", null, true));
        reorderService.SortByAttributes.Add(new SortBy("Canvas.Top", null, true));
        reorderService.SortByAttributes.Add(new SortBy("Canvas.Right", null, true));
        reorderService.SortByAttributes.Add(new SortBy("Canvas.Bottom", null, true));
        return reorderService;
    }

    private NodeReorderService GetReorderSettersService()
    {
        var reorderService = new NodeReorderService();
        reorderService.ParentNodeNames.Add(new NameSelector("DataTrigger", null));
        reorderService.ParentNodeNames.Add(new NameSelector("MultiDataTrigger", null));
        reorderService.ParentNodeNames.Add(new NameSelector("MultiTrigger", null));
        reorderService.ParentNodeNames.Add(new NameSelector("Style", null));
        reorderService.ParentNodeNames.Add(new NameSelector("Trigger", null));
        reorderService.ChildNodeNames.Add(new NameSelector(
            "Setter",
            "http://schemas.microsoft.com/winfx/2006/xaml/presentation"));

        switch (options.ReorderSetters)
        {
            case ReorderSettersBy.None:
                reorderService.IsEnabled = false;
                break;
            case ReorderSettersBy.Property:
                reorderService.SortByAttributes.Add(new SortBy("Property", null, false));
                break;
            case ReorderSettersBy.TargetName:
                reorderService.SortByAttributes.Add(new SortBy("TargetName", null, false));
                break;
            case ReorderSettersBy.TargetNameThenProperty:
                reorderService.SortByAttributes.Add(new SortBy("TargetName", null, false));
                reorderService.SortByAttributes.Add(new SortBy("Property", null, false));
                break;
            default:
                throw new NotImplementedException($"Unhandled {nameof(ReorderSettersBy)}: {options.ReorderSetters}");
        }

        return reorderService;
    }

    private void HandleNode(XNode node)
    {
        if (node.NodeType != XmlNodeType.Element)
        {
            return;
        }

        if (node is not XElement element)
        {
            return;
        }

        if (element.Nodes().Any())
        {
            foreach (var childNode in element.Nodes().ToList())
            {
                HandleNode(childNode);
            }
        }

        foreach (var elementService in processElementServices)
        {
            elementService.ProcessElement(element);
        }
    }
}