namespace Atc.XamlToolkit.XamlStyler.Model;

internal sealed class AttributeOrderRules
{
    private readonly IList<AttributeOrderRule> rules;
    private readonly Dictionary<string, AttributeOrderRule> ruleByAttributeName = new(StringComparer.Ordinal);

    public AttributeOrderRules(IXamlStylerOptions options)
    {
        rules = new List<AttributeOrderRule>();

        var groupIndex = 1;
        foreach (var group in options.AttributeOrderingRuleGroups)
        {
            if (!string.IsNullOrWhiteSpace(group))
            {
                var priority = 1;
                var names = group.Split(',')
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .Select(s => s.Trim())
                    .ToArray();

                foreach (var name in names)
                {
                    rules.Add(new AttributeOrderRule(name, groupIndex, priority));
                    priority++;
                }
            }

            groupIndex++;
        }

        rules.Add(new AttributeOrderRule("*", groupIndex, 0));
    }

    public AttributeOrderRule GetRuleFor(string attributeName)
    {
        // The rules never change after construction, so the result for a name can be remembered.
        if (!ruleByAttributeName.TryGetValue(attributeName, out var rule))
        {
            rule = rules
                .Where(r => r.Name.IsMatch(attributeName))
                .OrderByDescending(r => r.MatchScore)
                .First();
            ruleByAttributeName[attributeName] = rule;
        }

        return rule;
    }
}