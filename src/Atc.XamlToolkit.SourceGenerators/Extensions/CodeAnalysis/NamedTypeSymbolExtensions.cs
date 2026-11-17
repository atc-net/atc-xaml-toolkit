// ReSharper disable InvertIf
// ReSharper disable ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
namespace Atc.XamlToolkit.SourceGenerators.Extensions.CodeAnalysis;

internal static class NamedTypeSymbolExtensions
{
    public static List<DtoPropertyInfo> ExtractProperties(
        this INamedTypeSymbol namedTypeSymbol,
        List<string>? ignorePropertyNames = null)
    {
        var ignoreSet = ignorePropertyNames is not null
            ? new HashSet<string>(ignorePropertyNames, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);

        var isRecord = namedTypeSymbol.IsRecord;

        // Get primary constructor parameters for records
        var primaryConstructorParameters = new HashSet<string>(StringComparer.Ordinal);
        if (isRecord)
        {
            var primaryConstructor = namedTypeSymbol.Constructors.FirstOrDefault(c =>
                c.Parameters.Length > 0 &&
                c.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() is RecordDeclarationSyntax);

            if (primaryConstructor is not null)
            {
                foreach (var parameter in primaryConstructor.Parameters)
                {
                    primaryConstructorParameters.Add(parameter.Name);
                }
            }
        }

        return namedTypeSymbol
            .GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public &&
                        !p.IsStatic &&
                        !ignoreSet.Contains(p.Name))
            .Select(p =>
            {
                var isRecordParameter = isRecord &&
                                        primaryConstructorParameters.Contains(p.Name);

                // A setter the view model cannot call makes the property read-only.
                // Init-only record parameters are still writable through a 'with' expression.
                var isReadOnly = p.SetMethod is null ||
                                 p.SetMethod.DeclaredAccessibility != Accessibility.Public ||
                                 (p.SetMethod.IsInitOnly && !isRecordParameter);
                var attributes = p.ExtractPropertyAttributes();
                var documentationComments = p.ExtractPropertyDocumentationComments();

                return new DtoPropertyInfo(
                    p.Name,
                    p.Type.ToString(),
                    isRecordParameter,
                    isReadOnly,
                    new EquatableArray<string>(attributes.ToArray()),
                    new EquatableArray<string>(documentationComments?.ToArray() ?? []))
                {
                    UseEqualityComparer = !p.Type.SupportsEqualityOperator(),
                };
            })
            .ToList();
    }

    public static List<DtoMethodInfo> ExtractMethods(
        this INamedTypeSymbol namedTypeSymbol,
        List<string>? ignoreMethodNames = null)
    {
        var ignoreSet = ignoreMethodNames is not null
            ? new HashSet<string>(ignoreMethodNames, StringComparer.Ordinal)
            : new HashSet<string>(StringComparer.Ordinal);

        return namedTypeSymbol
            .GetMembers()
            .OfType<IMethodSymbol>()
            .Where(m => m.DeclaredAccessibility == Accessibility.Public &&
                        m is { IsStatic: false, MethodKind: MethodKind.Ordinary, IsImplicitlyDeclared: false } &&
                        m.Name != "ToString" &&
                        !m.IsGenericMethod &&
                        !OverridesObjectMember(m) &&
                        !ignoreSet.Contains(m.Name))
            .Select(m =>
            {
                var parameters = m.Parameters
                    .Select(p => new DtoMethodParameterInfo(
                        p.Name,
                        p.Type.ToString(),
                        GetParameterModifier(p),
                        GetParameterDefaultValue(p)))
                    .ToArray();

                return new DtoMethodInfo(
                    m.Name,
                    m.ReturnType.ToString(),
                    new EquatableArray<DtoMethodParameterInfo>(parameters));
            })
            .ToList();
    }

    // Equals(object) and GetHashCode() overrides cannot be forwarded without 'override'.
    private static bool OverridesObjectMember(IMethodSymbol methodSymbol)
    {
        if (!methodSymbol.IsOverride)
        {
            return false;
        }

        var rootMethod = methodSymbol;
        while (rootMethod.OverriddenMethod is not null)
        {
            rootMethod = rootMethod.OverriddenMethod;
        }

        return rootMethod.ContainingType.SpecialType == SpecialType.System_Object;
    }

    private static string GetParameterModifier(IParameterSymbol parameterSymbol)
    {
        if (parameterSymbol.IsParams)
        {
            return "params";
        }

        return parameterSymbol.RefKind switch
        {
            RefKind.Ref => "ref",
            RefKind.Out => "out",
            RefKind.In => "in",
            _ => string.Empty,
        };
    }

    private static string? GetParameterDefaultValue(
        IParameterSymbol parameterSymbol)
    {
        if (!parameterSymbol.HasExplicitDefaultValue)
        {
            return null;
        }

        var type = parameterSymbol.Type;
        var isNullableValueType = type.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T;

        var value = parameterSymbol.ExplicitDefaultValue;
        if (value is null)
        {
            return type.IsReferenceType || isNullableValueType
                ? "null"
                : "default";
        }

        var literal = SymbolDisplay.FormatPrimitive(value, quoteStrings: true, useHexadecimalNumbers: false);

        if (isNullableValueType && type is INamedTypeSymbol { TypeArguments.Length: 1 } nullableType)
        {
            type = nullableType.TypeArguments[0];
        }

        // The default value of an enum parameter is its underlying number.
        return type.TypeKind == TypeKind.Enum
            ? $"({type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)})({literal})"
            : literal;
    }

    public static string GetAccessModifier(
        this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Private => "private",
            Accessibility.Protected => "protected",
            Accessibility.Internal => "internal",
            _ => string.Empty,
        };

    public static bool HasObservableDtoViewModelAttribute(
        this INamedTypeSymbol namedTypeSymbol)
        => namedTypeSymbol
            .GetAttributes()
            .FirstOrDefault(attr => attr.AttributeClass?.Name
                is NameConstants.ObservableDtoViewModelAttribute
                or NameConstants.ObservableDtoViewModel) is not null;

    public static bool HasBaseTypeThePropertyName(
        this INamedTypeSymbol declaringType,
        string propertyName)
    {
        var pascalName = propertyName.EnsureFirstCharacterToUpper();
        for (var baseType = declaringType.BaseType;
             baseType is not null;
             baseType = baseType.BaseType)
        {
            if (baseType
                .GetMembers(pascalName)
                .Any(m => m.Kind == SymbolKind.Property))
            {
                return true;
            }
        }

        return false;
    }

    public static bool InheritsFrom(
        this INamedTypeSymbol namedTypeSymbol,
        params string[] baseClassNames)
    {
        var baseType = namedTypeSymbol.BaseType;
        while (baseType is not null)
        {
            if (baseClassNames.Contains(baseType.Name, StringComparer.Ordinal))
            {
                return true;
            }

            baseType = baseType.BaseType;
        }

        return false;
    }

    [SuppressMessage("Style", "IDE0305:Simplify collection initialization", Justification = "OK")]
    public static IList<ClassDeclarationSyntax> GetAllPartialClassDeclarations(
        this INamedTypeSymbol classSymbol)
        => classSymbol
            .DeclaringSyntaxReferences
            .Select(r => r.GetSyntax())
            .OfType<ClassDeclarationSyntax>()
            .ToList();

    /// <summary>
    /// Checks if the class symbol inherits from ViewModelBase or ObservableObject.
    /// Since all partial declarations resolve to the same type symbol, checking
    /// inheritance once is sufficient.
    /// </summary>
    /// <param name="classSymbol">The class symbol to check.</param>
    /// <returns>
    /// A tuple containing:
    /// - HasAnyBase: True if the class inherits from any valid base class
    /// - HasViewModelBase: True if the class inherits from ViewModelBase, MainWindowViewModelBase, or ViewModelDialogBase
    /// </returns>
    public static (bool HasAnyBase, bool HasViewModelBase) CheckBaseClasses(
        this INamedTypeSymbol classSymbol)
    {
        if (classSymbol.InheritsFrom(
                NameConstants.ViewModelBase,
                NameConstants.MainWindowViewModelBase,
                NameConstants.ViewModelDialogBase))
        {
            return (true, true);
        }

        return classSymbol.InheritsFrom(NameConstants.ObservableObject)
            ? (true, false)
            : (false, false);
    }

    /// <summary>
    /// Checks if the class symbol is a valid framework element target for source generation.
    /// This checks inheritance, naming conventions, and the presence of RoutedEvent attributes.
    /// </summary>
    /// <param name="classSymbol">The class symbol to check.</param>
    /// <returns>True if the class is a valid framework element target; otherwise, false.</returns>
    public static bool HasAnythingAroundFrameworkElement(
        this INamedTypeSymbol classSymbol)
    {
        if (classSymbol.InheritsFrom(
                NameConstants.UserControl,
                NameConstants.DependencyObject,
                NameConstants.FrameworkElement))
        {
            return true;
        }

        if (classSymbol.Name.EndsWith(NameConstants.Attach, StringComparison.Ordinal) ||
            classSymbol.Name.EndsWith(NameConstants.Behavior, StringComparison.Ordinal) ||
            classSymbol.Name.EndsWith(NameConstants.Helper, StringComparison.Ordinal))
        {
            return true;
        }

        // Symbol-based check replaces SyntaxTree.ToString().Contains("[RoutedEvent")
        return classSymbol.GetMembers()
            .Any(m => m.GetAttributes()
                .Any(a => a.AttributeClass?.Name
                    is NameConstants.RoutedEventAttribute
                    or NameConstants.RoutedEvent));
    }
}