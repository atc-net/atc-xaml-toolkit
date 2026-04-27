namespace Atc.XamlToolkit.SourceGenerators.Generators.Helpers;

/// <summary>
/// Predicate and transform helpers for <see cref="ObservableDtoViewModelGenerator"/>.
/// Lives apart from the generator class so the generator only carries the main flow
/// (Initialize / Execute).
/// </summary>
internal static class ObservableDtoViewModelGeneratorHelper
{
    /// <summary>
    /// Determines if a given syntax node is a valid target for code generation (predicate phase).
    /// </summary>
    /// <param name="syntaxNode">The syntax node to check.</param>
    /// <returns>True if the node is a valid target; otherwise, false.</returns>
    /// <remarks>
    /// Performs early filtering to optimize performance by checking:
    /// <list type="bullet">
    /// <item><description>The node is a partial class declaration</description></item>
    /// <item><description>The class has attributes (looking for ObservableDtoViewModel attribute)</description></item>
    /// <item><description>The class has a base list (inherits from a base class)</description></item>
    /// <item><description>The class specifically has the ObservableDtoViewModel attribute (syntax-only check)</description></item>
    /// </list>
    /// This avoids expensive semantic model operations on classes that won't generate code.
    /// </remarks>
    public static bool IsSyntaxTarget(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        // Must be partial (required for source generation)
        if (!classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            return false;
        }

        // Must have a base list (inherits from a base class)
        if (classDeclaration.BaseList is null)
        {
            return false;
        }

        // Must have the ObservableDtoViewModel attribute on this declaration.
        // For multi-file partial classes, only the declaration with the attribute
        // needs to pass the predicate; the semantic phase will discover all partials.
        return classDeclaration.AttributeLists.Count > 0 &&
               HasObservableDtoViewModelAttribute(classDeclaration.AttributeLists);
    }

    /// <summary>
    /// Extracts the semantic target for code generation (transform phase).
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>An <see cref="ObservableDtoViewModelToGenerate"/> object if valid; otherwise, null.</returns>
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    public static ObservableDtoViewModelToGenerate? GetSemanticTarget(
        GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

        if (classSymbol is null)
        {
            return null;
        }

        // Skip if an earlier partial declaration already qualifies via IsSyntaxTarget,
        // so only the first qualifying declaration triggers the full semantic analysis.
        foreach (var declRef in classSymbol.DeclaringSyntaxReferences)
        {
            if (declRef.SyntaxTree == classDeclarationSyntax.SyntaxTree &&
                declRef.Span == classDeclarationSyntax.Span)
            {
                break;
            }

            if (IsSyntaxTarget(declRef.GetSyntax()))
            {
                return null;
            }
        }

        var hasAttribute = classSymbol.HasObservableDtoViewModelAttribute();
        if (!hasAttribute)
        {
            return null;
        }

        var (hasAnyBase, inheritFromViewModel) = classSymbol.CheckBaseClasses();

        if (!hasAnyBase)
        {
            return null;
        }

        var result = ObservableDtoViewModelInspector.Inspect(
            context.SemanticModel.Compilation,
            classSymbol,
            inheritFromViewModel);

        if (!result.FoundAnythingToGenerate)
        {
            return null;
        }

        return new ObservableDtoViewModelToGenerate(
            NamespaceName: classSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: classSymbol.Name,
            ClassAccessModifier: classSymbol.GetAccessModifier(),
            DtoTypeName: result.DtoTypeName!,
            IsDtoRecord: result.IsDtoRecord,
            HasCustomToString: result.HasCustomToString,
            UseIsDirty: result.UseIsDirty,
            EnableValidationOnPropertyChanged: result.EnableValidationOnPropertyChanged,
            EnableValidationOnInit: result.EnableValidationOnInit,
            Properties: new EquatableArray<DtoPropertyInfo>(result.Properties.ToArray()),
            Methods: new EquatableArray<DtoMethodInfo>(result.Methods.ToArray()),
            CustomProperties: new EquatableArray<ObservablePropertyToGenerate>(result.CustomProperties.ToArray()),
            CustomCommands: new EquatableArray<RelayCommandToGenerate>(result.CustomCommands.ToArray()),
            ComputedProperties: new EquatableArray<ComputedPropertyToGenerate>(result.ComputedProperties.ToArray()),
            XamlPlatform: context.SemanticModel.Compilation.GetXamlPlatform());
    }

    /// <summary>
    /// Checks if the attribute lists contain the ObservableDtoViewModel attribute.
    /// This performs a fast syntax-only check for the attribute name.
    /// </summary>
    /// <remarks>
    /// ObservableDtoViewModel attribute (platform-agnostic, works on WPF, WinUI, and Avalonia):
    /// <list type="bullet">
    /// <item><description><b>ObservableDtoViewModel</b> - Generates a ViewModel wrapper around a DTO class</description></item>
    /// </list>
    /// This attribute generates:
    /// <list type="bullet">
    /// <item><description>INotifyPropertyChanged implementation for all DTO properties</description></item>
    /// <item><description>IsDirty tracking to detect changes from the original DTO</description></item>
    /// <item><description>InnerModel property to access the underlying DTO</description></item>
    /// <item><description>Optional validation support via DataAnnotations</description></item>
    /// <item><description>Optional computed properties with dependency tracking</description></item>
    /// </list>
    /// </remarks>
    private static bool HasObservableDtoViewModelAttribute(
        SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                // Get the attribute name - for generic attributes,
                // we need to check the base identifier name.
                var attributeName = attribute.GetSimpleAttributeName();

                // Check for ObservableDtoViewModel attribute (with or without "Attribute" suffix).
                // Platform-agnostic: works across WPF, WinUI, and Avalonia.
                if (attributeName is NameConstants.ObservableDtoViewModel or NameConstants.ObservableDtoViewModelAttribute)
                {
                    return true;
                }
            }
        }

        return false;
    }
}