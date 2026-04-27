// ReSharper disable InvertIf
namespace Atc.XamlToolkit.SourceGenerators.Generators.Helpers;

/// <summary>
/// Predicate, transform, and shared symbol utilities for <see cref="ViewModelGenerator"/>.
/// Lives apart from the generator class so the generator only carries the main flow
/// (Initialize / Execute / ExecuteINotifyPropertyChanged). Diagnostic-only pipelines live
/// in <see cref="ViewModelDiagnosticHelper"/>.
/// </summary>
internal static class ViewModelGeneratorHelper
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
    /// <item><description>The class has a base list (inherits from ViewModelBase or ObservableObject)</description></item>
    /// <item><description>The class contains fields, properties, or methods with ObservableProperty, ComputedProperty, or RelayCommand attributes</description></item>
    /// </list>
    /// The semantic analysis will then check for fields/properties with ObservableProperty attribute
    /// or methods with RelayCommand attribute.
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
            FieldDeclarationSyntax { AttributeLists.Count: > 0 } field =>
                HasRelevantAttribute(field.AttributeLists),
            PropertyDeclarationSyntax { AttributeLists.Count: > 0 } property =>
                HasRelevantAttribute(property.AttributeLists),
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
    /// <param name="attributeLists">The attribute lists from a member declaration.</param>
    /// <returns>True when at least one attribute matches a ViewModel-related name.</returns>
    public static bool HasRelevantAttribute(
        SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.GetSimpleAttributeName();

                if (attributeName is

                    // Generates observable properties from fields
                    NameConstants.ObservableProperty or NameConstants.ObservablePropertyAttribute or

                    // Generates computed properties with automatic dependency tracking
                    NameConstants.ComputedProperty or NameConstants.ComputedPropertyAttribute or

                    // Generates relay commands from methods
                    NameConstants.RelayCommand or NameConstants.RelayCommandAttribute or

                    // Specifies additional properties to notify on change
                    NameConstants.NotifyPropertyChangedFor or NameConstants.NotifyPropertyChangedForAttribute or

                    // Specifies additional commands whose CanExecute should re-evaluate on change
                    NameConstants.NotifyCanExecuteChangedFor or NameConstants.NotifyCanExecuteChangedForAttribute or

                    // Opts the setter into inline validation via ObservableValidator.ValidateProperty
                    NameConstants.NotifyDataErrorInfo or NameConstants.NotifyDataErrorInfoAttribute)
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
    /// <returns>A <see cref="ViewModelToGenerate"/> object if valid; otherwise, null.</returns>
    /// <remarks>
    /// Uses <c>CheckBaseClasses</c> to ensure the ViewModel inherits from a valid base class.
    /// This check is necessary to correctly identify ViewModels even if their base class is
    /// defined in another file.
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
    public static ViewModelToGenerate? GetSemanticTarget(
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

        var (hasAnyBase, inheritFromViewModel) = classSymbol.CheckBaseClasses();

        // [INotifyPropertyChanged] gives the class its own RaisePropertyChanged via the
        // INPC pipeline, so it qualifies as a valid base for [ObservableProperty] etc.
        // even without ObservableObject / ViewModelBase inheritance.
        if (!hasAnyBase && !ClassHasINotifyPropertyChangedAttribute(classSymbol))
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

        if (allObservableProperties.Count == 0 &&
            allRelayCommands.Count == 0)
        {
            return null;
        }

        ComputedPropertyInspector.LinkToObservableProperties(allObservableProperties, allComputedProperties);

        return new ViewModelToGenerate(
            NamespaceName: classSymbol.ContainingNamespace.ToDisplayString(),
            ClassName: classSymbol.Name,
            ClassAccessModifier: classSymbol.GetAccessModifier(),
            XamlPlatform: context.SemanticModel.Compilation.GetXamlPlatform(),
            PropertiesToGenerate: new EquatableArray<ObservablePropertyToGenerate>(allObservableProperties.ToArray()),
            RelayCommandsToGenerate: new EquatableArray<RelayCommandToGenerate>(allRelayCommands.ToArray()));
    }

    /// <summary>
    /// Predicate for the <c>[INotifyPropertyChanged]</c> emission pipeline — accepts only
    /// partial classes that carry the <c>[INotifyPropertyChanged]</c> attribute.
    /// </summary>
    /// <param name="syntaxNode">The candidate syntax node.</param>
    /// <returns>True when the node is a partial class declaration carrying <c>[INotifyPropertyChanged]</c>.</returns>
    public static bool IsINotifyPropertyChangedTarget(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not ClassDeclarationSyntax classDeclaration)
        {
            return false;
        }

        if (!classDeclaration.Modifiers.Any(m => m.IsKind(SyntaxKind.PartialKeyword)))
        {
            return false;
        }

        return HasINotifyPropertyChangedAttribute(classDeclaration.AttributeLists);
    }

    /// <summary>
    /// Transform for the <c>[INotifyPropertyChanged]</c> emission pipeline — picks the first
    /// qualifying partial declaration and produces the model used by the INPC code-emission step.
    /// </summary>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>The model for the INPC code-emission step, or <c>null</c> when this declaration is not the first partial.</returns>
    public static INotifyPropertyChangedTarget? GetINotifyPropertyChangedTarget(
        GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var classSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

        if (classSymbol is null)
        {
            return null;
        }

        // Only emit once per type — pick the first qualifying partial declaration.
        foreach (var declRef in classSymbol.DeclaringSyntaxReferences)
        {
            if (declRef.SyntaxTree == classDeclaration.SyntaxTree &&
                declRef.Span == classDeclaration.Span)
            {
                break;
            }

            if (declRef.GetSyntax() is ClassDeclarationSyntax priorDecl &&
                IsINotifyPropertyChangedTarget(priorDecl))
            {
                return null;
            }
        }

        return new INotifyPropertyChangedTarget(
            namespaceName: classSymbol.ContainingNamespace.ToDisplayString(),
            className: classSymbol.Name,
            accessModifier: classSymbol.GetAccessModifier());
    }

    /// <summary>
    /// Syntax-only check for the <c>[INotifyPropertyChanged]</c> attribute. Used by both the
    /// INPC emission pipeline and by the missing-partial diagnostic.
    /// </summary>
    /// <param name="attributeLists">The attribute lists from a class declaration.</param>
    /// <returns>True when an <c>[INotifyPropertyChanged]</c> attribute is present.</returns>
    public static bool HasINotifyPropertyChangedAttribute(
        SyntaxList<AttributeListSyntax> attributeLists)
    {
        foreach (var attributeList in attributeLists)
        {
            foreach (var attribute in attributeList.Attributes)
            {
                var attributeName = attribute.GetSimpleAttributeName();

                if (attributeName is
                    NameConstants.INotifyPropertyChanged or
                    NameConstants.INotifyPropertyChangedAttribute)
                {
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Symbol-level check for the <c>[INotifyPropertyChanged]</c> attribute. Used during the
    /// main semantic transform when we already have an <see cref="INamedTypeSymbol"/> in hand.
    /// </summary>
    /// <param name="classSymbol">The class symbol to inspect.</param>
    /// <returns>True when the class carries an <c>[INotifyPropertyChanged]</c> attribute.</returns>
    public static bool ClassHasINotifyPropertyChangedAttribute(
        INamedTypeSymbol classSymbol)
    {
        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name
                is NameConstants.INotifyPropertyChanged
                or NameConstants.INotifyPropertyChangedAttribute)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Returns true when the field carries an <c>[ObservableProperty]</c> attribute. Shared by
    /// the main semantic flow and several diagnostic pipelines that need to know which fields
    /// will produce a generated property.
    /// </summary>
    /// <param name="fieldSymbol">The field symbol to inspect.</param>
    /// <returns>True when the field carries an <c>[ObservableProperty]</c> attribute.</returns>
    public static bool FieldHasObservablePropertyAttribute(
        IFieldSymbol fieldSymbol)
    {
        foreach (var attribute in fieldSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name
                is NameConstants.ObservableProperty
                or NameConstants.ObservablePropertyAttribute)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Mirrors <c>ObservablePropertyInspector</c> naming logic: honours an explicit
    /// PropertyName argument when present; otherwise derives from the camelCase field name.
    /// </summary>
    /// <param name="fieldSymbol">The <c>[ObservableProperty]</c>-decorated field to derive the property name from.</param>
    /// <returns>The PascalCase property name the generator will emit for the given field.</returns>
    public static string GetGeneratedPropertyName(IFieldSymbol fieldSymbol)
    {
        foreach (var attribute in fieldSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name is not (
                NameConstants.ObservableProperty
                or NameConstants.ObservablePropertyAttribute))
            {
                continue;
            }

            if (attribute.ConstructorArguments.Length > 0 &&
                attribute.ConstructorArguments[0].Value is string explicitName &&
                !string.IsNullOrEmpty(explicitName))
            {
                return explicitName!.EnsureFirstCharacterToUpper();
            }
        }

        return fieldSymbol.Name
            .RemovePrefixFromField()
            .EnsureFirstCharacterToUpper();
    }

    /// <summary>
    /// Returns true when the method carries a <c>[RelayCommand]</c> attribute.
    /// </summary>
    /// <param name="methodSymbol">The method symbol to inspect.</param>
    /// <returns>True when the method carries a <c>[RelayCommand]</c> attribute.</returns>
    public static bool MethodHasRelayCommandAttribute(
        IMethodSymbol methodSymbol)
    {
        foreach (var attribute in methodSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name
                is NameConstants.RelayCommand
                or NameConstants.RelayCommandAttribute)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Mirrors <c>RelayCommandInspector.AppendRelayCommandToGenerate</c> naming logic:
    /// honour Name attribute argument when present, otherwise upper-case the method name,
    /// strip "Handler" suffix, append "Command" if missing, and append "X" on collision with
    /// the source method name.
    /// </summary>
    /// <param name="methodSymbol">The <c>[RelayCommand]</c>-decorated method to derive the command name from.</param>
    /// <returns>The PascalCase command property name the generator will emit for the given method.</returns>
    public static string GetGeneratedCommandName(IMethodSymbol methodSymbol)
    {
        var commandName = methodSymbol.Name.EnsureFirstCharacterToUpper();

        foreach (var attribute in methodSymbol.GetAttributes())
        {
            if (attribute.AttributeClass?.Name is not (
                NameConstants.RelayCommand
                or NameConstants.RelayCommandAttribute))
            {
                continue;
            }

            // ExtractConstructorArgumentValues maps positional arg #0 to key
            // "Name" (the same convention RelayCommandInspector uses).
            var args = attribute.ExtractConstructorArgumentValues();
            if (args.TryGetValue(NameConstants.Name, out var explicitName) &&
                !string.IsNullOrEmpty(explicitName))
            {
                commandName = explicitName!.EnsureFirstCharacterToUpper();
            }

            break;
        }

        if (commandName.EndsWith(NameConstants.Handler, StringComparison.Ordinal))
        {
            commandName = commandName.Substring(0, commandName.Length - NameConstants.Handler.Length);
        }

        if (!commandName.EndsWith(NameConstants.Command, StringComparison.Ordinal))
        {
            commandName += NameConstants.Command;
        }

        if (commandName == methodSymbol.Name)
        {
            commandName += "X";
        }

        return commandName;
    }

    /// <summary>
    /// Pulls a string identifier out of an attribute argument that is either a
    /// literal (<c>"Foo"</c>) or a <c>nameof(Foo)</c> expression. Returns null
    /// for any other shape.
    /// </summary>
    /// <param name="argument">The attribute-argument syntax to inspect.</param>
    /// <returns>The literal value or <c>nameof</c>-target identifier; otherwise <c>null</c>.</returns>
    public static string? ExtractAttributeStringArgument(
        AttributeArgumentSyntax argument)
    {
        if (argument.Expression is LiteralExpressionSyntax literal &&
            literal.IsKind(SyntaxKind.StringLiteralExpression))
        {
            return literal.Token.ValueText;
        }

        if (argument.Expression is not InvocationExpressionSyntax invocation)
        {
            return null;
        }

        if (invocation.Expression is not IdentifierNameSyntax invokedName ||
            invokedName.Identifier.Text != "nameof" ||
            invocation.ArgumentList.Arguments.Count != 1)
        {
            return null;
        }

        return invocation.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax id
            ? id.Identifier.Text
            : null;
    }
}