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

    public void Append(string stringToAppend)
    {
        if (wasLastCallAppendLine)
        {
            sb.Append(indent);
            wasLastCallAppendLine = false;
        }

        sb.Append(stringToAppend);
    }

    public void GenerateEnd()
    {
        DecreaseIndent();
        sb.AppendLine("}");
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