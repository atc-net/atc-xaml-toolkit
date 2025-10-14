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
    /// <item><description>The node is a partial class declaration (required for source generation)</description></item>
    /// </list>
    /// The semantic analysis will then check for fields/properties with attributes like
    /// AttachedProperty, DependencyProperty, RoutedEvent, or methods with RelayCommand.
    /// </remarks>
    private static bool IsSyntaxTarget(
        SyntaxNode syntaxNode)
    {
        return syntaxNode is ClassDeclarationSyntax classDeclaration &&
               classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword));
    }

    /// <summary>
    /// Extracts the semantic target for code generation (transform phase).
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>A FrameworkElementToGenerate object if valid; otherwise, null.</returns>
    /// <remarks>
    /// This method uses <c>HasBaseClassFromFrameworkElementOrEndsOnAttachOrBehavior</c> to ensure the element
    /// inherits from a valid base class or follows a recognized naming convention.
    ///
    /// The valid base classes or naming conventions are:
    /// <list type="bullet">
    /// <item><description>UserControl</description></item>
    /// <item><description>DependencyObject</description></item>
    /// <item><description>FrameworkElement</description></item>
    /// <item><description>Class name ending with "Attach"</description></item>
    /// <item><description>Class name ending with "Behavior"</description></item>
    /// </list>
    /// </remarks>
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static FrameworkElementToGenerate? GetSemanticTarget(
        GeneratorSyntaxContext context)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

        if (classSymbol is null)
        {
            return null;
        }

        var allPartialDeclarations = context
            .SemanticModel
            .Compilation
            .GetAllPartialClassDeclarations(classSymbol);

        if (!allPartialDeclarations.HasAnythingAroundFrameworkElement(context))
        {
            return null;
        }

        var allAttachedProperties = new List<AttachedPropertyToGenerate>();
        var allDependencyProperties = new List<DependencyPropertyToGenerate>();
        var allRoutedEvents = new List<RoutedEventToGenerate>();
        var allRelayCommands = new List<RelayCommandToGenerate>();
        var isStatic = false;

        foreach (var partialClassSyntax in allPartialDeclarations)
        {
            if (context.SemanticModel.Compilation
                    .GetSemanticModel(partialClassSyntax.SyntaxTree)
                    .GetDeclaredSymbol(partialClassSyntax) is not { } partialClassSymbol)
            {
                continue;
            }

            var result = FrameworkElementInspector.Inspect(partialClassSymbol);

            if (!result.FoundAnythingToGenerate)
            {
                continue;
            }

            result.ApplyCommandsAndPropertiesAndEvents(
                allAttachedProperties,
                allDependencyProperties,
                allRoutedEvents,
                allRelayCommands);

            if (!isStatic)
            {
                isStatic = result.IsStatic;
            }
        }

        if (allAttachedProperties.Count == 0 &&
            allDependencyProperties.Count == 0 &&
            allRoutedEvents.Count == 0 &&
            allRelayCommands.Count == 0)
        {
            return null;
        }

        var frameworkElementToGenerate = new FrameworkElementToGenerate(
            namespaceName: classSymbol.ContainingNamespace.ToDisplayString(),
            className: classSymbol.Name,
            accessModifier: classSymbol.GetAccessModifier(),
            isStatic: isStatic)
        {
            AttachedPropertiesToGenerate = allAttachedProperties,
            DependencyPropertiesToGenerate = allDependencyProperties,
            RoutedEventsToGenerate = allRoutedEvents,
            RelayCommandsToGenerate = allRelayCommands,
        };

        return frameworkElementToGenerate;
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

        if (frameworkElementToGenerate.AttachedPropertiesToGenerate?.Count > 0)
        {
            frameworkElementBuilder.GenerateAttachedProperties(frameworkElementToGenerate.AttachedPropertiesToGenerate);
        }

        if (frameworkElementToGenerate.DependencyPropertiesToGenerate?.Count > 0)
        {
            frameworkElementBuilder.GenerateDependencyProperties(frameworkElementToGenerate.DependencyPropertiesToGenerate);
        }

        if (frameworkElementToGenerate.RoutedEventsToGenerate?.Count > 0)
        {
            frameworkElementBuilder.GenerateRoutedEvents(frameworkElementToGenerate.RoutedEventsToGenerate);
        }

        if (frameworkElementToGenerate.RelayCommandsToGenerate?.Count > 0)
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