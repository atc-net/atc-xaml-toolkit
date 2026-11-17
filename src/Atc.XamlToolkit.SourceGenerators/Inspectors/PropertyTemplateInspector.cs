namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

/// <summary>
/// Finds the members a class takes over from the property templates it uses with
/// <c>[UseDependencyPropertyTemplate]</c>.
/// </summary>
internal static class PropertyTemplateInspector
{
    public static bool IsTemplate(INamedTypeSymbol classSymbol)
        => classSymbol.GetAttributes().Any(x => x.GetToolkitAttributeName()
            is NameConstants.DependencyPropertyTemplateAttribute
            or NameConstants.DependencyPropertyTemplate);

    /// <summary>
    /// Gets the attributed fields of every template the class uses. For a generic template
    /// they are the fields of the constructed type, so their types have the type arguments
    /// filled in.
    /// </summary>
    /// <param name="classSymbol">The class that uses the templates.</param>
    /// <param name="unusableTemplates">Receives the name of each template that cannot be applied.</param>
    /// <returns>The fields, in the order the templates are listed and the fields are declared.</returns>
    public static ImmutableArray<ISymbol> GetTemplateMembers(
        INamedTypeSymbol classSymbol,
        List<string> unusableTemplates)
        => GetTemplateMembers(classSymbol, unusableTemplates, documentationValues: null);

    /// <summary>
    /// Gets the attributed fields of every template the class uses, and the documentation
    /// values stated for them.
    /// </summary>
    /// <param name="classSymbol">The class that uses the templates.</param>
    /// <param name="unusableTemplates">Receives the name of each template that cannot be applied.</param>
    /// <param name="documentationValues">Receives, per generated member name, the values for the placeholders in its documentation.</param>
    /// <returns>The fields, in the order the templates are listed and the fields are declared.</returns>
    public static ImmutableArray<ISymbol> GetTemplateMembers(
        INamedTypeSymbol classSymbol,
        List<string> unusableTemplates,
        Dictionary<string, string[]>? documentationValues)
    {
        var result = ImmutableArray.CreateBuilder<ISymbol>();

        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (attribute.GetToolkitAttributeName()
                    is not (NameConstants.UseDependencyPropertyTemplateAttribute
                    or NameConstants.UseDependencyPropertyTemplate) ||
                attribute.ConstructorArguments.Length == 0 ||
                attribute.ConstructorArguments[0].Value is not INamedTypeSymbol templateType)
            {
                continue;
            }

            var members = templateType
                .GetMembers()
                .Where(x => x is IFieldSymbol { IsImplicitlyDeclared: false } && x.GetAttributes().Length > 0)
                .ToList();

            // A template in another assembly shows no fields: private members are not
            // part of what the compiler lets a referencing project see.
            if (!IsTemplate(templateType.OriginalDefinition) || members.Count == 0)
            {
                unusableTemplates.Add(templateType.Name);
                continue;
            }

            var excludedNames = GetExcludedNames(attribute);
            var appliedMembers = members.Where(x => !excludedNames.Contains(GetGeneratedName(x))).ToList();
            result.AddRange(appliedMembers);

            var values = GetStringArray(attribute, NameConstants.DocumentationValues);
            if (documentationValues is not null && values.Length > 0)
            {
                foreach (var member in appliedMembers)
                {
                    documentationValues[GetGeneratedName(member)] = values;
                }
            }
        }

        return result.ToImmutable();
    }

    private static HashSet<string> GetExcludedNames(AttributeData attribute)
        => new(
            GetStringArray(attribute, NameConstants.Exclude).Select(GetGeneratedName),
            StringComparer.Ordinal);

    private static string[] GetStringArray(
        AttributeData attribute,
        string argumentName)
    {
        var result = new List<string>();

        foreach (var namedArgument in attribute.NamedArguments)
        {
            if (namedArgument.Key != argumentName ||
                namedArgument.Value.Kind != TypedConstantKind.Array ||
                namedArgument.Value.IsNull)
            {
                continue;
            }

            // A null element keeps its place, so the values after it keep their index
            foreach (var value in namedArgument.Value.Values)
            {
                result.Add(value.Value as string ?? string.Empty);
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// Replaces the placeholders <c>{0}</c>, <c>{1}</c>, ... in documentation comments.
    /// Other braces, such as the type parameters in a <c>cref</c>, are left alone.
    /// </summary>
    /// <param name="documentationComments">The documentation comment lines.</param>
    /// <param name="values">The values, by placeholder index.</param>
    /// <returns>The lines with the placeholders filled in.</returns>
    public static EquatableArray<string> FillPlaceholders(
        EquatableArray<string> documentationComments,
        string[] values)
    {
        var result = new string[documentationComments.Count];
        var index = 0;

        foreach (var comment in documentationComments)
        {
            var line = comment;
            for (var i = 0; i < values.Length; i++)
            {
                // The value is text in an XML comment
                var text = values[i]
                    .Replace("&", "&amp;")
                    .Replace("<", "&lt;")
                    .Replace(">", "&gt;");

                line = line.Replace("{" + i.ToString(CultureInfo.InvariantCulture) + "}", text);
            }

            result[index++] = line;
        }

        return new EquatableArray<string>(result);
    }

    // The name of the member generated from a template field: the PropertyName its
    // attribute states, otherwise the name derived from the field.
    private static string GetGeneratedName(ISymbol member)
    {
        foreach (var attribute in member.GetAttributes())
        {
            if (attribute.ExtractStringArgument(NameConstants.PropertyName) is { } propertyName &&
                SyntaxFacts.IsValidIdentifier(propertyName) &&
                SyntaxFacts.GetKeywordKind(propertyName) == SyntaxKind.None)
            {
                return propertyName;
            }
        }

        return GetGeneratedName(member.Name);
    }

    // The name of the property or event generated from a field: no leading underscore,
    // first letter in upper case.
    private static string GetGeneratedName(string fieldName)
        => fieldName.TrimStart('_').EnsureFirstCharacterToUpper();
}