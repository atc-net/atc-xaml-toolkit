namespace Atc.XamlToolkit.SourceGenerators.Extensions.Builder;

[SuppressMessage("Design", "CA1308:Teplace the call to 'ToLowerInvariant' with 'ToUpperInvariant'", Justification = "OK.")]
[SuppressMessage("Design", "MA0051:Method is too long", Justification = "OK.")]
internal static class FrameworkElementBuilderExtensions
{
    public static void GenerateStart(
        this FrameworkElementBuilder builder,
        FrameworkElementToGenerate frameworkElementToGenerate)
    {
        builder.AppendLine("// <auto-generated>");
        builder.AppendLine("#nullable enable");
        if (frameworkElementToGenerate.DependencyPropertiesToGenerate is not null &&
            frameworkElementToGenerate.DependencyPropertiesToGenerate.Any(x => x.DefaultUpdateSourceTrigger is not null))
        {
            builder.AppendLine("using System.Windows.Data;");
        }

        if (frameworkElementToGenerate.RelayCommandsToGenerate?.Count > 0)
        {
            builder.AppendLine("using Atc.XamlToolkit.Command;");
        }

        builder.AppendLine();
        builder.AppendLine($"namespace {frameworkElementToGenerate.NamespaceName};");
        builder.AppendLine();
        builder.AppendLine(frameworkElementToGenerate.IsStatic
            ? $"{frameworkElementToGenerate.ClassAccessModifier} static partial class {frameworkElementToGenerate.ClassName}"
            : $"{frameworkElementToGenerate.ClassAccessModifier} partial class {frameworkElementToGenerate.ClassName}");
        builder.AppendLine("{");
        builder.IncreaseIndent();
    }

    public static void GenerateAttachedProperties(
        this FrameworkElementBuilder builder,
        IEnumerable<AttachedPropertyToGenerate>? attachedPropertiesToGenerate)
    {
        if (attachedPropertiesToGenerate is null)
        {
            return;
        }

        foreach (var propertyToGenerate in attachedPropertiesToGenerate)
        {
            GenerateAttachedProperty(builder, propertyToGenerate);
        }
    }

    public static void GenerateDependencyProperties(
        this FrameworkElementBuilder builder,
        IEnumerable<DependencyPropertyToGenerate>? dependencyPropertiesToGenerate)
    {
        if (dependencyPropertiesToGenerate is null)
        {
            return;
        }

        foreach (var propertyToGenerate in dependencyPropertiesToGenerate)
        {
            GenerateDependencyProperty(builder, propertyToGenerate);
        }
    }

    public static void GenerateRoutedEvents(
        this FrameworkElementBuilder builder,
        IEnumerable<RoutedEventToGenerate>? routedEventToGenerates)
    {
        if (routedEventToGenerates is null)
        {
            return;
        }

        foreach (var routedEventToGenerate in routedEventToGenerates)
        {
            GenerateRoutedEvent(builder, routedEventToGenerate);
        }
    }

    private static void GenerateAttachedProperty(
        FrameworkElementBuilder builder,
        AttachedPropertyToGenerate p)
    {
        GenerateDependencyPropertyHeader(builder, p, isAttached: true);
        GenerateDependencyPropertyBody(builder, p);
        GenerateClrAttachedMethods(builder, p);
    }

    private static void GenerateDependencyProperty(
        FrameworkElementBuilder builder,
        DependencyPropertyToGenerate p)
    {
        GenerateDependencyPropertyHeader(builder, p, isAttached: false);
        GenerateDependencyPropertyBody(builder, p);
        GenerateClrDependencyProperty(builder, p);
    }

    private static void GenerateDependencyPropertyHeader(
        FrameworkElementBuilder builder,
        BaseFrameworkElementPropertyToGenerate p,
        bool isAttached)
    {
        var registerMethod = isAttached
            ? "RegisterAttached"
            : "Register";

        builder.AppendLineBeforeMember();
        builder.AppendLine(
            p.UseNewKeyword
                ? $"public static new readonly DependencyProperty {p.Name}Property = DependencyProperty.{registerMethod}("
                : $"public static readonly DependencyProperty {p.Name}Property = DependencyProperty.{registerMethod}(");
        builder.IncreaseIndent();

        if (isAttached || p.IsOwnerTypeStatic)
        {
            builder.AppendLine($"\"{p.Name}\",");
        }
        else
        {
            builder.AppendLine($"{p.Name.EnsureNameofContent()},");
        }

        builder.AppendLine($"typeof({p.Type.TrimNullableForTypeOf()}),");
    }

    private static void GenerateDependencyPropertyBody(
        FrameworkElementBuilder builder,
        BaseFrameworkElementPropertyToGenerate p)
    {
        if (p.HasAnyMetadata)
        {
            builder.AppendLine($"typeof({p.OwnerType.RemoveNullableSuffix()}),");

            if (string.IsNullOrEmpty(p.Flags) &&
                string.IsNullOrEmpty(p.PropertyChangedCallback) &&
                string.IsNullOrEmpty(p.CoerceValueCallback))
            {
                if (p.HasAnyValidateValueCallback)
                {
                    if (p.DefaultValue is not null && !"null".Equals(p.DefaultValue))
                    {
                        builder.AppendLine($"new PropertyMetadata(defaultValue: {p.DefaultValue}),");
                    }
                    else
                    {
                        builder.AppendLine("new PropertyMetadata(defaultValue: null),");
                    }

                    builder.DecreaseIndent();
                    builder.AppendLine($"validateValueCallback: {p.ValidateValueCallback});");
                }
                else
                {
                    if (p.DefaultValue is not null && !"null".Equals(p.DefaultValue))
                    {
                        builder.AppendLine($"new PropertyMetadata(defaultValue: {p.DefaultValue}));");
                    }
                    else
                    {
                        builder.AppendLine("new PropertyMetadata());");
                    }

                    builder.DecreaseIndent();
                }
            }
            else
            {
                if (string.IsNullOrEmpty(p.Flags) &&
                    string.IsNullOrEmpty(p.DefaultUpdateSourceTrigger) &&
                    p.IsAnimationProhibited is null)
                {
                    if (p.HasAnyValidateValueCallback)
                    {
                        GeneratePropertyMetadataExtended(builder, p, endWithComma: true);
                        builder.DecreaseIndent();
                        builder.AppendLine($"validateValueCallback: {p.ValidateValueCallback});");
                    }
                    else
                    {
                        GeneratePropertyMetadataExtended(builder, p, endWithComma: false);
                        builder.DecreaseIndent();
                    }
                }
                else
                {
                    if (p.HasAnyValidateValueCallback)
                    {
                        GenerateFrameworkPropertyMetadata(builder, p, endWithComma: true);
                        builder.DecreaseIndent();
                        builder.AppendLine($"validateValueCallback: {p.ValidateValueCallback});");
                    }
                    else
                    {
                        GenerateFrameworkPropertyMetadata(builder, p, endWithComma: false);
                        builder.DecreaseIndent();
                    }
                }

                builder.DecreaseIndent();
            }
        }
        else
        {
            if (p.HasAnyValidateValueCallback)
            {
                builder.AppendLine($"typeof({p.OwnerType.RemoveNullableSuffix()}),");
                builder.DecreaseIndent();
                builder.AppendLine($"validateValueCallback: {p.ValidateValueCallback});");
            }
            else
            {
                builder.AppendLine($"typeof({p.OwnerType.RemoveNullableSuffix()}));");
                builder.DecreaseIndent();
            }
        }
    }

    private static void GeneratePropertyMetadataExtended(
        FrameworkElementBuilder builder,
        BaseFrameworkElementPropertyToGenerate p,
        bool endWithComma)
    {
        builder.AppendLine("new PropertyMetadata(");
        builder.IncreaseIndent();
        if (p.DefaultValue is not null && !"null".Equals(p.DefaultValue))
        {
            builder.AppendLine($"defaultValue: {p.DefaultValue},");
        }
        else
        {
            builder.AppendLine("defaultValue: null,");
        }

        if (string.IsNullOrEmpty(p.PropertyChangedCallback))
        {
            builder.AppendLine(
                endWithComma
                    ? $"coerceValueCallback: {p.CoerceValueCallback}),"
                    : $"coerceValueCallback: {p.CoerceValueCallback}));");
        }
        else if (string.IsNullOrEmpty(p.CoerceValueCallback))
        {
            builder.AppendLine(
                endWithComma
                    ? $"propertyChangedCallback: {p.PropertyChangedCallback}),"
                    : $"propertyChangedCallback: {p.PropertyChangedCallback}));");
        }
        else
        {
            builder.AppendLine($"propertyChangedCallback: {p.PropertyChangedCallback},");
            builder.AppendLine(
                endWithComma
                    ? $"coerceValueCallback: {p.CoerceValueCallback}),"
                    : $"coerceValueCallback: {p.CoerceValueCallback}));");
        }
    }

    private static void GenerateFrameworkPropertyMetadata(
        FrameworkElementBuilder builder,
        BaseFrameworkElementPropertyToGenerate p,
        bool endWithComma)
    {
        builder.AppendLine("new FrameworkPropertyMetadata(");
        builder.IncreaseIndent();
        if (p.DefaultValue is not null && !"null".Equals(p.DefaultValue))
        {
            builder.AppendLine($"defaultValue: {p.DefaultValue},");
        }
        else
        {
            builder.AppendLine("defaultValue: null,");
        }

        if (!string.IsNullOrEmpty(p.PropertyChangedCallback))
        {
            builder.AppendLine($"propertyChangedCallback: {p.PropertyChangedCallback},");
        }

        if (!string.IsNullOrEmpty(p.CoerceValueCallback))
        {
            builder.AppendLine($"coerceValueCallback: {p.CoerceValueCallback},");
        }

        var hasAddedLine = false;
        if (!string.IsNullOrEmpty(p.Flags))
        {
            builder.Append($"flags: {p.Flags}");
            hasAddedLine = true;
        }

        if (!string.IsNullOrEmpty(p.DefaultUpdateSourceTrigger))
        {
            if (hasAddedLine)
            {
                builder.AppendLine(",");
            }

            if (string.IsNullOrEmpty(p.CoerceValueCallback))
            {
                builder.AppendLine("coerceValueCallback: null,");
            }

            if (!p.IsAnimationProhibited.HasValue)
            {
                builder.AppendLine("isAnimationProhibited: false,");
            }

            builder.Append($"defaultUpdateSourceTrigger: {p.DefaultUpdateSourceTrigger}");
        }

        if (p.IsAnimationProhibited.HasValue)
        {
            if (hasAddedLine)
            {
                builder.AppendLine(",");
            }

            if (string.IsNullOrEmpty(p.DefaultUpdateSourceTrigger) &&
                string.IsNullOrEmpty(p.CoerceValueCallback))
            {
                builder.AppendLine("coerceValueCallback: null,");
            }

            builder.Append($"isAnimationProhibited: {p.IsAnimationProhibited.Value.ToString().ToLowerInvariant()}");
        }

        builder.AppendLine(
            endWithComma
                ? "),"
                : "));");
    }

    private static void GenerateClrDependencyProperty(
        FrameworkElementBuilder builder,
        DependencyPropertyToGenerate p)
    {
        builder.AppendLine();

        if (p.IsOwnerTypeStatic)
        {
            if (!string.IsNullOrEmpty(p.Category))
            {
                builder.AppendLine($"[Category(\"{p.Category}\")]");
            }

            if (!string.IsNullOrEmpty(p.Description))
            {
                builder.AppendLine($"[Description(\"Get: {p.Description}\")]");
            }

            builder.AppendLine($"public static {p.Type} Get{p.Name}(UIElement element)");
            builder.IncreaseIndent();
            builder.AppendLine($"=> ({p.Type})element.GetValue({p.Name}Property);");
            builder.DecreaseIndent();

            builder.AppendLine();

            if (!string.IsNullOrEmpty(p.Category))
            {
                builder.AppendLine($"[Category(\"{p.Category}\")]");
            }

            if (!string.IsNullOrEmpty(p.Description))
            {
                builder.AppendLine($"[Description(\"Set: {p.Description}\")]");
            }

            builder.AppendLine($"public static void Set{p.Name}(UIElement element, {p.Type} value)");
            builder.IncreaseIndent();
            builder.AppendLine(
                p.Type.StartsWith("bool", StringComparison.Ordinal)
                ? $"=> element?.SetValue({p.Name}Property, BooleanBoxes.Box(value));"
                : $"=> element?.SetValue({p.Name}Property, value);");
            builder.DecreaseIndent();
        }
        else
        {
            if (!string.IsNullOrEmpty(p.Category))
            {
                builder.AppendLine($"[Category(\"{p.Category}\")]");
            }

            if (!string.IsNullOrEmpty(p.Description))
            {
                builder.AppendLine($"[Description(\"{p.Description}\")]");
            }

            builder.AppendLine(
                p.UseNewKeyword
                    ? $"public new {p.Type} {p.Name}"
                    : $"public {p.Type} {p.Name}");
            builder.AppendLine("{");
            builder.IncreaseIndent();
            builder.AppendLine($"get => ({p.Type})GetValue({p.Name}Property);");
            if (!p.IsReadOnly)
            {
                builder.AppendLine(
                    p.Type.StartsWith("bool", StringComparison.Ordinal)
                    ? $"set => SetValue({p.Name}Property, BooleanBoxes.Box(value));"
                    : $"set => SetValue({p.Name}Property, value);");
            }

            builder.DecreaseIndent();
            builder.AppendLine("}");
        }
    }

    private static void GenerateClrAttachedMethods(
        FrameworkElementBuilder builder,
        AttachedPropertyToGenerate p)
    {
        builder.AppendLine();

        if (!string.IsNullOrEmpty(p.Category))
        {
            builder.AppendLine($"[Category(\"{p.Category}\")]");
        }

        if (!string.IsNullOrEmpty(p.Description))
        {
            builder.AppendLine($"[Description(\"Get: {p.Description}\")]");
        }

        builder.AppendLine($"public static {p.Type} Get{p.Name}(UIElement element)");
        builder.IncreaseIndent();
        builder.AppendLine($"=> ({p.Type})element.GetValue({p.Name}Property);");
        if (!p.IsReadOnly)
        {
            builder.DecreaseIndent();

            builder.AppendLine();

            if (!string.IsNullOrEmpty(p.Category))
            {
                builder.AppendLine($"[Category(\"{p.Category}\")]");
            }

            if (!string.IsNullOrEmpty(p.Description))
            {
                builder.AppendLine($"[Description(\"Set: {p.Description}\")]");
            }

            builder.AppendLine($"public static void Set{p.Name}(UIElement element, {p.Type} value)");
            builder.IncreaseIndent();
            builder.AppendLine(
                p.Type.StartsWith("bool", StringComparison.Ordinal)
                ? $"=> element?.SetValue({p.Name}Property, BooleanBoxes.Box(value));"
                : $"=> element?.SetValue({p.Name}Property, value);");
        }

        builder.DecreaseIndent();
    }

    private static void GenerateRoutedEvent(
        FrameworkElementBuilder builder,
        RoutedEventToGenerate re)
    {
        builder.AppendLineBeforeMember();
        builder.AppendLine($"public static readonly RoutedEvent {re.Name}Event = EventManager.RegisterRoutedEvent(");
        builder.IncreaseIndent();
        builder.AppendLine($"name: nameof({re.Name}),");
        builder.AppendLine(string.IsNullOrEmpty(re.RoutingStrategy)
            ? "routingStrategy: RoutingStrategy.Bubble,"
            : $"routingStrategy: RoutingStrategy.{re.RoutingStrategy},");
        builder.AppendLine($"handlerType: typeof({re.HandlerTypeName}),");
        builder.AppendLine($"ownerType: typeof({re.OwnerType}));");
        builder.DecreaseIndent();
        builder.AppendLine();
        builder.AppendLine($"public event {re.HandlerTypeName} {re.Name}");
        builder.AppendLine("{");
        builder.IncreaseIndent();
        builder.AppendLine($"add => AddHandler({re.Name}Event, value);");
        builder.AppendLine($"remove => RemoveHandler({re.Name}Event, value);");
        builder.DecreaseIndent();
        builder.AppendLine("}");
    }
}