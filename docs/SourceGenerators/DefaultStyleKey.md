# ⚙️ Source Generator for the Default Style Key

A custom control finds its default style by a type key. Telling the framework which key to use is one line of boilerplate per control, and on WPF that line has to live in a static constructor. The `[DefaultStyleKey]` attribute generates it.

| Platform | Supported | Generated code |
|---|---|---|
| WPF | ✅ | A static constructor that calls `DefaultStyleKeyProperty.OverrideMetadata(...)` |
| Avalonia | ✅ | `protected override Type StyleKeyOverride => typeof(...)` |
| WinUI 3 | ❌ | WinUI sets `DefaultStyleKey` in the instance constructor, which generated code cannot add to. The attribute does not exist in the WinUI package. |

## 🚀 Usage

The class must be `partial`.

```csharp
[DefaultStyleKey]
public partial class Flyout : ContentControl
{
}
```

### WPF output

```csharp
public partial class Flyout
{
    static Flyout()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Flyout),
            new FrameworkPropertyMetadata(typeof(Flyout)));
    }
}
```

### Avalonia output

```csharp
public partial class Flyout
{
    /// <inheritdoc />
    protected override global::System.Type StyleKeyOverride => typeof(Flyout);
}
```

## 🎨 Reusing another control's style

Pass the type whose style should be used, for example when a derived control should keep the look of its base:

```csharp
[DefaultStyleKey(typeof(Button))]
public partial class ConfirmButton : Button
{
}
```

## 🧱 A class that already has a static constructor (WPF)

A class can only have one static constructor. When the class declares its own, the generator emits a method for it to call instead:

```csharp
[DefaultStyleKey]
public partial class Flyout : ContentControl
{
    static Flyout()
    {
        OverrideDefaultStyleKey();
        FocusableProperty.OverrideMetadata(typeof(Flyout), new FrameworkPropertyMetadata(false));
    }
}
```

Generated:

```csharp
public partial class Flyout
{
    private static void OverrideDefaultStyleKey()
        => DefaultStyleKeyProperty.OverrideMetadata(
            typeof(Flyout),
            new FrameworkPropertyMetadata(typeof(Flyout)));
}
```

If the static constructor does not call `OverrideDefaultStyleKey()`, the control would be shown without its default style, so the generator reports warning `AtcXamlToolkit0011`.

## 📝 Notes

- `[DefaultStyleKey]` can be combined with `[DependencyProperty]`, `[StyledProperty]`, `[RoutedEvent]` and `[RelayCommand]` on the same class; everything goes into one generated file.
- The attribute alone is enough for the generator to pick the class up; the class-naming rule for the other framework-element attributes does not apply to it.
- The style itself (in `Themes/Generic.xaml` on WPF, or a `ControlTheme` on Avalonia) is still written by hand.
