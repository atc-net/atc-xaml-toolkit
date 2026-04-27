namespace Atc.XamlToolkit.SourceGenerators.Generators;

/// <summary>
/// Source generator for generating framework element properties and commands.
/// </summary>
[Generator]
public sealed class FrameworkElementGenerator : IIncrementalGenerator
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
                predicate: static (syntaxNode, _) => FrameworkElementGeneratorHelper.IsSyntaxTarget(syntaxNode),
                transform: static (context, _) => FrameworkElementGeneratorHelper.GetSemanticTarget(context))
            .Where(static target => target is not null)
            .WithTrackingName("FrameworkElementGenerator.SemanticTarget")
            .Collect()
            .Select(static (viewModels, _) => viewModels
                .GroupBy(vm => vm!.GeneratedFileName, StringComparer.Ordinal)
                .Select(static group => group.First())
                .ToImmutableArray())
            .WithTrackingName("FrameworkElementGenerator.Deduplicated");

        context.RegisterSourceOutput(
            viewModelsToGenerate,
            static (spc, sources) =>
            {
                foreach (var source in sources)
                {
                    Execute(spc, source);
                }
            });

        FrameworkElementGeneratorHelper.RegisterRoutedEventOnNonWpfDiagnostic(context);
    }

    /// <summary>
    /// Executes the source generation process.
    /// </summary>
    /// <param name="context">The source production context.</param>
    /// <param name="frameworkElementToGenerate">The framework element to generate.</param>
    private static void Execute(
        SourceProductionContext context,
        FrameworkElementToGenerate? frameworkElementToGenerate)
    {
        if (frameworkElementToGenerate is null)
        {
            return;
        }

        var frameworkElementBuilder = new FrameworkElementBuilder();

        frameworkElementBuilder.GenerateStart(frameworkElementToGenerate);

        if (frameworkElementToGenerate.AttachedPropertiesToGenerate.Count > 0)
        {
            frameworkElementBuilder.GenerateAttachedProperties(
                frameworkElementToGenerate.XamlPlatform,
                frameworkElementToGenerate.AttachedPropertiesToGenerate);
        }

        if (frameworkElementToGenerate.DependencyPropertiesToGenerate.Count > 0)
        {
            frameworkElementBuilder.GenerateDependencyProperties(
                frameworkElementToGenerate.XamlPlatform,
                frameworkElementToGenerate.DependencyPropertiesToGenerate);
        }

        if (frameworkElementToGenerate.RoutedEventsToGenerate.Count > 0 &&
            frameworkElementToGenerate.XamlPlatform == XamlPlatform.Wpf)
        {
            // Routed events are only supported in WPF.
            // For WinUI and Avalonia, the [RoutedEvent] attribute surfaces as
            // diagnostic AtcXamlToolkit0010 (see FrameworkElementGeneratorHelper).
            frameworkElementBuilder.GenerateRoutedEvents(frameworkElementToGenerate.RoutedEventsToGenerate);
        }

        if (frameworkElementToGenerate.RelayCommandsToGenerate.Count > 0)
        {
            frameworkElementBuilder.GenerateRelayCommands(frameworkElementBuilder, frameworkElementToGenerate.RelayCommandsToGenerate);
        }

        frameworkElementBuilder.GenerateEnd();

        var sourceText = frameworkElementBuilder.ToSourceText();

        context.AddSource(
            frameworkElementToGenerate.GeneratedFileName,
            sourceText);
    }
}