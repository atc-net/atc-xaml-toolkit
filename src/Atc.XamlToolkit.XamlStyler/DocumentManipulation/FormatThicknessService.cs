namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

internal sealed class FormatThicknessService : IProcessElementService
{
    /// <summary>
    /// The default XML namespaces of the XAML dialects whose Setter element is recognized:
    /// WPF / WinUI / UWP, Avalonia, .NET MAUI and Xamarin.Forms.
    /// </summary>
    [SuppressMessage("Security", "S5332:Using http protocol is insecure", Justification = "OK - XML namespace identifiers, not URLs.")]
    internal static readonly string[] SetterNamespaces =
    [
        "http://schemas.microsoft.com/winfx/2006/xaml/presentation",
        "https://github.com/avaloniaui",
        "http://schemas.microsoft.com/dotnet/2021/maui",
        "http://xamarin.com/schemas/2014/forms",
    ];

    private static readonly HashSet<XName> SetterNames = new(SetterNamespaces.Select(ns => XName.Get("Setter", ns)));

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

        if (SetterNames.Contains(element.Name))
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