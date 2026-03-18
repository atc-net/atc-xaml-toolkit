namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Formatter;

internal sealed class MultiLineMarkupExtensionFormatter : MarkupExtensionFormatterBase
{
    internal MultiLineMarkupExtensionFormatter(
        MarkupExtensionFormatter markupExtensionFormatter)
        : base(markupExtensionFormatter)
    {
    }

    protected override IEnumerable<string> FormatArguments(
        IList<Argument> arguments,
        bool isNested = false)
    {
        var list = new List<string>();
        string? deferred = null;

        foreach (var argument in arguments)
        {
            if (deferred is not null)
            {
                deferred = string.Concat(deferred, ",");
            }

            foreach (var line in FormatArgument(argument, isNested))
            {
                if (deferred is not null)
                {
                    list.Add(deferred);
                }

                deferred = line;
            }
        }

        if (deferred is not null)
        {
            list.Add(deferred);
        }

        return list;
    }
}