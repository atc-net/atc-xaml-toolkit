// ReSharper disable ConvertIfStatementToReturnStatement
namespace Atc.XamlToolkit.SourceGenerators.Generators;

/// <summary>
/// Source generator for generating observable DTO view models.
/// </summary>
[Generator]
public sealed class ObservableDtoViewModelGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the source generator.
    /// </summary>
    /// <param name="context">The initialization context.</param>
    public void Initialize(
        IncrementalGeneratorInitializationContext context)
    {
        //// #if DEBUG
        ////         if (!System.Diagnostics.Debugger.IsAttached)
        ////         {
        ////             System.Diagnostics.Debugger.Launch();
        ////         }
        //// #endif

        var viewModelsToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsSyntaxTarget(syntaxNode),
                transform: static (context, _) => GetSemanticTarget(context))
            .Where(target => target is not null)
            .Collect()
            .Select((viewModels, _) => viewModels
                .GroupBy(vm => vm!.GeneratedFileName, StringComparer.Ordinal)
                .Select(group => group.First())
                .ToImmutableArray());

        context.RegisterSourceOutput(
            viewModelsToGenerate,
            static (spc, sources) =>
            {
                foreach (var source in sources)
                {
                    Execute(spc, source);
                }
            });
    }

    /// <summary>
    /// Determines if a given syntax node is a valid target for code generation (predicate phase).
    /// </summary>
    /// <param name="syntaxNode">The syntax node to check.</param>
    /// <returns>True if the node is a valid target; otherwise, false.</returns>
    /// <remarks>
    /// This method performs early filtering to optimize performance by checking:
    /// <list type="bullet">
    /// <item><description>The node is a partial class declaration</description></item>
    /// <item><description>The class has attributes (looking for ObservableDtoViewModel attribute)</description></item>
    /// <item><description>The class has a base list (inherits from a base class)</description></item>
    /// </list>
    /// This avoids expensive semantic model operations on classes that won't generate code.
    /// </remarks>
    private static bool IsSyntaxTarget(
        SyntaxNode syntaxNode)
        => syntaxNode is ClassDeclarationSyntax { BaseList: not null, AttributeLists.Count: > 0 } classDeclaration &&
           classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));

    /// <summary>
    /// Extracts the semantic target for code generation (transform phase).
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>An ObservableDtoViewModelToGenerate object if valid; otherwise, null.</returns>
    private static ObservableDtoViewModelToGenerate? GetSemanticTarget(
        GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

        if (classSymbol is null)
        {
            return null;
        }

        var hasAttribute = classSymbol.HasObservableDtoViewModelAttribute();
        if (!hasAttribute)
        {
            return null;
        }

        var allPartialDeclarations = context
            .SemanticModel
            .Compilation
            .GetAllPartialClassDeclarations(classSymbol);

        var (hasAnyBase, inheritFromViewModel) = allPartialDeclarations.CheckBaseClasses(context);

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
            namespaceName: classSymbol.ContainingNamespace.ToDisplayString(),
            className: classSymbol.Name,
            accessModifier: classSymbol.GetAccessModifier(),
            dtoTypeName: result.DtoTypeName!,
            isDtoRecord: result.IsDtoRecord,
            hasCustomToString: result.HasCustomToString,
            useIsDirty: result.UseIsDirty,
            enableValidationOnPropertyChanged: result.EnableValidationOnPropertyChanged,
            enableValidationOnInit: result.EnableValidationOnInit,
            properties: result.Properties,
            methods: result.Methods,
            customProperties: result.CustomProperties,
            customCommands: result.CustomCommands,
            computedProperties: result.ComputedProperties);
    }

    private static void Execute(
        SourceProductionContext context,
        ObservableDtoViewModelToGenerate? viewModelToGenerate)
    {
        if (viewModelToGenerate is null)
        {
            return;
        }

        var builder = new ObservableDtoViewModelBuilder();

        builder.GenerateStart(viewModelToGenerate);

        builder.GenerateDtoFieldAndCommandBackingFields(viewModelToGenerate);

        builder.GenerateConstructor(viewModelToGenerate);

        builder.GenerateCustomCommandProperties(viewModelToGenerate);

        builder.GenerateInnerModelProperty(viewModelToGenerate);

        builder.GenerateProperties(viewModelToGenerate);

        builder.GenerateMethods(viewModelToGenerate);

        builder.GenerateCustomProperties(viewModelToGenerate);

        builder.GenerateToString(viewModelToGenerate);

        builder.GenerateEnd();

        var sourceText = builder.ToSourceText();

        context.AddSource(
            viewModelToGenerate.GeneratedFileName,
            sourceText);
    }
}