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
                predicate: static (syntaxNode, _) => IsSyntaxTargetPartialClass(syntaxNode),
                transform: static (context, _) => GetSemanticTargetViewModelToGenerate(context))
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

    private static bool IsSyntaxTargetPartialClass(SyntaxNode syntaxNode)
        => syntaxNode.HasPartialClassDeclaration();

    private static ObservableDtoViewModelToGenerate? GetSemanticTargetViewModelToGenerate(
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

        if (!allPartialDeclarations.HasBaseClassFromList(
                context,
                NameConstants.ViewModelBase,
                NameConstants.MainWindowViewModelBase,
                NameConstants.ViewModelDialogBase,
                NameConstants.ObservableObject))
        {
            return null;
        }

        var result = ObservableDtoViewModelInspector.Inspect(context.SemanticModel.Compilation, classSymbol);

        if (!result.FoundAnythingToGenerate)
        {
            return null;
        }

        return new ObservableDtoViewModelToGenerate(
            namespaceName: classSymbol.ContainingNamespace.ToDisplayString(),
            className: classSymbol.Name,
            accessModifier: classSymbol.GetAccessModifier(),
            dtoTypeName: result.DtoTypeName!,
            properties: result.Properties);
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

        builder.GenerateConstructor(viewModelToGenerate);

        builder.GenerateProperties(viewModelToGenerate);

        builder.GenerateEnd();

        var sourceText = builder.ToSourceText();

        context.AddSource(
            viewModelToGenerate.GeneratedFileName,
            sourceText);
    }
}