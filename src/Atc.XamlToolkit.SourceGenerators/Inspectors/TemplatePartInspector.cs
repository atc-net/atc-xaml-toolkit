namespace Atc.XamlToolkit.SourceGenerators.Inspectors;

/// <summary>
/// Finds the fields of a control that are marked with <c>[TemplatePartField]</c>.
/// </summary>
internal static class TemplatePartInspector
{
    private const string IsRequired = nameof(IsRequired);
    private const string Name = nameof(Name);

    public static List<TemplatePartToGenerate> Inspect(
        INamedTypeSymbol classSymbol)
    {
        var templateParts = new List<TemplatePartToGenerate>();

        foreach (var fieldSymbol in classSymbol.GetMembers().OfType<IFieldSymbol>())
        {
            if (fieldSymbol.IsStatic || fieldSymbol.IsConst)
            {
                continue;
            }

            var attribute = fieldSymbol
                .GetAttributes()
                .FirstOrDefault(x => x.GetToolkitAttributeName()
                    is NameConstants.TemplatePartFieldAttribute
                    or NameConstants.TemplatePartField);

            if (attribute is null ||
                !TryGetPartName(attribute, out var partNameExpression, out var partNameText))
            {
                continue;
            }

            var requiredNamespaces = new HashSet<string>(StringComparer.Ordinal);
            fieldSymbol.Type.AddRequiredNamespacesTo(requiredNamespaces);

            // The annotation belongs to the field, not to the type of the part.
            var typeName = fieldSymbol.Type
                .WithNullableAnnotation(NullableAnnotation.NotAnnotated)
                .ToGeneratedCodeTypeName()
                .TrimEnd('?');

            templateParts.Add(
                new TemplatePartToGenerate(
                    FieldName: fieldSymbol.Name.EscapeKeyword(),
                    PartNameExpression: partNameExpression,
                    PartNameText: partNameText,
                    TypeName: typeName,
                    IsRequired: GetIsRequired(attribute),
                    IsFieldNullable: fieldSymbol.NullableAnnotation == NullableAnnotation.Annotated,
                    RequiredNamespaces: new EquatableArray<string>(requiredNamespaces.OrderBy(x => x, StringComparer.Ordinal).ToArray())));
        }

        return templateParts;
    }

    /// <summary>
    /// Builds the platform's <c>[TemplatePart]</c> attribute for every part that the class
    /// does not already declare itself.
    /// </summary>
    /// <param name="xamlPlatform">The platform of the project.</param>
    /// <param name="classSymbol">The control class.</param>
    /// <param name="templateParts">The parts found by <see cref="Inspect"/>.</param>
    /// <returns>One attribute list per part.</returns>
    public static List<string> GetClassAttributes(
        XamlPlatform xamlPlatform,
        INamedTypeSymbol classSymbol,
        List<TemplatePartToGenerate> templateParts)
    {
        var classAttributes = new List<string>();
        if (templateParts.Count == 0)
        {
            return classAttributes;
        }

        var declaredPartNames = GetPartNamesDeclaredOnClass(classSymbol);

        foreach (var templatePart in templateParts)
        {
            if (templatePart.PartNameText is not null &&
                !declaredPartNames.Add(templatePart.PartNameText))
            {
                continue;
            }

            classAttributes.Add(xamlPlatform switch
            {
                XamlPlatform.Avalonia => $"[global::Avalonia.Controls.Metadata.TemplatePart({templatePart.PartNameExpression}, typeof({templatePart.TypeName}))]",
                XamlPlatform.WinUI => $"[global::Microsoft.UI.Xaml.TemplatePart(Name = {templatePart.PartNameExpression}, Type = typeof({templatePart.TypeName}))]",
                _ => $"[global::System.Windows.TemplatePart(Name = {templatePart.PartNameExpression}, Type = typeof({templatePart.TypeName}))]",
            });
        }

        return classAttributes;
    }

    // The part name is the attribute's only constructor argument. When the attribute class
    // cannot be resolved, only the syntax is available: a literal gives the name itself,
    // anything else (a constant) is passed on as written.
    private static bool TryGetPartName(
        AttributeData attribute,
        out string partNameExpression,
        out string? partNameText)
    {
        if (attribute.ConstructorArguments.Length > 0 &&
            attribute.ConstructorArguments[0].Value is string resolvedName)
        {
            partNameText = resolvedName;
            partNameExpression = SymbolDisplay.FormatLiteral(resolvedName, quote: true);
            return true;
        }

        var firstArgument = GetArguments(attribute).FirstOrDefault(x => x.NameEquals is null && x.NameColon is null);
        switch (firstArgument?.Expression)
        {
            case LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression):
                partNameText = literal.Token.ValueText;
                partNameExpression = SymbolDisplay.FormatLiteral(partNameText, quote: true);
                return true;
            case { } expression:
                partNameText = null;
                partNameExpression = expression.ToString();
                return true;
            default:
                partNameText = null;
                partNameExpression = string.Empty;
                return false;
        }
    }

    private static bool GetIsRequired(AttributeData attribute)
    {
        foreach (var namedArgument in attribute.NamedArguments)
        {
            if (namedArgument.Key == IsRequired)
            {
                return namedArgument.Value.Value is true;
            }
        }

        return GetArguments(attribute)
            .Any(x => x.NameEquals?.Name.Identifier.ValueText == IsRequired &&
                      x.Expression.IsKind(SyntaxKind.TrueLiteralExpression));
    }

    // The names of the parts the class declares with the platform's own [TemplatePart].
    private static HashSet<string> GetPartNamesDeclaredOnClass(
        INamedTypeSymbol classSymbol)
    {
        var partNames = new HashSet<string>(StringComparer.Ordinal);

        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name is not ("TemplatePartAttribute" or "TemplatePart"))
            {
                continue;
            }

            // WPF and WinUI: Name = "..."; Avalonia: the first constructor argument.
            foreach (var namedArgument in attribute.NamedArguments)
            {
                if (namedArgument.Key == Name && namedArgument.Value.Value is string namedValue)
                {
                    partNames.Add(namedValue);
                }
            }

            if (attribute.ConstructorArguments.Length > 0 &&
                attribute.ConstructorArguments[0].Value is string positionalValue)
            {
                partNames.Add(positionalValue);
            }

            foreach (var argument in GetArguments(attribute))
            {
                var isNameArgument = argument.NameEquals?.Name.Identifier.ValueText == Name ||
                                     (argument.NameEquals is null && argument.NameColon is null);

                if (isNameArgument &&
                    argument.Expression is LiteralExpressionSyntax literal &&
                    literal.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    partNames.Add(literal.Token.ValueText);
                }
            }
        }

        return partNames;
    }

    private static IEnumerable<AttributeArgumentSyntax> GetArguments(
        AttributeData attribute)
        => attribute.ApplicationSyntaxReference?.GetSyntax() is AttributeSyntax { ArgumentList: { } argumentList }
            ? argumentList.Arguments
            : [];
}