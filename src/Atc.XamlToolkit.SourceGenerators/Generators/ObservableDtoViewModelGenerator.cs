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
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //// #if DEBUG
        ////         if (!System.Diagnostics.Debugger.IsAttached)
        ////         {
        ////             System.Diagnostics.Debugger.Launch();
        ////         }
        //// #endif

        var viewModelsToGenerate = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => ObservableDtoViewModelGeneratorHelper.IsSyntaxTarget(syntaxNode),
                transform: static (context, _) => ObservableDtoViewModelGeneratorHelper.GetSemanticTarget(context))
            .Where(static target => target is not null)
            .WithTrackingName("ObservableDtoViewModelGenerator.SemanticTarget")
            .Collect()
            .Select(static (viewModels, _) => viewModels
                .GroupBy(vm => vm!.GeneratedFileName, StringComparer.Ordinal)
                .Select(static group => group.First())
                .ToImmutableArray())
            .WithTrackingName("ObservableDtoViewModelGenerator.Deduplicated");

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

    private static void Execute(
        SourceProductionContext context,
        ObservableDtoViewModelToGenerate? viewModelToGenerate)
    {
        if (viewModelToGenerate is null)
        {
            return;
        }

        var builder = new ObservableDtoViewModelBuilder
        {
            XamlPlatform = viewModelToGenerate.XamlPlatform,
        };

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