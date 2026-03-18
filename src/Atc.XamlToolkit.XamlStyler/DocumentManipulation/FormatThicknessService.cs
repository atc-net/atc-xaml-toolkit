namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

internal sealed class FormatThicknessService : IProcessElementService
{
    private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XName SetterName = XName.Get("Setter", XamlNamespace);

    public FormatThicknessService(
        ThicknessStyle thicknessStyle,
        string thicknessAttributes)
    {
        IsEnabled = thicknessStyle != ThicknessStyle.None;
        ThicknessStyle = thicknessStyle;
        ThicknessAttributeNames = thicknessAttributes.ToNameSelectorList();
    }

    public bool IsEnabled { get; }

    public ThicknessStyle ThicknessStyle { get; }

    public IList<NameSelector> ThicknessAttributeNames { get; }

    public void ProcessElement(XElement element)
    {
        if (!IsEnabled || !element.HasAttributes)
        {
            return;
        }

        if (element.Name == SetterName)
        {
            var propertyAttribute = element.Attributes("Property").FirstOrDefault();
            if (propertyAttribute is not null
                && !propertyAttribute.Value.ContainsOrdinal(':')
                && ThicknessAttributeNames.Any(ns => ns.IsMatch(propertyAttribute.Value.Trim())))
            {
                var valueAttribute = element.Attributes("Value").FirstOrDefault();
                if (valueAttribute is not null)
                {
                    FormatAttribute(valueAttribute);
                }
            }
        }
        else
        {
            foreach (var attribute in element.Attributes()
                .Where(attribute => ThicknessAttributeNames.Any(ns => ns.IsMatch(attribute.Name))))
            {
                FormatAttribute(attribute);
            }
        }
    }

    private void FormatAttribute(XAttribute attribute)
    {
        var separator = ThicknessStyle == ThicknessStyle.Comma ? ',' : ' ';
        if (ThicknessFormatter.TryFormat(attribute.Value, separator, out var formatted))
        {
            attribute.Value = formatted;
        }
    }
}