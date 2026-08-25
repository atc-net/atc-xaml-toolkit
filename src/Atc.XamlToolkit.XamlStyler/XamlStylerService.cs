namespace Atc.XamlToolkit.XamlStyler;

/// <summary>
/// Main XAML styling service. Formats XAML documents according to the provided options.
/// </summary>
public sealed class XamlStylerService
{
    [SuppressMessage("Minor Code Smell", "S5332:Using http protocol is insecure. Use https instead", Justification = "OK — these are XML namespace identifiers, not network endpoints, and must match the literal values used in XAML documents.")]
    private readonly string[] ignoredNamespacesInOrdering =
    [
        "http://schemas.microsoft.com/expression/blend/2008",
        "http://xamarin.com/schemas/2014/forms/design",
    ];

    private readonly DocumentManipulationService documentManipulationService;
    private readonly IXamlStylerOptions options;
    private readonly XamlLanguageOptions xamlLanguageOptions;
    private readonly XmlEscapingService xmlEscapingService;
    private Dictionary<XmlNodeType, IDocumentProcessor>? documentProcessors;

    /// <summary>
    /// Initializes a new instance of the <see cref="XamlStylerService"/> class.
    /// </summary>
    /// <param name="options">The formatting options.</param>
    /// <param name="xamlLanguageOptions">Optional XAML language options.</param>
    public XamlStylerService(
        IXamlStylerOptions options,
        XamlLanguageOptions? xamlLanguageOptions = null)
    {
        if (options is null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        xmlEscapingService = new XmlEscapingService();
        documentManipulationService = new DocumentManipulationService(options);
        this.options = options;
        this.xamlLanguageOptions = xamlLanguageOptions ?? new XamlLanguageOptions();
    }

    /// <summary>
    /// Styles a XAML document string and returns the formatted result.
    /// </summary>
    /// <param name="xamlSource">The XAML source string to format.</param>
    /// <returns>The formatted XAML string.</returns>
    public string StyleDocument(string xamlSource)
    {
        if (!documentManipulationService.AllowProcessing)
        {
            return xamlSource;
        }

        var escapedDocument = xmlEscapingService.EscapeDocument(xamlSource);
        var xDocument = XDocument.Parse(escapedDocument, LoadOptions.PreserveWhitespace);
        var manipulatedDocument = documentManipulationService.ManipulateDocument(xDocument);

        var ignoredNamespacesPrefixes = FindIgnoredNamespaces(manipulatedDocument);
        ApplyOptions(ignoredNamespacesPrefixes, options.IgnoreDesignTimeReferencePrefix);

        var formatted = Format(manipulatedDocument);
        var result = xmlEscapingService.UnescapeDocument(formatted);

        result = TrimLeadingWhitespace(result);

        return ApplyLineEnding(result);
    }

    /// <summary>
    /// Styles a XAML file in-place. Returns true if the file was modified.
    /// </summary>
    /// <param name="filePath">The path to the XAML file.</param>
    /// <param name="forceUtf8Bom">When true, always writes files with UTF-8 BOM regardless of original encoding.</param>
    /// <returns>True if the file was modified; otherwise, false.</returns>
    public bool StyleFile(
        string filePath,
        bool forceUtf8Bom = false)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("XAML file not found.", filePath);
        }

        string originalContent;
        Encoding detectedEncoding;

        using (var stream = File.OpenRead(filePath))
        using (var reader = new StreamReader(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), detectEncodingFromByteOrderMarks: true))
        {
            originalContent = reader.ReadToEnd();
            detectedEncoding = reader.CurrentEncoding;
        }

        var formattedContent = StyleDocument(originalContent);

        if (string.Equals(originalContent, formattedContent, StringComparison.Ordinal))
        {
            return false;
        }

        var writeEncoding = forceUtf8Bom
            ? new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)
            : detectedEncoding;

        using (var stream = File.Create(filePath))
        using (var writer = new StreamWriter(stream, writeEncoding))
        {
            writer.Write(formattedContent);
        }

        return true;
    }

    private void ApplyOptions(
        IList<string> ignoredNamespacesPrefixes,
        bool ignoreDesignTimeReferencePrefix)
    {
        var indentService = new IndentService(options);
        var markupExtensionFormatter = new MarkupExtensionFormatter(options.NoNewLineMarkupExtensions.ToList());
        var attributeInfoFactory = new AttributeInfoFactory(
            new MarkupExtensionParser(),
            new AttributeOrderRules(options),
            ignoredNamespacesPrefixes,
            ignoreDesignTimeReferencePrefix);
        var attributeInfoFormatter = new AttributeInfoFormatter(
            markupExtensionFormatter,
            indentService);

        documentProcessors = new Dictionary<XmlNodeType, IDocumentProcessor>
        {
            { XmlNodeType.Element, new ElementDocumentProcessor(options, xamlLanguageOptions, attributeInfoFactory, attributeInfoFormatter, indentService) },
            { XmlNodeType.Text, new TextDocumentProcessor(indentService) },
            { XmlNodeType.CDATA, new CdataDocumentProcessor(indentService) },
            { XmlNodeType.ProcessingInstruction, new ProcessInstructionDocumentProcessor(indentService) },
            { XmlNodeType.Comment, new CommentDocumentProcessor(options, indentService) },
            { XmlNodeType.Whitespace, new WhitespaceDocumentProcessor() },
            { XmlNodeType.SignificantWhitespace, new SignificantWhitespaceDocumentProcessor() },
            { XmlNodeType.EndElement, new EndElementDocumentProcessor(options, indentService) },
            { XmlNodeType.XmlDeclaration, new XmlDeclarationDocumentProcessor() },
        };
    }

    private IList<string> FindIgnoredNamespaces(string xamlSource)
    {
        using var sourceReader = new StringReader(xamlSource);
        using var xmlReader = XmlReader.Create(sourceReader);

        while (!xmlReader.Read() || xmlReader.NodeType != XmlNodeType.Element)
        {
            if (xmlReader.EOF)
            {
                return Array.Empty<string>();
            }
        }

        if (xmlReader.EOF)
        {
            return Array.Empty<string>();
        }

        if (!xmlReader.MoveToFirstAttribute())
        {
            return Array.Empty<string>();
        }

        var ignoredNamespacesPrefixes = new List<string>();
        while (xmlReader.MoveToNextAttribute())
        {
            var prefix = xmlReader.LocalName;
            var namespaceUri = xmlReader.Value.ReplaceOrdinal($"[{prefix}]", string.Empty);

            if (ignoredNamespacesInOrdering.Contains(namespaceUri, StringComparer.Ordinal))
            {
                ignoredNamespacesPrefixes.Add(prefix);
            }
        }

        return ignoredNamespacesPrefixes;
    }

    private string Format(string xamlSource)
    {
        if (documentProcessors is null)
        {
            throw new InvalidOperationException("ApplyOptions must be called before Format.");
        }

        var output = new StringBuilder();

        using var sourceReader = new StringReader(xamlSource);
        using var xmlReader = XmlReader.Create(sourceReader);
        var elementProcessContext = new ElementProcessContext();

        while (xmlReader.Read())
        {
            if (documentProcessors.TryGetValue(xmlReader.NodeType, out var processor))
            {
                processor.Process(xmlReader, output, elementProcessContext);
            }
            else
            {
                Trace.TraceInformation("Unprocessed NodeType: {0} Name: {1} Value: {2}", xmlReader.NodeType, xmlReader.Name, xmlReader.Value);
            }
        }

        return output.ToString();
    }

    private static string TrimLeadingWhitespace(string content)
    {
        var index = content.IndexOf("<", StringComparison.Ordinal);
        if (index > 0)
        {
            return content[index..];
        }

        return content;
    }

    private string ApplyLineEnding(string content) =>
        options.LineEnding switch
        {
            LineEnding.LF => content.ReplaceOrdinal("\r\n", "\n"),
            LineEnding.CRLF => content.ReplaceOrdinal("\r\n", "\n").ReplaceOrdinal("\n", "\r\n"),
            _ => content,
        };
}