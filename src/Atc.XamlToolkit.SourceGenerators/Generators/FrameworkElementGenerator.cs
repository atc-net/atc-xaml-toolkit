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
            .WithTrackingName("FrameworkElementGenerator.Deduplicated")
            .SelectMany(static (viewModels, _) => viewModels);

        // One output per class: an edit only regenerates the files of the classes whose model changed.
        context.RegisterSourceOutput(
            viewModelsToGenerate,
            static (spc, source) => Execute(spc, source));

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
        if (frameworkElementToGenerate is null ||
            !ReportUnusablePropertyTemplates(context, frameworkElementToGenerate))
        {
            return;
        }

        var frameworkElementBuilder = new FrameworkElementBuilder
        {
            XamlPlatform = frameworkElementToGenerate.XamlPlatform,
        };

        frameworkElementBuilder.GenerateStart(frameworkElementToGenerate);

        GenerateDefaultStyleKey(context, frameworkElementBuilder, frameworkElementToGenerate);

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

            // The cancel methods the cancel commands call, and DisposeCommands().
            frameworkElementBuilder.GenerateRelayCommandMethods(frameworkElementBuilder, frameworkElementToGenerate.RelayCommandsToGenerate);
        }

        frameworkElementBuilder.GenerateTemplateParts(frameworkElementToGenerate);

        frameworkElementBuilder.GenerateEnd();

        var sourceText = frameworkElementBuilder.ToSourceText();

        context.AddSource(
            frameworkElementToGenerate.GeneratedFileName,
            sourceText);
    }

    // Returns whether there is still code to emit for the class.
    private static bool ReportUnusablePropertyTemplates(
        SourceProductionContext context,
        FrameworkElementToGenerate frameworkElementToGenerate)
    {
        foreach (var templateName in frameworkElementToGenerate.UnusablePropertyTemplates)
        {
            context.ReportDiagnostic(
                DiagnosticFactory.CreateUnusablePropertyTemplate(templateName, frameworkElementToGenerate.ClassName));
        }

        return frameworkElementToGenerate.HasAnythingToGenerate;
    }

    private static void GenerateDefaultStyleKey(
        SourceProductionContext context,
        FrameworkElementBuilder frameworkElementBuilder,
        FrameworkElementToGenerate frameworkElementToGenerate)
    {
        if (frameworkElementToGenerate.DefaultStyleKey is not { } defaultStyleKey)
        {
            return;
        }

        frameworkElementBuilder.GenerateDefaultStyleKey(frameworkElementToGenerate);

        // The class has a static constructor of its own, so only a method was generated.
        // Unless that constructor calls it, the style key is never overridden.
        if (frameworkElementToGenerate.XamlPlatform == XamlPlatform.Wpf &&
            defaultStyleKey is { HasOwnStaticConstructor: true, OwnStaticConstructorCallsOverride: false })
        {
            context.ReportDiagnostic(
                DiagnosticFactory.CreateDefaultStyleKeyOverrideNotCalled(frameworkElementToGenerate.ClassName));
        }
    }
}