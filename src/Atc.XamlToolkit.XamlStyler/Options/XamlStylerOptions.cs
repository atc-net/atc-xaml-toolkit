namespace Atc.XamlToolkit.XamlStyler.Options;

/// <summary>
/// Default implementation of <see cref="IXamlStylerOptions"/> with JSON configuration support.
/// </summary>
public sealed class XamlStylerOptions : IXamlStylerOptions
{
    private const string DefaultOptionsPath = "Atc.XamlToolkit.XamlStyler.Options.DefaultSettings.json";
    private const int FallbackIndentSize = 4;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    [ThreadStatic]
    private static bool isDeserializing;

    public XamlStylerOptions()
    {
        if (!isDeserializing)
        {
            InitializeFromDefaults();
        }
    }

    public XamlStylerOptions(string? configPath)
    {
        if (!string.IsNullOrWhiteSpace(configPath)
            && File.Exists(configPath)
            && TryLoadConfiguration(File.ReadAllText(configPath)))
        {
            return;
        }

        InitializeFromDefaults();
    }

    [JsonPropertyName("IndentSize")]
    public int IndentSize { get; set; } = FallbackIndentSize;

    [JsonPropertyName("IndentWithTabs")]
    public bool? IndentWithTabs { get; set; }

    [JsonPropertyName("AttributesTolerance")]
    public int AttributesTolerance { get; set; } = 2;

    [JsonPropertyName("KeepFirstAttributeOnSameLine")]
    public bool KeepFirstAttributeOnSameLine { get; set; }

    [JsonPropertyName("FirstLineAttributes")]
    public string FirstLineAttributes { get; set; } = string.Empty;

    [JsonPropertyName("MaxAttributeCharactersPerLine")]
    public int MaxAttributeCharactersPerLine { get; set; }

    [JsonPropertyName("MaxAttributesPerLine")]
    public int MaxAttributesPerLine { get; set; } = 1;

    [JsonPropertyName("NewlineExemptionElements")]
    public string NoNewLineElements { get; set; } = "RadialGradientBrush, GradientStop, LinearGradientBrush, ScaleTransform, SkewTransform, RotateTransform, TranslateTransform, Trigger, Condition, Setter";

    [JsonPropertyName("SeparateByGroups")]
    public bool PutAttributeOrderRuleGroupsOnSeparateLines { get; set; }

    [JsonPropertyName("AttributeIndentation")]
    public int AttributeIndentation { get; set; }

    [JsonPropertyName("RemoveDesignTimeReferences")]
    public bool RemoveDesignTimeReferences { get; set; }

    [JsonPropertyName("AttributeIndentationStyle")]
    public AttributeIndentationStyle AttributeIndentationStyle { get; set; } = AttributeIndentationStyle.Spaces;

    [JsonPropertyName("IgnoreDesignTimeReferencePrefix")]
    public bool IgnoreDesignTimeReferencePrefix { get; set; }

    [JsonPropertyName("EnableAttributeReordering")]
    public bool EnableAttributeReordering { get; set; } = true;

    [SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "Required for serialization/deserialization")]
    [JsonPropertyName("AttributeOrderingRuleGroups")]
    public string[] AttributeOrderingRuleGroups { get; set; } =
    [
        "x:Class",
        "xmlns, xmlns:x",
        "xmlns:*",
        "x:Key, Key, x:Name, Name, x:Uid, Uid, Title",
        "Grid.Row, Grid.RowSpan, Grid.Column, Grid.ColumnSpan, Canvas.Left, Canvas.Top, Canvas.Right, Canvas.Bottom",
        "Width, Height, MinWidth, MinHeight, MaxWidth, MaxHeight",
        "Margin, Padding, HorizontalAlignment, VerticalAlignment, HorizontalContentAlignment, VerticalContentAlignment, Panel.ZIndex",
        "*:*, *",
        "PageSource, PageIndex, Offset, Color, TargetName, Property, Value, StartPoint, EndPoint",
        "mc:Ignorable, d:IsDataSource, d:LayoutOverrides, d:IsStaticText",
        "Storyboard.*, From, To, Duration",
    ];

    [JsonPropertyName("OrderAttributesByName")]
    public bool OrderAttributesByName { get; set; } = true;

    [JsonPropertyName("PutEndingBracketOnNewLine")]
    public bool PutEndingBracketOnNewLine { get; set; }

    [JsonPropertyName("RemoveEndingTagOfEmptyElement")]
    public bool RemoveEndingTagOfEmptyElement { get; set; } = true;

    [JsonPropertyName("SpaceBeforeClosingSlash")]
    public bool SpaceBeforeClosingSlash { get; set; } = true;

    [JsonPropertyName("RootElementLineBreakRule")]
    public LineBreakRule RootElementLineBreakRule { get; set; } = LineBreakRule.Default;

    [JsonPropertyName("ReorderVSM")]
    public VisualStateManagerRule ReorderVSM { get; set; } = VisualStateManagerRule.Last;

    [JsonPropertyName("ReorderGridChildren")]
    public bool ReorderGridChildren { get; set; }

    [JsonPropertyName("ReorderCanvasChildren")]
    public bool ReorderCanvasChildren { get; set; }

    [JsonPropertyName("ReorderSetters")]
    public ReorderSettersBy ReorderSetters { get; set; } = ReorderSettersBy.None;

    [JsonPropertyName("FormatMarkupExtension")]
    public bool FormatMarkupExtension { get; set; } = true;

    [JsonPropertyName("NoNewLineMarkupExtensions")]
    public string NoNewLineMarkupExtensions { get; set; } = "x:Bind, Binding";

    [JsonPropertyName("ThicknessSeparator")]
    public ThicknessStyle ThicknessStyle { get; set; } = ThicknessStyle.Comma;

    [JsonPropertyName("ThicknessAttributes")]
    public string ThicknessAttributes { get; set; } = "Margin, Padding, BorderThickness, ThumbnailClipMargin";

    [JsonPropertyName("CommentPadding")]
    public int CommentSpaces { get; set; } = 2;

    [JsonIgnore]
    public LineEnding LineEnding { get; set; } = LineEnding.Auto;

    [JsonPropertyName("SuppressProcessing")]
    public bool SuppressProcessing { get; set; }

    /// <summary>
    /// Creates a deep clone of this options instance.
    /// </summary>
    /// <returns>A new <see cref="XamlStylerOptions"/> instance with the same settings.</returns>
    public XamlStylerOptions Clone()
    {
        var json = JsonSerializer.Serialize(this, JsonOptions);
        return JsonSerializer.Deserialize<XamlStylerOptions>(json, JsonOptions) ?? new XamlStylerOptions();
    }

    /// <summary>
    /// Loads options from a JSON configuration file path.
    /// Returns a new options instance with the loaded settings, falling back to defaults on failure.
    /// </summary>
    /// <param name="configPath">The path to the JSON configuration file.</param>
    /// <returns>A new <see cref="XamlStylerOptions"/> instance.</returns>
    public static XamlStylerOptions FromConfigFile(string configPath) =>
        new XamlStylerOptions(configPath);

    /// <summary>
    /// Finds a Settings.XamlStyler file by walking up the directory hierarchy from the given path.
    /// Returns null if no configuration file is found.
    /// </summary>
    /// <param name="startPath">The starting file or directory path.</param>
    /// <returns>The path to the configuration file, or null if not found.</returns>
    public static string? FindConfigFile(string startPath)
    {
        var directory = File.Exists(startPath)
            ? Path.GetDirectoryName(startPath)
            : startPath;

        while (!string.IsNullOrEmpty(directory))
        {
            var configPath = Path.Combine(directory, "Settings.XamlStyler");
            if (File.Exists(configPath))
            {
                return configPath;
            }

            directory = Path.GetDirectoryName(directory);
        }

        return null;
    }

    private void InitializeFromDefaults()
    {
        var assembly = Assembly.GetExecutingAssembly();
        using var stream = assembly.GetManifestResourceStream(DefaultOptionsPath);
        if (stream is null)
        {
            return;
        }

        using var reader = new StreamReader(stream);
        var json = reader.ReadToEnd();
        TryLoadConfiguration(json);
    }

    [SuppressMessage("Critical Code Smell", "S2696:Instance members should not write to static fields", Justification = "Thread-static recursion guard for JSON deserialization.")]
    private bool TryLoadConfiguration(string json)
    {
        try
        {
            isDeserializing = true;
            XamlStylerOptions? loaded;
            try
            {
                loaded = JsonSerializer.Deserialize<XamlStylerOptions>(json, JsonOptions);
            }
            finally
            {
                isDeserializing = false;
            }

            if (loaded is null)
            {
                return false;
            }

            IndentSize = loaded.IndentSize > 0 ? loaded.IndentSize : FallbackIndentSize;
            IndentWithTabs = loaded.IndentWithTabs;
            AttributesTolerance = loaded.AttributesTolerance;
            KeepFirstAttributeOnSameLine = loaded.KeepFirstAttributeOnSameLine;
            FirstLineAttributes = loaded.FirstLineAttributes;
            MaxAttributeCharactersPerLine = loaded.MaxAttributeCharactersPerLine;
            MaxAttributesPerLine = loaded.MaxAttributesPerLine;
            NoNewLineElements = loaded.NoNewLineElements;
            PutAttributeOrderRuleGroupsOnSeparateLines = loaded.PutAttributeOrderRuleGroupsOnSeparateLines;
            AttributeIndentation = loaded.AttributeIndentation;
            RemoveDesignTimeReferences = loaded.RemoveDesignTimeReferences;
            AttributeIndentationStyle = loaded.AttributeIndentationStyle;
            IgnoreDesignTimeReferencePrefix = loaded.IgnoreDesignTimeReferencePrefix;
            EnableAttributeReordering = loaded.EnableAttributeReordering;
            AttributeOrderingRuleGroups = loaded.AttributeOrderingRuleGroups;
            OrderAttributesByName = loaded.OrderAttributesByName;
            PutEndingBracketOnNewLine = loaded.PutEndingBracketOnNewLine;
            RemoveEndingTagOfEmptyElement = loaded.RemoveEndingTagOfEmptyElement;
            SpaceBeforeClosingSlash = loaded.SpaceBeforeClosingSlash;
            RootElementLineBreakRule = loaded.RootElementLineBreakRule;
            ReorderVSM = loaded.ReorderVSM;
            ReorderGridChildren = loaded.ReorderGridChildren;
            ReorderCanvasChildren = loaded.ReorderCanvasChildren;
            ReorderSetters = loaded.ReorderSetters;
            FormatMarkupExtension = loaded.FormatMarkupExtension;
            NoNewLineMarkupExtensions = loaded.NoNewLineMarkupExtensions;
            ThicknessStyle = loaded.ThicknessStyle;
            ThicknessAttributes = loaded.ThicknessAttributes;
            CommentSpaces = loaded.CommentSpaces;
            SuppressProcessing = loaded.SuppressProcessing;

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }
}