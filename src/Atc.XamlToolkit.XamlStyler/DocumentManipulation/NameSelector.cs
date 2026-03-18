namespace Atc.XamlToolkit.XamlStyler.DocumentManipulation;

public class NameSelector
{
    private Wildcard? nameRegex;
    private Wildcard? namespaceRegex;

    private string? name;

    public string? Name
    {
        get => name;
        set
        {
            name = value;
            nameRegex = name is not null ? new Wildcard(name) : null;
        }
    }

    private string? namespaceName;

    public string? Namespace
    {
        get => namespaceName;
        set
        {
            namespaceName = value;
            namespaceRegex = namespaceName is not null ? new Wildcard(namespaceName) : null;
        }
    }

    public NameSelector()
    {
    }

    public NameSelector(string name)
    {
        Name = name;
    }

    public NameSelector(
        string? name,
        string? @namespace)
    {
        Name = name;
        Namespace = @namespace;
    }

    public bool IsMatch(XName xname)
    {
        if (xname is null)
        {
            throw new ArgumentNullException(nameof(xname));
        }

        if (nameRegex is not null && !nameRegex.IsMatch(xname.LocalName))
        {
            return false;
        }

        if (namespaceRegex is not null && !namespaceRegex.IsMatch(xname.Namespace.NamespaceName))
        {
            return false;
        }

        return true;
    }

    public bool IsMatch(string nameToMatch) =>
        nameRegex is null || nameRegex.IsMatch(nameToMatch);

    public override string ToString()
    {
        var prefix = Namespace is not null ? $"{Namespace}:" : string.Empty;
        return $"{prefix}{Name}";
    }
}