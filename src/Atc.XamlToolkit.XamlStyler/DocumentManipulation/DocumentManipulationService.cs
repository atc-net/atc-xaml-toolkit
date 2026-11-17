namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

internal sealed class DocumentManipulationService
{
    private readonly IXamlStylerOptions options;
    private List<IProcessElementService>? processElementServices;
    private (VisualStateManagerRule ReorderVsm, ThicknessStyle ThicknessStyle, string ThicknessAttributes, bool ReorderGridChildren, bool ReorderCanvasChildren, ReorderSettersBy ReorderSetters) processElementServicesOptions;

    public bool AllowProcessing => !options.SuppressProcessing;

    public DocumentManipulationService(IXamlStylerOptions options)
    {
        this.options = options;
    }

    // The services are kept between documents (creating them compiles regular expressions),
    // but rebuilt when an option they depend on has changed - so a change to the options
    // after the service was created takes effect, as it does for the formatting options.
    private List<IProcessElementService> GetProcessElementServices()
    {
        var currentOptions = (
            options.ReorderVSM,
            options.ThicknessStyle,
            options.ThicknessAttributes,
            options.ReorderGridChildren,
            options.ReorderCanvasChildren,
            options.ReorderSetters);

        if (processElementServices is null ||
            !currentOptions.Equals(processElementServicesOptions))
        {
            processElementServicesOptions = currentOptions;
            processElementServices =
            [
                new VSMReorderService { Mode = options.ReorderVSM },
                new FormatThicknessService(
                    options.ThicknessStyle,
                    options.ThicknessAttributes),
                GetReorderGridChildrenService(),
                GetReorderCanvasChildrenService(),
                GetReorderSettersService(),
            ];
        }

        return processElementServices;
    }

    public string ManipulateDocument(XDocument xDocument)
    {
        var xmlDeclaration = xDocument.Declaration?.ToString() ?? string.Empty;
        var rootElement = xDocument.Root;

        if (rootElement is not null)
        {
            // The removal service is bound to this document's namespace prefixes, so it is created
            // per call and never added to the shared list.
            var sharedServices = GetProcessElementServices();
            IReadOnlyList<IProcessElementService> services = options.RemoveDesignTimeReferences
                ? [.. sharedServices, GetRemoveDesignTimeReferencesService(rootElement)]
                : sharedServices;

            HandleNode(rootElement, services);
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
        foreach (var setterNamespace in FormatThicknessService.SetterNamespaces)
        {
            reorderService.ChildNodeNames.Add(new NameSelector("Setter", setterNamespace));
        }

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

    private static void HandleNode(
        XNode node,
        IReadOnlyList<IProcessElementService> services)
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
                HandleNode(childNode, services);
            }
        }

        foreach (var elementService in services)
        {
            elementService.ProcessElement(element);
        }
    }
}