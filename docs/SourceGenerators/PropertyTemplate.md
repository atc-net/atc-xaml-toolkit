# 🧩 Property templates

Several controls often need the same set of dependency properties and routed events. A hardware picker for drives and one for printers have the same `Value`, `WatermarkText`, `ShowRefreshButton` and `ValueChanged`; only the item type differs. A common base class is not always possible, for example when each control is a XAML-backed `UserControl`.

A **property template** declares the members once and applies them to each control. The source generator emits, in every control that uses the template, exactly the code it would emit if the template's fields were declared in the control itself.

## 🚀 Declaring a template

Mark a class with `[DependencyPropertyTemplate]` and give it the same attributed fields you would put in a control:

```csharp
[DependencyPropertyTemplate]
internal abstract class PickerMembers<TInfo>
{
    /// <summary>
    /// Gets or sets the selected item.
    /// </summary>
    [DependencyProperty(Flags = FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, GeneratePartialHooks = true)]
    private TInfo value;

    [DependencyProperty(DefaultValue = "")]
    private string watermarkText;

    [DependencyProperty(DefaultValue = true)]
    private bool showRefreshButton;

    [RoutedEvent(HandlerType = typeof(RoutedPropertyChangedEventHandler<>), GenerateRaiseMethod = true)]
    private static readonly RoutedEvent valueChanged;
}
```

- The class is never instantiated and gets no generated code of its own. Make it `abstract`; it does not have to be `partial` or derive from anything.
- It can be generic. The type arguments given where the template is used replace the type parameters in the field types.
- Every option of `[DependencyProperty]`, `[StyledProperty]`, `[AttachedProperty]` and `[RoutedEvent]` works, and XML doc comments on the fields are copied to the generated members.

## 🔗 Using a template

```csharp
[UseDependencyPropertyTemplate(typeof(PickerMembers<DriveInfo?>))]
public partial class DrivePicker : UserControl
{
    partial void OnValueChanged(DriveInfo? oldValue, DriveInfo? newValue)
        => RaiseValueChanged(oldValue, newValue);
}

[UseDependencyPropertyTemplate(typeof(PickerMembers<PrinterInfo?>))]
public partial class PrinterPicker : UserControl
{
}
```

Each control gets its own `ValueProperty`, `WatermarkTextProperty`, `ShowRefreshButtonProperty` and `ValueChangedEvent`, registered with itself as owner, plus the CLR properties, the event and the `RaiseValueChanged` method.

- **Nullability** comes from the type argument: `typeof(PickerMembers<DriveInfo?>)` gives `public DriveInfo? Value` and `RoutedPropertyChangedEventHandler<DriveInfo?>`; `typeof(PickerMembers<int>)` gives `public int Value`.
- A control can use several templates (repeat the attribute) and can declare further attributed fields of its own.
- Anything a template names must exist in each control that uses it: a `PropertyChangedCallback = "OnValueChanged"` needs an `OnValueChanged` method in every control. Write the name as a string: `nameof(OnValueChanged)` does not compile in the template, where the method does not exist. The typed hooks (`GeneratePartialHooks`, `GenerateCoerceHook`) fit templates well, because each control implements only the hooks it needs.
- The same goes for XML documentation: `<see cref="ItemTemplate"/>` in a template field's documentation refers to a member that only exists in the controls, and the compiler reports it in the template (CS1574). Use `<c>ItemTemplate</c>`, or suppress CS1574 for the template class.
- `[UseDependencyPropertyTemplate]` is an instruction to the generator and is not kept in the compiled control, so a public control does not carry a reference to an internal template type.

### Documentation that names what the control is about

XML documentation is copied from the template's fields, so every control gets the same text. Where the text should name the thing the control handles, write placeholders in the template and give each control its words in `DocumentationValues`:

```csharp
[DependencyPropertyTemplate]
internal abstract class PickerMembers<TInfo>
{
    /// <summary>
    /// Gets or sets the selected {0}; <see langword="null"/> when no {0} is selected.
    /// </summary>
    [DependencyProperty]
    private TInfo value;
}

[UseDependencyPropertyTemplate(typeof(PickerMembers<DriveInfo?>), DocumentationValues = ["drive"])]
public partial class DrivePicker : UserControl
{
}
```

`{0}`, `{1}`, ... are replaced by the values in order. Other braces, such as `List{T}` in a `cref`, are left alone.

### Leaving members out

A control that needs only part of a template lists the rest in `Exclude`, by the name of the generated property or event:

```csharp
[UseDependencyPropertyTemplate(typeof(PickerMembers<DriveInfo?>), Exclude = ["Value", "ValueChanged"])]
public partial class LabelDrivePicker : UserControl
{
}
```

## 🎯 Routed events in a generic template

C# does not allow a type parameter in an attribute argument, so `HandlerType = typeof(RoutedPropertyChangedEventHandler<TInfo>)` does not compile inside the template. Name the handler as an unbound generic type instead:

```csharp
[RoutedEvent(HandlerType = typeof(RoutedPropertyChangedEventHandler<>))]
private static readonly RoutedEvent valueChanged;
```

The generator closes it with the type arguments the template is used with. This applies when the handler has as many type parameters as the template.

## ⚠️ Limits

- **Same project only.** The generator reads the template's fields from source. A template in another assembly shows it no fields, because private members are not visible to a referencing project. Share a template between projects by linking the source file into each.
- A type that is not marked `[DependencyPropertyTemplate]`, or that has no attributed fields, is reported as `AtcXamlToolkit0012` and nothing is generated for it.
- The template's fields are unused as far as the compiler can see, like any field the generators read. If your project reports them (CS0169, CS0649, IDE0051), suppress those for the template class. The same goes for an analyzer that warns about a static field in a generic type (Sonar S2743), which a `[RoutedEvent]` field in a generic template is.
- `[RelayCommand]`, `[DefaultStyleKey]` and `[TemplatePartField]` in a template are not applied; only property and routed-event fields are.

## 🧱 Platform support

| Platform | Template members |
|----------|------------------|
| WPF | `[DependencyProperty]`, `[AttachedProperty]`, `[RoutedEvent]` |
| WinUI 3 | `[DependencyProperty]`, `[AttachedProperty]` |
| Avalonia | `[StyledProperty]`, `[AttachedProperty]` |

The attribute names are the same on all three platforms.
