namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Formatter;

internal sealed class SingleLineMarkupExtensionFormatter : MarkupExtensionFormatterBase
{
    internal SingleLineMarkupExtensionFormatter(
        MarkupExtensionFormatter markupExtensionFormatter)
        : base(markupExtensionFormatter)
    {
    }

    protected override IEnumerable<string> FormatArguments(
        IList<Argument> arguments,
        bool isNested = false)
    {
        var sb = new StringBuilder();
        foreach (var argument in arguments)
        {
            if (sb.Length > 0)
            {
                sb.Append(", ");
            }

            foreach (var line in FormatArgument(argument, isNested: true))
            {
                sb.Append(line);
            }
        }

        yield return sb.ToString();
    }
}