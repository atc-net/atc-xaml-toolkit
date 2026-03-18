namespace Atc.XamlToolkit.XamlStyler.Options;

/// <summary>
/// Options that control how the XAML styler formats documents.
/// </summary>
public interface IXamlStylerOptions
{
    /// <summary>Gets or sets the indent size.</summary>
    int IndentSize { get; set; }

    /// <summary>Gets or sets a value indicating whether to indent with tabs.</summary>
    bool? IndentWithTabs { get; set; }

    /// <summary>Gets or sets the number of attributes tolerated on a single line.</summary>
    int AttributesTolerance { get; set; }

    /// <summary>Gets or sets a value indicating whether to keep the first attribute on the same line.</summary>
    bool KeepFirstAttributeOnSameLine { get; set; }

    /// <summary>Gets or sets the first line attributes.</summary>
    string FirstLineAttributes { get; set; }

    /// <summary>Gets or sets the maximum attribute characters per line.</summary>
    int MaxAttributeCharactersPerLine { get; set; }

    /// <summary>Gets or sets the maximum attributes per line.</summary>
    int MaxAttributesPerLine { get; set; }

    /// <summary>Gets or sets the elements that should not have line breaks.</summary>
    string NoNewLineElements { get; set; }

    /// <summary>Gets or sets a value indicating whether to put attribute order rule groups on separate lines.</summary>
    bool PutAttributeOrderRuleGroupsOnSeparateLines { get; set; }

    /// <summary>Gets or sets the attribute indentation.</summary>
    int AttributeIndentation { get; set; }

    /// <summary>Gets or sets a value indicating whether to remove design time references.</summary>
    bool RemoveDesignTimeReferences { get; set; }

    /// <summary>Gets or sets the attribute indentation style.</summary>
    AttributeIndentationStyle AttributeIndentationStyle { get; set; }

    /// <summary>Gets or sets a value indicating whether to ignore design time reference prefix.</summary>
    bool IgnoreDesignTimeReferencePrefix { get; set; }

    /// <summary>Gets or sets a value indicating whether to enable attribute reordering.</summary>
    bool EnableAttributeReordering { get; set; }

    /// <summary>Gets or sets the attribute ordering rule groups.</summary>
    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Required for serialization/deserialization")]
    string[] AttributeOrderingRuleGroups { get; set; }

    /// <summary>Gets or sets a value indicating whether to order attributes by name.</summary>
    bool OrderAttributesByName { get; set; }

    /// <summary>Gets or sets a value indicating whether to put the ending bracket on a new line.</summary>
    bool PutEndingBracketOnNewLine { get; set; }

    /// <summary>Gets or sets a value indicating whether to remove the ending tag of empty elements.</summary>
    bool RemoveEndingTagOfEmptyElement { get; set; }

    /// <summary>Gets or sets a value indicating whether to add a space before the closing slash.</summary>
    bool SpaceBeforeClosingSlash { get; set; }

    /// <summary>Gets or sets the root element line break rule.</summary>
    LineBreakRule RootElementLineBreakRule { get; set; }

    /// <summary>Gets or sets the visual state manager reorder rule.</summary>
    VisualStateManagerRule ReorderVSM { get; set; }

    /// <summary>Gets or sets a value indicating whether to reorder grid children.</summary>
    bool ReorderGridChildren { get; set; }

    /// <summary>Gets or sets a value indicating whether to reorder canvas children.</summary>
    bool ReorderCanvasChildren { get; set; }

    /// <summary>Gets or sets the reorder setters mode.</summary>
    ReorderSettersBy ReorderSetters { get; set; }

    /// <summary>Gets or sets a value indicating whether to format markup extensions.</summary>
    bool FormatMarkupExtension { get; set; }

    /// <summary>Gets or sets the markup extensions that should not have line breaks.</summary>
    string NoNewLineMarkupExtensions { get; set; }

    /// <summary>Gets or sets the thickness style.</summary>
    ThicknessStyle ThicknessStyle { get; set; }

    /// <summary>Gets or sets the thickness attributes.</summary>
    string ThicknessAttributes { get; set; }

    /// <summary>Gets or sets the number of spaces in comments.</summary>
    int CommentSpaces { get; set; }

    /// <summary>Gets or sets the line ending style.</summary>
    LineEnding LineEnding { get; set; }

    /// <summary>Gets or sets a value indicating whether to suppress processing.</summary>
    bool SuppressProcessing { get; set; }
}