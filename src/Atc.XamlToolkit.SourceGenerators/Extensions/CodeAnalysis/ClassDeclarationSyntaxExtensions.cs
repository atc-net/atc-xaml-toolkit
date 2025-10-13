namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class ClassDeclarationSyntaxExtensions
{
    public static string GetNamespace(
        this ClassDeclarationSyntax classDeclaration)
    {
        // Check for file-scoped namespace declaration (C# 10+)
        var fileScopedNamespace = classDeclaration.SyntaxTree.GetRoot()
            .DescendantNodes()
            .OfType<FileScopedNamespaceDeclarationSyntax>()
            .FirstOrDefault();

        if (fileScopedNamespace is not null)
        {
            return fileScopedNamespace.Name.ToString();
        }

        // Check for traditional namespace declaration
        var namespaceDeclaration = classDeclaration.Ancestors()
            .OfType<NamespaceDeclarationSyntax>()
            .FirstOrDefault();

        return namespaceDeclaration?.Name.ToString() ?? string.Empty;
    }

    public static bool HasBaseClassFromList(
        this IEnumerable<ClassDeclarationSyntax> partialDeclarations,
        GeneratorSyntaxContext context,
        params string[] baseClassNames)
        => partialDeclarations.Any(
            declaration =>
            {
                var semanticModel = context.SemanticModel.Compilation.GetSemanticModel(declaration.SyntaxTree);
                var symbol = semanticModel.GetDeclaredSymbol(declaration);
                return symbol?.InheritsFrom(baseClassNames) == true;
            });

    /// <summary>
    /// Checks if the partial class declarations inherit from ViewModelBase or ObservableObject.
    /// This method optimizes performance by checking ViewModelBase classes first and only
    /// checking ObservableObject if needed.
    /// </summary>
    /// <param name="partialDeclarations">The partial class declarations to check.</param>
    /// <param name="context">The generator syntax context.</param>
    /// <returns>
    /// A tuple containing:
    /// - HasAnyBase: True if the class inherits from any valid base class
    /// - HasViewModelBase: True if the class inherits from ViewModelBase, MainWindowViewModelBase, or ViewModelDialogBase
    /// </returns>
    public static (bool HasAnyBase, bool HasViewModelBase) CheckBaseClasses(
        this IEnumerable<ClassDeclarationSyntax> partialDeclarations,
        GeneratorSyntaxContext context)
    {
        var partialDeclarationList = partialDeclarations.ToList();

        var hasViewModelBase = partialDeclarationList.HasBaseClassFromList(
            context,
            NameConstants.ViewModelBase,
            NameConstants.MainWindowViewModelBase,
            NameConstants.ViewModelDialogBase);

        if (hasViewModelBase)
        {
            return (true, true);
        }

        var hasObservableObject = partialDeclarationList.HasBaseClassFromList(
            context,
            NameConstants.ObservableObject);

        return (hasObservableObject, false);
    }

    public static bool HasAnythingAroundFrameworkElement(
        this IEnumerable<ClassDeclarationSyntax> declarations,
        GeneratorSyntaxContext context)
        => declarations.Any(declaration =>
        {
            var semanticModel = context.SemanticModel.Compilation.GetSemanticModel(declaration.SyntaxTree);
            var symbol = semanticModel.GetDeclaredSymbol(declaration);

            if (symbol?.InheritsFrom(
                    NameConstants.UserControl,
                    NameConstants.DependencyObject,
                    NameConstants.FrameworkElement) == true)
            {
                return true;
            }

            if (symbol?.Name.EndsWith(NameConstants.Attach, StringComparison.Ordinal) == true ||
                symbol?.Name.EndsWith(NameConstants.Behavior, StringComparison.Ordinal) == true)
            {
                return true;
            }

            var code = semanticModel.SyntaxTree.ToString();

            return code.Contains($"[{NameConstants.RoutedEvent}");
        });
}