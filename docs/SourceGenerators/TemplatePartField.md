# ⚙️ Source Generator for Template Parts

A templated control looks up named parts of its template (`PART_CloseButton` and the like) when the template is applied. Each part is normally written four times: a `[TemplatePart]` attribute on the class, a constant for the name, a field, and a lookup in `OnApplyTemplate`. With `[TemplatePartField]` it is written once, on the field.

| Platform | Supported | Class attribute generated | Lookup generated |
|---|---|---|---|
| WPF | ✅ | `System.Windows.TemplatePart` | `GetTemplateChild(name) as T` |
| WinUI 3 | ✅ | `Microsoft.UI.Xaml.TemplatePart` | `GetTemplateChild(name) as T` |
| Avalonia | ✅ | `Avalonia.Controls.Metadata.TemplatePart` | `e.NameScope.Find<T>(name)` |

> The attribute is called `[TemplatePartField]` and not `[TemplatePart]` because all three platforms already have a class-level attribute with that name.

## 🚀 Usage

The class must be `partial`. Mark the fields and call the generated `ResolveTemplateParts` method from `OnApplyTemplate`:

```csharp
public partial class Flyout : ContentControl
{
    [TemplatePartField("PART_Overlay")]
    private Border? overlayBorder;

    [TemplatePartField("PART_CloseButton")]
    private Button? closeButton;

    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        ResolveTemplateParts();

        // Hook events on the parts here.
    }
}
```

### WPF / WinUI output

```csharp
[global::System.Windows.TemplatePart(Name = "PART_Overlay", Type = typeof(Border))]
[global::System.Windows.TemplatePart(Name = "PART_CloseButton", Type = typeof(Button))]
public partial class Flyout
{
    /// <summary>
    /// Assigns the template parts to their fields. Call it from <c>OnApplyTemplate</c>.
    /// </summary>
    private void ResolveTemplateParts()
    {
        overlayBorder = GetTemplateChild("PART_Overlay") as Border;
        closeButton = GetTemplateChild("PART_CloseButton") as Button;
    }
}
```

On WinUI the class attribute is `global::Microsoft.UI.Xaml.TemplatePart`.

### Avalonia

Avalonia hands the template's name scope to `OnApplyTemplate`, so the generated method takes the event args:

```csharp
protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
{
    base.OnApplyTemplate(e);

    ResolveTemplateParts(e);
}
```

Generated:

```csharp
[global::Avalonia.Controls.Metadata.TemplatePart("PART_Overlay", typeof(Border))]
public partial class Flyout
{
    private void ResolveTemplateParts(global::Avalonia.Controls.Primitives.TemplateAppliedEventArgs e)
    {
        overlayBorder = e.NameScope.Find<Border>("PART_Overlay");
    }
}
```

## ❗ Required parts

Set `IsRequired = true` when the control cannot work without the part. The generated method then throws an `InvalidOperationException` that names the part, its type and the control, and the field can be declared non-nullable:

```csharp
[TemplatePartField("PART_CloseButton", IsRequired = true)]
private Button closeButton = null!;
```

```csharp
closeButton = GetTemplateChild("PART_CloseButton") as Button
    ?? throw new global::System.InvalidOperationException("Template part 'PART_CloseButton' of type 'Button' was not found in the template of 'Flyout'.");
```

## 📝 Notes

- **Why a method and not the override.** `OnApplyTemplate` usually does more than look parts up: it unhooks events from the old parts and hooks them on the new ones. The generator therefore emits a method for your override to call, and leaves the override to you.
- **Name constants.** The name can be a constant: `[TemplatePartField(PartCloseButton)]`.
- **Parts already declared on the class.** If the class already has the platform's `[TemplatePart]` attribute for a name, the generator does not declare that part a second time.
- **Optional parts and nullability.** An optional part should be a nullable field (`Border?`). If you declare it non-nullable anyway, the generated assignment keeps that choice and does not add a nullable warning.
- **Static and constant fields** are ignored.
- `[TemplatePartField]` can be combined with `[DefaultStyleKey]`, `[DependencyProperty]`, `[StyledProperty]`, `[RoutedEvent]` and `[RelayCommand]` on the same class; everything goes into one generated file.
