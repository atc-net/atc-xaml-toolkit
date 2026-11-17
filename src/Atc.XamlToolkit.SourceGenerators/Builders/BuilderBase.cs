namespace Atc.XamlToolkit.SourceGenerators.Builders;

internal abstract class BuilderBase
{
    private const int IndentSpaces = 4;
    private const int MaxCachedIndentLevel = 16;

    private static readonly string[] IndentCache = CreateIndentCache();

    private readonly StringBuilder sb = new();
    private string indent = string.Empty;
    private bool wasLastCallAppendLine = true;
    private bool isFirstMember = true;
    private int uniqueVarCounter;
    private int openContainingTypes;

    public int IndentLevel { get; private set; }

    public XamlPlatform XamlPlatform { get; set; } = XamlPlatform.Wpf;

    public string GetUniqueVariableName(string prefix)
        => $"{prefix}{uniqueVarCounter++}";

    public void IncreaseIndent()
    {
        IndentLevel++;
        indent = IndentLevel < MaxCachedIndentLevel
            ? IndentCache[IndentLevel]
            : new string(' ', IndentLevel * IndentSpaces);
    }

    public bool DecreaseIndent()
    {
        if (IndentLevel == 0)
        {
            return false;
        }

        IndentLevel--;
        indent = IndentLevel < MaxCachedIndentLevel
            ? IndentCache[IndentLevel]
            : new string(' ', IndentLevel * IndentSpaces);
        return true;
    }

    private static string[] CreateIndentCache()
    {
        var cache = new string[MaxCachedIndentLevel];
        for (var i = 0; i < MaxCachedIndentLevel; i++)
        {
            cache[i] = new string(' ', i * IndentSpaces);
        }

        return cache;
    }

    public void AppendLineBeforeMember()
    {
        if (!isFirstMember)
        {
            sb.AppendLine();
        }

        isFirstMember = false;
    }

    public void AppendLine(string line)
    {
        if (wasLastCallAppendLine)
        {
            sb.Append(indent);
        }

        sb.AppendLine(line);
        wasLastCallAppendLine = true;
    }

    public void AppendLine()
    {
        sb.AppendLine();
        wasLastCallAppendLine = true;
    }

    // Appends the XML doc comments copied from the source member. When there are none and
    // generateDocumentation is set, appends a summary with the fallback text instead.
    public void AppendDocumentation(
        EquatableArray<string> documentationComments,
        bool generateDocumentation,
        string fallbackSummary)
    {
        if (!documentationComments.IsEmpty)
        {
            foreach (var documentationComment in documentationComments)
            {
                // Re-indent each line: the source member may be nested deeper than the
                // generated member, e.g. in a block-scoped namespace.
                foreach (var line in documentationComment.Split('\n'))
                {
                    AppendLine(line.Trim());
                }
            }
        }
        else if (generateDocumentation)
        {
            AppendSummary(fallbackSummary);
        }
    }

    public void AppendSummary(string summary)
    {
        AppendLine("/// <summary>");
        AppendLine($"/// {summary}");
        AppendLine("/// </summary>");
    }

    public void Append(string stringToAppend)
    {
        if (wasLastCallAppendLine)
        {
            sb.Append(indent);
            wasLastCallAppendLine = false;
        }

        sb.Append(stringToAppend);
    }

    // Appends the using directives the generated code needs, so it does not depend on the
    // global usings of the consumer project: System namespaces first, then the others.
    // Namespaces that enclose the generated class are in scope already.
    public void AppendUsings(
        IEnumerable<string> namespaces,
        string classNamespaceName)
    {
        var sortedNamespaces = namespaces
            .Where(x => x != classNamespaceName &&
                        !classNamespaceName.StartsWith(x + ".", StringComparison.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .OrderBy(x => x == "System" || x.StartsWith("System.", StringComparison.Ordinal) ? 0 : 1)
            .ThenBy(x => x, StringComparer.Ordinal);

        foreach (var sortedNamespace in sortedNamespaces)
        {
            AppendLine($"using {sortedNamespace};");
        }
    }

    // Appends the namespace, the containing types of a nested class and the class header.
    public void AppendTypeStart(GenerateBase toGenerate)
    {
        AppendLine();

        if (toGenerate.NamespaceName.Length > 0)
        {
            AppendLine($"namespace {toGenerate.NamespaceName};");
            AppendLine();
        }

        foreach (var containingTypeDeclaration in toGenerate.ContainingTypeDeclarations)
        {
            AppendLine(containingTypeDeclaration);
            AppendLine("{");
            IncreaseIndent();
            openContainingTypes++;
        }

        var staticModifier = toGenerate.IsStatic ? " static" : string.Empty;

        foreach (var classAttribute in toGenerate.ClassAttributes)
        {
            AppendLine(classAttribute);
        }

        AppendLine($"{toGenerate.ClassAccessModifier}{staticModifier} partial class {toGenerate.ClassName}{toGenerate.TypeParameterList}");
        AppendLine("{");
        IncreaseIndent();
    }

    public void GenerateEnd()
    {
        DecreaseIndent();
        AppendLine("}");

        while (openContainingTypes > 0)
        {
            DecreaseIndent();
            AppendLine("}");
            openContainingTypes--;
        }

        sb.AppendLine();
        sb.AppendLine("#nullable disable");
    }

    public override string ToString()
        => sb.ToString();

    public SourceText ToSourceText()
    {
        var str = sb.ToString();
        str = str.Substring(0, str.Length - 2);
        return SourceText.From(
            str,
            Encoding.UTF8);
    }
}