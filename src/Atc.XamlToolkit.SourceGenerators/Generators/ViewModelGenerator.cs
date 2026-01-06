// ReSharper disable InvertIf
// ReSharper disable ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.XamlToolkit.SourceGenerators.Generators;

/// <summary>
/// Source generator for generating view model properties and commands.
/// </summary>
[Generator]
public sealed class ViewModelGenerator : IIncrementalGenerator
{
    /// <summary>
    /// Initializes the source generator.
    /// </summary>
    /// <param name="context">The initialization context.</param>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        ////#if DEBUG
        ////        if (!System.Diagnostics.Debugger.IsAttached)
        ////        {
        ////            System.Diagnostics.Debugger.Launch();
        ////        }
        ////#endif

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
    /// <item><description>The class has a base list (inherits from ViewModelBase or ObservableObject)</description></item>
    /// <item><description>The class contains fields, properties, or methods with ObservableProperty, ComputedProperty, or RelayCommand attributes</description></item>
    /// </list>
    /// The semantic analysis will then check for fields/properties with ObservableProperty attribute
    /// or methods with RelayCommand attribute.
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

        // Accept 'if' has base list (might be inheriting from ViewModelBase/ObservableObject)
        // This is important for multi-file scenarios where base list and attributes are in different files
        if (classDeclaration.BaseList is not null)
        {
            return true;
        }

        // Also accept if class has any members with ViewModel-related attributes
        // This catches classes without explicit base list but using our attributes
        return classDeclaration.Members.Any(member => member switch
        {
            // Fields with attributes (for ObservableProperty)
            FieldDeclarationSyntax { AttributeLists.Count: > 0 } field =>
                HasRelevantAttribute(field.AttributeLists),

            // Properties with attributes (for ComputedProperty)
            PropertyDeclarationSyntax { AttributeLists.Count: > 0 } property =>
                HasRelevantAttribute(property.AttributeLists),

            // Methods with attributes (for RelayCommand)
            MethodDeclarationSyntax { AttributeLists.Count: > 0 } method =>
                HasRelevantAttribute(method.AttributeLists),

            _ => false,
        });
    }

    /// <summary>
    /// Checks if the attribute lists contain any ViewModel-related attributes.
    /// This performs a fast syntax-only check for attribute names.
    /// </summary>
    /// <remarks>
    /// ViewModel attributes (all platform-agnostic, work on WPF, WinUI, and Avalonia):
    /// <list type="bullet">
    /// <item><description><b>ObservableProperty</b> - Generates a property with INotifyPropertyChanged implementation from a field</description></item>
    /// <item><description><b>ComputedProperty</b> - Generates a property that automatically raises PropertyChanged when dependencies change</description></item>
    /// <item><description><b>RelayCommand</b> - Generates IRelayCommand or IRelayCommandAsync from methods</description></item>
    /// <item><description><b>NotifyPropertyChangedFor</b> - Specifies additional properties to notify when a property changes</description></item>
    /// </list>
    /// These attributes work with classes inheriting from ViewModelBase or ObservableObject.
    /// </remarks>
    private static bool HasRelevantAttribute(
        SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                // Get the attribute name - for generic attributes,
                // we need to check the base identifier name
                var attributeName = attribute.Name switch
                {
                    GenericNameSyntax genericName => genericName.Identifier.Text,
                    _ => attribute.Name.ToString(),
                };

                // Check for ViewModel attributes (with or without "Attribute" suffix)
                // All these attributes are platform-agnostic and work across WPF, WinUI, and Avalonia
                if (attributeName is

                    // Generates observable properties from fields
                    NameConstants.ObservableProperty or NameConstants.ObservablePropertyAttribute or

                    // Generates computed properties with automatic dependency tracking
                    NameConstants.ComputedProperty or NameConstants.ComputedPropertyAttribute or

                    // Generates relay commands from methods
                    NameConstants.RelayCommand or NameConstants.RelayCommandAttribute or

                    // Specifies additional properties to notify on change
                    NameConstants.NotifyPropertyChangedFor or NameConstants.NotifyPropertyChangedForAttribute)
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
    /// <returns>A ViewModelToGenerate object if valid; otherwise, null.</returns>
    /// <remarks>
    /// This method uses <c>HasBaseClassFromList</c> to ensure the ViewModel inherits
    /// from a valid base class. This check is necessary to correctly identify ViewModels
    /// even if their base class is defined in another file.
    ///
    /// The valid base classes are:
    /// <list type="bullet">
    /// <item><description>ViewModelBase</description></item>
    /// <item><description>MainWindowViewModelBase</description></item>
    /// <item><description>ViewModelDialogBase</description></item>
    /// <item><description>ObservableObject</description></item>
    /// </list>
    /// </remarks>
    [SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
    private static ViewModelToGenerate? GetSemanticTarget(
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

        var (hasAnyBase, inheritFromViewModel) = allPartialDeclarations.CheckBaseClasses(context);

        if (!hasAnyBase)
        {
            return null;
        }

        var allObservableProperties = new List<ObservablePropertyToGenerate>();
        var allRelayCommands = new List<RelayCommandToGenerate>();
        var allComputedProperties = new List<ComputedPropertyToGenerate>();

        foreach (var partialClassSyntax in allPartialDeclarations)
        {
            if (context.SemanticModel.Compilation
                    .GetSemanticModel(partialClassSyntax.SyntaxTree)
                    .GetDeclaredSymbol(partialClassSyntax) is not { } partialClassSymbol)
            {
                continue;
            }

            var result = ViewModelInspector.Inspect(
                partialClassSymbol,
                inheritFromViewModel);

            if (!result.FoundAnythingToGenerate)
            {
                continue;
            }

            result.ApplyCommandsAndProperties(
                allObservableProperties,
                allRelayCommands,
                allComputedProperties);
        }

        if (allObservableProperties.Count == 00 &&
            allRelayCommands.Count == 0)
        {
            return null;
        }

        LinkComputedPropertiesToObservableProperties(allObservableProperties, allComputedProperties);

        var viewModelToGenerate = new ViewModelToGenerate(
            namespaceName: classSymbol.ContainingNamespace.ToDisplayString(),
            className: classSymbol.Name,
            accessModifier: classSymbol.GetAccessModifier())
        {
            XamlPlatform = context.SemanticModel.Compilation.GetXamlPlatform(),
            PropertiesToGenerate = allObservableProperties,
            RelayCommandsToGenerate = allRelayCommands,
        };

        return viewModelToGenerate;
    }

    /// <summary>
    /// Links computed properties to observable properties by adding them to the PropertyNamesToInvalidate list.
    /// </summary>
    /// <param name="observableProperties">The list of observable properties.</param>
    /// <param name="computedProperties">The list of computed properties.</param>
    private static void LinkComputedPropertiesToObservableProperties(
        List<ObservablePropertyToGenerate> observableProperties,
        List<ComputedPropertyToGenerate> computedProperties)
    {
        foreach (var observableProperty in observableProperties)
        {
            foreach (var computedProperty in computedProperties)
            {
                if (computedProperty.DependentPropertyNames.Contains(observableProperty.Name, StringComparer.Ordinal))
                {
                    observableProperty.PropertyNamesToInvalidate ??= [];
                    if (!observableProperty.PropertyNamesToInvalidate.Contains(computedProperty.Name, StringComparer.Ordinal))
                    {
                        observableProperty.PropertyNamesToInvalidate.Add(computedProperty.Name);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Executes the source generation process.
    /// </summary>
    /// <param name="context">The source production context.</param>
    /// <param name="viewModelToGenerate">The ViewModel to generate.</param>
    private static void Execute(
        SourceProductionContext context,
        ViewModelToGenerate? viewModelToGenerate)
    {
        if (viewModelToGenerate is null)
        {
            return;
        }

        var viewModelBuilder = new ViewModelBuilder
        {
            XamlPlatform = viewModelToGenerate.XamlPlatform,
        };

        viewModelBuilder.GenerateStart(viewModelToGenerate);

        if (viewModelToGenerate.ContainsRelayCommandNameDuplicates)
        {
            context.ReportDiagnostic(DiagnosticFactory.CreateContainsDuplicateNamesForRelayCommand());
        }
        else
        {
            viewModelBuilder.GenerateRelayCommands(viewModelBuilder, viewModelToGenerate.RelayCommandsToGenerate);
        }

        viewModelBuilder.GenerateProperties(viewModelToGenerate.PropertiesToGenerate);

        viewModelBuilder.GenerateRelayCommandMethods(viewModelBuilder, viewModelToGenerate.RelayCommandsToGenerate);

        viewModelBuilder.GenerateEnd();

        var sourceText = viewModelBuilder.ToSourceText();

        context.AddSource(
            viewModelToGenerate.GeneratedFileName,
            sourceText);
    }
}