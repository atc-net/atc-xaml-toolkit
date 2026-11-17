#pragma warning disable MA0051 // Method is too long
namespace Atc.XamlToolkit.XamlStyler.DocumentProcessors;

internal sealed class ElementDocumentProcessor : IDocumentProcessor
{
    private readonly IXamlStylerOptions options;
    private readonly XamlLanguageOptions xamlLanguageOptions;
    private readonly AttributeInfoFactory attributeInfoFactory;
    private readonly AttributeInfoFormatter attributeInfoFormatter;
    private readonly IndentService indentService;
    private readonly XmlEscapingService xmlEscapingService;
    private readonly IList<string> noNewLineElementsList;
    private readonly IList<string> firstLineAttributes;
    private readonly string[] inlineCollections = ["TextBlock", "RichTextBlock", "Paragraph", "Run", "Span", "InlineUIContainer", "AnchoredBlock"];
    private readonly string[] inlineTypes = ["Paragraph", "Run", "Span", "InlineUIContainer", "AnchoredBlock", "Hyperlink", "Bold", "Italic", "Underline", "LineBreak"];

    public ElementDocumentProcessor(
        IXamlStylerOptions options,
        XamlLanguageOptions xamlLanguageOptions,
        AttributeInfoFactory attributeInfoFactory,
        AttributeInfoFormatter attributeInfoFormatter,
        IndentService indentService,
        XmlEscapingService xmlEscapingService)
    {
        this.xmlEscapingService = xmlEscapingService;
        this.options = options;
        this.xamlLanguageOptions = xamlLanguageOptions;
        this.attributeInfoFactory = attributeInfoFactory;
        this.attributeInfoFormatter = attributeInfoFormatter;
        this.indentService = indentService;
        noNewLineElementsList = options.NoNewLineElements.ToList();
        firstLineAttributes = options.FirstLineAttributes.ToList();
    }

    public void Process(
        XmlReader xmlReader,
        StringBuilder output,
        ElementProcessContext elementProcessContext)
    {
        elementProcessContext.UpdateParentElementProcessStatus(ContentTypes.Mixed);

        var elementName = xmlReader.Name;
        elementProcessContext.Push(new ElementProcessStatus
        {
            Parent = elementProcessContext.Current,
            Name = elementName,
            ContentType = ContentTypes.None,
            IsMultlineStartTag = false,
            IsPreservingSpace = elementProcessContext.Current.IsPreservingSpace,
        });

        var currentIndentString = indentService.GetIndentString(xmlReader.Depth);
        var attributeIndentationString = GetAttributeIndentationString(xmlReader);

        if (!elementProcessContext.Current.IsPreservingSpace)
        {
            var parent = elementProcessContext.Current.Parent;
            var parentIsInlineCollection =
                parent?.Name is not null
                && inlineCollections.Any(c => IsNameOfType(parent.Name, c));

            if (parent is not null
                && parentIsInlineCollection
                && inlineTypes.Any(t => IsNameOfType(elementName, t)))
            {
                parent.IsSignificantWhiteSpace = true;
                if (output.Length == 0 || output.IsNewLine())
                {
                    output.Append(currentIndentString);
                }
                else if (parent.TextTrailingWhitespaceIndex == output.Length)
                {
                    // The preceding text ended with whitespace (e.g. "Hello <Run />"); keep one space.
                    output.Append(' ');
                }
            }
            else
            {
                if (elementProcessContext.Current.Parent is not null)
                {
                    // The content of an inline collection is text, so a space after any of its
                    // children is shown and has to stay.
                    elementProcessContext.Current.Parent.IsSignificantWhiteSpace = parentIsInlineCollection;
                }

                if (output.Length == 0 || output.IsNewLine())
                {
                    output.Append(currentIndentString);
                }
                else
                {
                    output.Append(Environment.NewLine).Append(currentIndentString);
                }
            }
        }

        output.Append('<').Append(elementName);

        var isEmptyElement = xmlReader.IsEmptyElement;

        if (xmlReader.HasAttributes)
        {
            var isNoLineBreakElement = noNewLineElementsList.Contains(elementName, StringComparer.Ordinal);
            ProcessAttributes(
                xmlReader,
                output,
                elementProcessContext,
                isNoLineBreakElement,
                attributeIndentationString);
        }

        var putEndingBracketOnNewLine = options.PutEndingBracketOnNewLine
            && elementProcessContext.Current.IsMultlineStartTag;

        if (putEndingBracketOnNewLine)
        {
            output.Append(Environment.NewLine).Append(attributeIndentationString);
        }

        if (isEmptyElement)
        {
            if (!putEndingBracketOnNewLine && options.SpaceBeforeClosingSlash)
            {
                output.Append(' ');
            }

            output.Append("/>");
            elementProcessContext.Pop();
        }
        else
        {
            output.Append('>');
            elementProcessContext.Current.StartTagEndIndex = output.Length - 1;
        }
    }

    private void ProcessAttributes(
        XmlReader xmlReader,
        StringBuilder output,
        ElementProcessContext elementProcessContext,
        bool isNoLineBreakElement,
        string attributeIndentationString)
    {
        var list = new List<AttributeInfo>(xmlReader.AttributeCount);
        var firstLineList = new List<AttributeInfo>(xmlReader.AttributeCount);

        while (xmlReader.MoveToNextAttribute())
        {
            var attributeInfo = attributeInfoFactory.Create(xmlReader);
            list.Add(attributeInfo);

            if (options.EnableAttributeReordering
                && firstLineAttributes.Contains(attributeInfo.Name, StringComparer.Ordinal))
            {
                firstLineList.Add(attributeInfo);
            }

            if (string.Equals(xmlReader.LocalName, "space", StringComparison.Ordinal)
                && string.Equals(xmlReader.Prefix, "xml", StringComparison.Ordinal))
            {
                elementProcessContext.Current.IsPreservingSpace = xmlReader.Value == "preserve";
            }
        }

        if (options.EnableAttributeReordering)
        {
            list.InsertionSort(AttributeInfoComparison);
            firstLineList.InsertionSort(AttributeInfoComparison);
        }

        var noLineBreakInAttributes = list.Count <= options.AttributesTolerance || isNoLineBreakElement;
        var forceLineBreakInAttributes = false;

        if (elementProcessContext.Count == 2)
        {
            switch (options.RootElementLineBreakRule)
            {
                case LineBreakRule.Default:
                    break;
                case LineBreakRule.Always:
                    noLineBreakInAttributes = false;
                    forceLineBreakInAttributes = true;
                    break;
                case LineBreakRule.Never:
                    noLineBreakInAttributes = true;
                    break;
                default:
                    throw new NotImplementedException($"Unhandled {nameof(LineBreakRule)}: {options.RootElementLineBreakRule}");
            }
        }

        if (noLineBreakInAttributes)
        {
            foreach (var attrInfo in list)
            {
                output.Append(' ').Append(attributeInfoFormatter.ToSingleLineString(attrInfo, xamlLanguageOptions));
            }

            elementProcessContext.Current.IsMultlineStartTag = false;
        }
        else
        {
            ProcessMultiLineAttributes(
                output,
                elementProcessContext,
                attributeIndentationString,
                list,
                firstLineList,
                forceLineBreakInAttributes);
        }
    }

    private void ProcessMultiLineAttributes(
        StringBuilder output,
        ElementProcessContext elementProcessContext,
        string attributeIndentationString,
        List<AttributeInfo> list,
        List<AttributeInfo> firstLineList,
        bool forceLineBreakInAttributes)
    {
        var attributeLines = new List<string>();
        var currentLineBuffer = new StringBuilder();
        var attributeCountInCurrentLineBuffer = 0;
        var xmlnsAliasesBypassLengthInCurrentLine = 0;

        AttributeInfo? lastAttributeInfo = null;

        var firstLine = string.Empty;
        foreach (var attrInfo in firstLineList)
        {
            firstLine = $"{firstLine} {attributeInfoFormatter.ToSingleLineString(attrInfo, xamlLanguageOptions)}";
        }

        if (firstLine.Length > 0)
        {
            attributeLines.Add(firstLine);
        }

        foreach (var attrInfo in list)
        {
            if (firstLineList.Contains(attrInfo))
            {
                continue;
            }

            if (attrInfo.IsMarkupExtension && options.FormatMarkupExtension)
            {
                if (currentLineBuffer.Length > 0)
                {
                    attributeLines.Add(currentLineBuffer.ToString());
                    currentLineBuffer.Length = 0;
                    attributeCountInCurrentLineBuffer = 0;
                }

                attributeLines.Add(attributeInfoFormatter.ToMultiLineString(attrInfo, attributeIndentationString));
            }
            else
            {
                var pendingAppend = attributeInfoFormatter.ToSingleLineString(attrInfo, xamlLanguageOptions);

                // xmlns declarations carry a temporary "[prefix]" marker that is removed from the final
                // output, so it must not count towards the line length.
                var pendingBypassLength = pendingAppend.Length - xmlEscapingService.RestoreXmlnsAliasesBypass(pendingAppend).Length;

                var isAttributeCharLengthExceeded = attributeCountInCurrentLineBuffer > 0
                    && options.MaxAttributeCharactersPerLine > 0
                    && (currentLineBuffer.Length + pendingAppend.Length - xmlnsAliasesBypassLengthInCurrentLine - pendingBypassLength) > options.MaxAttributeCharactersPerLine;

                var isAttributeCountExceeded = options.MaxAttributesPerLine > 0
                    && (attributeCountInCurrentLineBuffer + 1) > options.MaxAttributesPerLine;

                var isAttributeRuleGroupChanged = options.PutAttributeOrderRuleGroupsOnSeparateLines
                    && lastAttributeInfo is not null
                    && lastAttributeInfo.OrderRule.Group != attrInfo.OrderRule.Group;

                if (currentLineBuffer.Length > 0
                    && (forceLineBreakInAttributes || isAttributeCharLengthExceeded || isAttributeCountExceeded || isAttributeRuleGroupChanged))
                {
                    attributeLines.Add(currentLineBuffer.ToString());
                    currentLineBuffer.Length = 0;
                    attributeCountInCurrentLineBuffer = 0;
                    xmlnsAliasesBypassLengthInCurrentLine = 0;
                }

                currentLineBuffer.AppendFormat(CultureInfo.InvariantCulture, "{0} ", pendingAppend);
                attributeCountInCurrentLineBuffer++;
                xmlnsAliasesBypassLengthInCurrentLine += pendingBypassLength;
            }

            lastAttributeInfo = attrInfo;
        }

        if (currentLineBuffer.Length > 0)
        {
            attributeLines.Add(currentLineBuffer.ToString());
        }

        for (var i = 0; i < attributeLines.Count; i++)
        {
            if (i == 0 && (options.KeepFirstAttributeOnSameLine || firstLineList.Count > 0))
            {
                output.Append(' ').Append(attributeLines[i].Trim());
            }
            else
            {
                output.Append(Environment.NewLine)
                    .Append(indentService.Normalize(attributeIndentationString + attributeLines[i].Trim()));
            }
        }

        elementProcessContext.Current.IsMultlineStartTag = true;
    }

    private int AttributeInfoComparison(
        AttributeInfo x,
        AttributeInfo y)
    {
        if (x.OrderRule.Group != y.OrderRule.Group)
        {
            return x.OrderRule.Group.CompareTo(y.OrderRule.Group);
        }

        if (x.OrderRule.Priority != y.OrderRule.Priority)
        {
            return x.OrderRule.Priority.CompareTo(y.OrderRule.Priority);
        }

        if (!options.OrderAttributesByName)
        {
            return 0;
        }

        if (x.AttributeHasIgnoredNamespace && y.AttributeHasIgnoredNamespace)
        {
            return string.Compare(x.AttributeNameWithoutNamespace, y.AttributeNameWithoutNamespace, StringComparison.Ordinal);
        }

        if (x.AttributeHasIgnoredNamespace && !string.Equals(x.AttributeNameWithoutNamespace, y.Name, StringComparison.Ordinal))
        {
            return string.Compare(x.AttributeNameWithoutNamespace, y.Name, StringComparison.Ordinal);
        }

        if (y.AttributeHasIgnoredNamespace && !string.Equals(y.AttributeNameWithoutNamespace, x.Name, StringComparison.Ordinal))
        {
            return string.Compare(x.Name, y.AttributeNameWithoutNamespace, StringComparison.Ordinal);
        }

        return string.Compare(x.Name, y.Name, StringComparison.Ordinal);
    }

    private string GetAttributeIndentationString(XmlReader xmlReader)
    {
        if (options.AttributeIndentation == 0)
        {
            return options.KeepFirstAttributeOnSameLine
                ? indentService.GetIndentString(xmlReader.Depth, xmlReader.Name.Length + 2)
                : indentService.GetIndentString(xmlReader.Depth + 1);
        }

        return indentService.GetIndentString(xmlReader.Depth, options.AttributeIndentation);
    }

    // True for the type itself ("Run", "ui:Run"), for a type named after it as derived types
    // are ("SelectableTextBlock"), and for their property elements ("TextBlock.Inlines") -
    // not for other types whose name merely contains it ("RunButton", "TextBlockHost").
    private static bool IsNameOfType(
        string elementName,
        string typeName)
    {
        var prefixEnd = elementName.IndexOf(":", StringComparison.Ordinal);
        var localName = prefixEnd >= 0
            ? elementName.Substring(prefixEnd + 1)
            : elementName;

        var propertyStart = localName.IndexOf(".", StringComparison.Ordinal);
        var ownerName = propertyStart >= 0
            ? localName.Substring(0, propertyStart)
            : localName;

        return ownerName.EndsWith(typeName, StringComparison.Ordinal);
    }
}