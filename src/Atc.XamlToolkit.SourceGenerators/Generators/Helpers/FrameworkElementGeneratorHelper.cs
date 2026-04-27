// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.XamlToolkit.SourceGenerators.Generators.Helpers;

/// <summary>
/// Predicate, transform, and diagnostic-pipeline helpers for <see cref="FrameworkElementGenerator"/>.
/// Lives apart from the generator class so the generator only carries the main flow
/// (Initialize / Execute).
/// </summary>
internal static class FrameworkElementGeneratorHelper
{
    /// <summary>
    /// Determines if a given syntax node is a valid target for code generation (predicate phase).
    /// </summary>
    /// <param name="syntaxNode">The syntax node to check.</param>
    /// <returns>True if the node is a valid target; otherwise, false.</returns>
    /// <remarks>
    /// Performs early filtering to optimize performance by checking:
    /// <list type="bullet">
    /// <item><description>The node is a partial class declaration (required for source generation)</description></item>
    /// <item><description>The class name ends with "Attach", "Behavior", or "Helper" (common patterns), OR</description></item>
    /// <item><description>The class contains fields, properties, or methods that might have framework element attributes</description></item>
    /// <item><description>The class has attribute lists that might contain DependencyProperty, AttachedProperty, StyledProperty, RoutedEvent, or RelayCommand attributes</description></item>
    /// </list>
    /// Semantic analysis (in <see cref="GetSemanticTarget"/>) then checks for fields/properties with
    /// AttachedProperty, DependencyProperty, RoutedEvent, or methods with RelayCommand.
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
    /// Extracts the semantic target for code generation (transform phase).
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>A <see cref="FrameworkElementToGenerate"/> object if valid; otherwise, null.</returns>
    /// <remarks>
    /// Uses <c>HasAnythingAroundFrameworkElement</c> to ensure the element inherits from a valid
    /// base class or follows a recognized naming convention.
    ///
    /// Valid base classes or naming conventions are:
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
    public static FrameworkElementToGenerate? GetSemanticTarget(
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

        return new FrameworkElementToGenerate(
            XamlPlatform: xamlPlatform,
            NamespaceName: classSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: classSymbol.Name,
            ClassAccessModifier: classSymbol.GetAccessModifier(),
            IsStatic: isStatic,
            AttachedPropertiesToGenerate: new EquatableArray<AttachedPropertyToGenerate>(allAttachedProperties.ToArray()),
            DependencyPropertiesToGenerate: new EquatableArray<DependencyPropertyToGenerate>(allDependencyProperties.ToArray()),
            RoutedEventsToGenerate: new EquatableArray<RoutedEventToGenerate>(allRoutedEvents.ToArray()),
            RelayCommandsToGenerate: new EquatableArray<RelayCommandToGenerate>(allRelayCommands.ToArray()));
    }

    /// <summary>
    /// Registers the <c>AtcXamlToolkit0010</c> diagnostic pipeline that surfaces a build-time
    /// warning when <c>[RoutedEvent]</c> is detected on a WinUI 3 or Avalonia compilation.
    /// Without this, the generator would silently emit no code — the developer would expect the
    /// routed event to materialize and find nothing, with no build-time clue. WPF supports
    /// RoutedEvent natively; the other platforms have no EventManager equivalent.
    /// </summary>
    /// <param name="context">The incremental generator initialization context.</param>
    public static void RegisterRoutedEventOnNonWpfDiagnostic(
        IncrementalGeneratorInitializationContext context)
    {
        var routedEventCandidates = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (syntaxNode, _) => IsRoutedEventCandidate(syntaxNode),
                transform: static (ctx, _) => GetRoutedEventLocations(ctx))
            .Where(static result => result is not null && result.Locations.Length > 0)
            .Combine(context.CompilationProvider.Select(static (c, _) => c.GetXamlPlatform()))
            .Where(static pair => pair.Right != XamlPlatform.Wpf);

        context.RegisterSourceOutput(
            routedEventCandidates,
            static (spc, pair) =>
            {
                var (candidate, platform) = pair;
                if (candidate is null)
                {
                    return;
                }

                var platformName = platform switch
                {
                    XamlPlatform.WinUI => "WinUI 3",
                    XamlPlatform.Avalonia => "Avalonia",
                    _ => platform.ToString(),
                };

                foreach (var (fieldName, location) in candidate.Locations)
                {
                    spc.ReportDiagnostic(
                        DiagnosticFactory.CreateRoutedEventOnNonWpf(
                            fieldName,
                            platformName,
                            location));
                }
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
    /// Note: The predicate accepts all attributes; platform-specific filtering happens in the transform phase
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

    private static bool IsRoutedEventCandidate(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        foreach (var member in classDeclaration.Members)
        {
            if (member is not FieldDeclarationSyntax field)
            {
                continue;
            }

            foreach (var attributeList in field.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name switch
                    {
                        GenericNameSyntax generic => generic.Identifier.Text,
                        _ => attribute.Name.ToString(),
                    };

                    if (attributeName is NameConstants.RoutedEvent or NameConstants.RoutedEventAttribute)
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private static RoutedEventLocations? GetRoutedEventLocations(
        GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var locations = new List<(string FieldName, Location Location)>();

        foreach (var member in classDeclaration.Members)
        {
            if (member is not FieldDeclarationSyntax field)
            {
                continue;
            }

            foreach (var attributeList in field.AttributeLists)
            {
                foreach (var attribute in attributeList.Attributes)
                {
                    var attributeName = attribute.Name switch
                    {
                        GenericNameSyntax generic => generic.Identifier.Text,
                        _ => attribute.Name.ToString(),
                    };

                    if (attributeName is not (NameConstants.RoutedEvent or NameConstants.RoutedEventAttribute))
                    {
                        continue;
                    }

                    foreach (var variable in field.Declaration.Variables)
                    {
                        locations.Add((variable.Identifier.Text, variable.Identifier.GetLocation()));
                    }
                }
            }
        }

        return locations.Count == 0
            ? null
            : new RoutedEventLocations(new EquatableArray<(string, Location)>(locations.ToArray()));
    }
}