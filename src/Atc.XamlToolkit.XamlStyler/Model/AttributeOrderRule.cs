namespace Atc.XamlToolkit.XamlStyler.Model;

internal sealed class AttributeOrderRule
{
    public Wildcard Name { get; }

    public int Group { get; }

    public int Priority { get; }

    public int MatchScore { get; }

    public AttributeOrderRule(
        string name,
        int group,
        int priority)
    {
        Name = new Wildcard(name);
        Group = group;
        Priority = priority;

        MatchScore = (name.Equals("*", StringComparison.Ordinal) || name.Equals("*:*", StringComparison.Ordinal))
            ? -2
            : name.Any(c => c == '*')
                ? -1
                : name.Any(c => c == '?')
                    ? 0
                    : 1;
    }
}