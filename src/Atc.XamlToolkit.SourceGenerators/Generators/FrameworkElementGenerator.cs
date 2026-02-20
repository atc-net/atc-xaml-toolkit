// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
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
    /// <item><description>The class name ends with "Attach", "Behavior", or "Helper" (common patterns), OR</description></item>
    /// <item><description>The class contains fields, properties, or methods that might have framework element attributes</description></item>
    /// <item><description>The class has attribute lists that might contain DependencyProperty, AttachedProperty, StyledProperty, RoutedEvent, or RelayCommand attributes</description></item>
    /// </list>
    /// The semantic analysis will then check for fields/properties with attributes like
    /// AttachedProperty, DependencyProperty, RoutedEvent, or methods with RelayCommand.
    /// </remarks>
    private static bool IsSyntaxTarget(SyntaxNode syntaxNode)
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

        // Quick check: class names ending with common patterns
        var className = classDeclaration.Identifier.Text;
        if (className.EndsWith(NameConstants.Attach, StringComparison.Ordinal) ||
            className.EndsWith(NameConstants.Behavior, StringComparison.Ordinal) ||
            className.EndsWith(NameConstants.Helper, StringComparison.Ordinal))
        {
            return true;
        }

        // Check if class itself has framework element attributes (like [DependencyProperty<T>])
        if (classDeclaration.AttributeLists.Count > 0 && HasRelevantAttribute(classDeclaration.AttributeLists))
        {
            return true;
        }

        // Check if class has any members with attributes
        // This filters out empty partial classes or those without any attributed members
        return classDeclaration.Members.Any(member => member switch
        {
            // Fields with attributes (for DependencyProperty, AttachedProperty, StyledProperty)
            FieldDeclarationSyntax { AttributeLists.Count: > 0 } field =>
                HasRelevantAttribute(field.AttributeLists),

            // Properties with attributes (for DependencyProperty, AttachedProperty, StyledProperty)
            PropertyDeclarationSyntax { AttributeLists.Count: > 0 } property =>
                HasRelevantAttribute(property.AttributeLists),

            // Methods with attributes (for RelayCommand, event handlers, callbacks)
            MethodDeclarationSyntax { AttributeLists.Count: > 0 } method =>
                HasRelevantAttribute(method.AttributeLists),

            _ => false,
        });
    }

    /// <summary>
    /// Checks if the attribute lists contain any framework element-related attributes.
    /// This performs a fast syntax-only check for attribute names.
    /// </summary>
    /// <remarks>
    /// Platform-specific attributes (checked in the predicate, filtered in transform):
    /// <list type="bullet">
    /// <item><description><b>DependencyProperty</b> - WPF, WinUI (generates DependencyProperty)</description></item>
    /// <item><description><b>AttachedProperty</b> - WPF, WinUI (generates attached DependencyProperty)</description></item>
    /// <item><description><b>StyledProperty</b> - Avalonia only (generates StyledProperty, Avalonia's equivalent to DependencyProperty)</description></item>
    /// <item><description><b>RoutedEvent</b> - WPF only (generates RoutedEvent via EventManager)</description></item>
    /// <item><description><b>RelayCommand</b> - All platforms (generates IRelayCommand)</description></item>
    /// </list>
    /// Note: The predicate accepts all attributes; platform filtering happens in the transform phase
    /// where SemanticModel is available to call GetXamlPlatform().
    /// </remarks>
    private static bool HasRelevantAttribute(
        SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                // Get the attribute name - for generic attributes like DependencyProperty<T>,
                // we need to check the base identifier name
                var attributeName = attribute.Name switch
                {
                    GenericNameSyntax genericName => genericName.Identifier.Text,
                    _ => attribute.Name.ToString(),
                };

                // Check for framework element attributes (with or without "Attribute" suffix)
                // Note: We check all attributes here; platform-specific filtering happens in GetSemanticTarget
                if (attributeName is

                    // WPF & WinUI: DependencyProperty for regular and attached properties
                    NameConstants.DependencyProperty or NameConstants.DependencyPropertyAttribute or
                    NameConstants.AttachedProperty or NameConstants.AttachedPropertyAttribute or

                    // Avalonia only: StyledProperty (Avalonia's equivalent to DependencyProperty)
                    NameConstants.StyledProperty or NameConstants.StyledPropertyAttribute or

                    // WPF only: RoutedEvent
                    NameConstants.RoutedEvent or NameConstants.RoutedEventAttribute or

                    // All platforms: RelayCommand
                    NameConstants.RelayCommand or NameConstants.RelayCommandAttribute)
                {
                    return true;
                }
            }
        }

        return false;
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
    /// <item><description>Class name ending with "Helper"</description></item>
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

        var allPartialDeclarations = classSymbol.GetAllPartialClassDeclarations();

        if (!classSymbol.HasAnythingAroundFrameworkElement())
        {
            return null;
        }

        var allAttachedProperties = new List<AttachedPropertyToGenerate>();
        var allDependencyProperties = new List<DependencyPropertyToGenerate>();
        var allRoutedEvents = new List<RoutedEventToGenerate>();
        var allRelayCommands = new List<RelayCommandToGenerate>();
        var isStatic = false;

        var xamlPlatform = context.SemanticModel.Compilation.GetXamlPlatform();

        foreach (var partialClassSyntax in allPartialDeclarations)
        {
            if (context.SemanticModel.Compilation
                    .GetSemanticModel(partialClassSyntax.SyntaxTree)
                    .GetDeclaredSymbol(partialClassSyntax) is not { } partialClassSymbol)
            {
                continue;
            }

            var result = FrameworkElementInspector.Inspect(xamlPlatform, partialClassSymbol);

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
            xamlPlatform: xamlPlatform,
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
            frameworkElementBuilder.GenerateAttachedProperties(
                frameworkElementToGenerate.XamlPlatform,
                frameworkElementToGenerate.AttachedPropertiesToGenerate);
        }

        if (frameworkElementToGenerate.DependencyPropertiesToGenerate?.Count > 0)
        {
            frameworkElementBuilder.GenerateDependencyProperties(
                frameworkElementToGenerate.XamlPlatform,
                frameworkElementToGenerate.DependencyPropertiesToGenerate);
        }

        if (frameworkElementToGenerate.RoutedEventsToGenerate?.Count > 0 &&
            frameworkElementToGenerate.XamlPlatform == XamlPlatform.Wpf)
        {
            // Routed events are only supported in WPF
            // For WinUI and Avalonia, the [RoutedEvent] attribute is silently ignored
            // as these platforms do not support the WPF RoutedEvent/EventManager pattern
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