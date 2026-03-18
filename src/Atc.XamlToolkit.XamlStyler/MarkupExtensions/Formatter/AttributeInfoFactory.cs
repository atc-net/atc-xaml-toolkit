namespace Atc.XamlToolkit.XamlStyler.MarkupExtensions.Formatter;

internal sealed class AttributeInfoFactory
{
    private readonly AttributeOrderRules orderRules;
    private readonly IList<string> ignoredNamespacesPrefixes;
    private readonly bool ignoreDesignTimeReferencePrefix;

    public AttributeInfoFactory(
        MarkupExtensionParser parser,
        AttributeOrderRules orderRules,
        IList<string> ignoredNamespacesPrefixes,
        bool ignoreDesignTimeReferencePrefix)
    {
        _ = parser; // Retained for API compatibility; TryParse is now static.
        this.orderRules = orderRules;
        this.ignoredNamespacesPrefixes = ignoredNamespacesPrefixes;
        this.ignoreDesignTimeReferencePrefix = ignoreDesignTimeReferencePrefix;
    }

    public AttributeInfo Create(XmlReader xmlReader)
    {
        var attributeName = xmlReader.Name;
        var attributeValue = xmlReader.Value;

        var attributeNameWithoutNamespace = string.Empty;
        var attributeHasIgnoredNamespace = ignoreDesignTimeReferencePrefix
            && CheckIfAttributeHasIgnoredNamespace(attributeName, out attributeNameWithoutNamespace);

        var orderRule = orderRules.GetRuleFor(
            attributeHasIgnoredNamespace ? attributeNameWithoutNamespace : attributeName);

        MarkupExtension? markupExtension = null;
        if (attributeValue.ContainsOrdinal('{'))
        {
            _ = MarkupExtensionParser.TryParse(attributeValue, out markupExtension);
        }

        return new AttributeInfo(
            attributeName,
            attributeValue,
            attributeHasIgnoredNamespace,
            attributeNameWithoutNamespace,
            orderRule,
            markupExtension);
    }

    private bool CheckIfAttributeHasIgnoredNamespace(
        string attributeName,
        out string attributeNameWithoutNamespace)
    {
        attributeNameWithoutNamespace = string.Empty;
#pragma warning disable CA1307 // IndexOf(char, StringComparison) not available on netstandard2.0
        var colonIndex = attributeName.IndexOf(':');
#pragma warning restore CA1307

        if (colonIndex > 0 && colonIndex < attributeName.Length - 1)
        {
            var namespacePrefix = attributeName.Substring(0, colonIndex);
            if (ignoredNamespacesPrefixes.Contains(namespacePrefix, StringComparer.Ordinal))
            {
                attributeNameWithoutNamespace = attributeName.Substring(colonIndex + 1);
                return true;
            }
        }

        return false;
    }
}