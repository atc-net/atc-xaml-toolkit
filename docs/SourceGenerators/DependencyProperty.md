# ⚙️ DependencyProperties with SourceGeneration

> ✅ This feature is supported in `WPF`, `WinUI 3`, and `Avalonia` ✅

In WPF and WinUI 3, **dependency properties** are a specialized type of property that extends the functionality of standard CLR properties. In Avalonia, the equivalent is **styled properties**. These properties support features such as data binding, animation, and property value inheritance, which are integral to the XAML property system. However, defining these properties traditionally involves verbose boilerplate code. To streamline this process, source generators can automatically generate the necessary code, reducing errors and improving maintainability.

> **Note:** For Avalonia, use the `[StyledProperty]` attribute instead of `[DependencyProperty]`. The source generator will automatically create `StyledProperty<T>` registrations with the appropriate Avalonia-specific code.

> **Note:** `[DependencyProperty]` and `[StyledProperty]` are picked up in any `partial` class, whatever it is called. (Earlier versions required a base class of `UserControl`, `DependencyObject` or `FrameworkElement`, or a class name ending with "Attach", "Behavior" or "Helper".) The generated CLR property calls `GetValue` / `SetValue`, so the class has to derive from `DependencyObject` (on Avalonia: `AvaloniaObject`) for the generated code to compile.

> **Note:** A class that is not `partial` gets no generated code and is reported as `AtcXamlToolkit0002`. An attributed field must be `private` and start with a lower-case letter or an underscore (`isRunning`, `_isRunning`); a field that is not `private`, or that starts with an upper-case letter, is skipped without a diagnostic.

---

## 🚀 Setting Up Your First UserControl

### ✨ Creating a Simple UserControl

You can now define dependency properties in two ways: via a constructor-based attribute or by annotating a backing field. Both approaches trigger the source generator to create the required registration code and property wrappers.

#### Defined by Constructor

In this approach you specify the property name and type in the attribute.

```csharp
[DependencyProperty<bool>("IsRunning")]
public partial class TestView: UserControl
{
    public TestView()
    {
        InitializeComponent();
    }
}
```

#### Defined by Field-Level Attribute

Alternatively, you can mark a private field with `[DependencyProperty]` and let the generator infer the property name (by capitalizing the field name) and type-

```csharp
public partial class TestView : UserControl
{
    public TestView()
    {
        InitializeComponent();
    }

    [DependencyProperty]
    private bool isRunning;
}
```

This field-level approach further reduces redundancy by eliminating the need to explicitly specify the property name and type.

### 🔍 What's Happening Here?

- The `constructor-based` declaration tells the generator to create a dependency property named `IsRunning` of type `bool`.
- The field-level declaration allows the generator to infer the property details from the field itself (e.g., `isRunning` becomes `IsRunning`).
- In both cases, the generator creates:
  - A static `DependencyProperty` field (e.g., `IsRunningProperty`).
  - A CLR property that wraps `GetValue`/`SetValue`.

### 🖥️ XAML Binding Example

```xml
<UserControl xmlns:local="clr-namespace:MyApp.MyUserControl"
    x:Name="UcMyUserControl">
    <StackPanel>

        <atc:LabelToggleSwitch
            IsOn="{Binding ElementName=UcMyUserControl, Path=IsRunning}"
            LabelText="Start / Stop spinner" />

        <atc:BusyOverlay IsBusy="{Binding ElementName=UcMyUserControl, Path=IsRunning}">
            <TextBox Text="Hello world" />
        </atc:BusyOverlay>

    </StackPanel>
</UserControl>
```

This setup allows the UI to dynamically update when the IsRunning property changes.

---

## 🔧 Platform-Specific Considerations

### WPF vs WinUI 3 vs Avalonia

While the source generator provides a unified API across all platforms, there are some differences in the generated code:

**WPF:**
- Supports all `FrameworkPropertyMetadataOptions` flags
- Supports `ValidateValueCallback`, `CoerceValueCallback`, `DefaultUpdateSourceTrigger`, and `IsAnimationProhibited`
- Uses `FrameworkPropertyMetadata` for advanced scenarios
- Uses `BooleanBoxes` for boolean default values to reduce allocations

**WinUI 3:**
- Uses `PropertyMetadata` (simpler than WPF's `FrameworkPropertyMetadata`)
- Does NOT support: `ValidateValueCallback`, `CoerceValueCallback`, `Flags`, `DefaultUpdateSourceTrigger`, or `IsAnimationProhibited`
- The WPF-only options do not exist on the WinUI attribute, so using one is a compile error (CS0246), not something the generator ignores

**Avalonia:**
- Uses `[StyledProperty]` attribute instead of `[DependencyProperty]`
- Generates `StyledProperty<T>` fields instead of `DependencyProperty`
- Uses `Avalonia.AvaloniaProperty.Register<>()` for registration
- Property changed callbacks are registered in a static constructor via `.Changed.AddClassHandler<T>()`
- Does NOT use `BooleanBoxes` (uses plain bool values)
- Has the same options as WinUI (see the options reference below); the WPF-only options do not exist on `[StyledProperty]`, so using one is a compile error (CS0246)
- Control must inherit from `AvaloniaObject` (or `UserControl`, `Control`, etc.)

The source generator automatically detects your platform and generates the appropriate code.

---

## 🧩 Options reference

The options of `[DependencyProperty]` (WPF, WinUI) and `[StyledProperty]` (Avalonia). The class-level forms `[DependencyProperty<T>("Name")]` and `[StyledProperty<T>("Name")]` take the same options, except that the property name is the constructor argument instead of `PropertyName`.

| Option | Type | Default | Platforms | Description |
|--------|------|---------|-----------|-------------|
| `PropertyName` | `string` | field name, first letter in upper case | WPF, WinUI, Avalonia (field form) | Name of the generated property. |
| `DefaultValue` | `object` | default of the property type | WPF, WinUI, Avalonia | Default value as a constant. |
| `DefaultValueExpression` | `string` | none | WPF, WinUI, Avalonia | Default value as C# source text, emitted unchanged. Wins over `DefaultValue`. |
| `PropertyChangedCallback` | `string` | none | WPF, WinUI, Avalonia | Name of a static method that is called when the value changes. |
| `GeneratePartialHooks` | `bool` | `false` | WPF, WinUI, Avalonia | Generate a typed `partial void On{Name}Changed(T oldValue, T newValue)` hook. |
| `AccessModifier` | `PropertyAccessModifier` | `Public` | WPF, WinUI, Avalonia | Accessibility of the `{Name}Property` field and the CLR property. |
| `SetterAccessModifier` | `PropertyAccessModifier` | `Public` (`Private` with `IsReadOnly`) | WPF, WinUI, Avalonia | Accessibility of the CLR property's setter. |
| `GenerateClrAccessors` | `bool` | `true` | WPF, WinUI, Avalonia | `false` generates only the `{Name}Property` field. |
| `Category` | `string` | none | WPF, WinUI, Avalonia | Puts `[Category("...")]` on the CLR property. |
| `Description` | `string` | none | WPF, WinUI, Avalonia | Puts `[Description("...")]` on the CLR property. |
| `GenerateDocumentation` | `bool` | `false`, or `true` with `[assembly: GenerateDocumentationDefault]` | WPF, WinUI, Avalonia | Emit default XML summaries when the field has none. |
| `CoerceValueCallback` | `string` | none | WPF | Name of a static coerce callback. |
| `GenerateCoerceHook` | `bool` | `false` | WPF | Generate a typed `private partial T Coerce{Name}(T value)` method that the class must implement. |
| `ValidateValueCallback` | `string` | none | WPF | Name of a static validation callback. |
| `Flags` | `FrameworkPropertyMetadataOptions` | `None` | WPF | Metadata flags; makes the metadata a `FrameworkPropertyMetadata`. |
| `DefaultUpdateSourceTrigger` | `UpdateSourceTrigger` | `Default` | WPF | Default update trigger for bindings. |
| `IsAnimationProhibited` | `bool` | `false` | WPF | Prohibit animation of the property. |
| `MetadataType` | `DependencyPropertyMetadataType` | `Auto` | WPF | The metadata class that is created. |
| `IsReadOnly` | `bool` | `false` | WPF | Register a read-only dependency property with a `DependencyPropertyKey`. |
| `PropertyKeyAccessModifier` | `PropertyAccessModifier` | private | WPF | Accessibility of the `{Name}PropertyKey` field of a read-only property. |
| `AddOwnerOf` | `Type` | none | WPF | Share the existing property of this type instead of registering a new one. |

The options marked WPF do not exist on the WinUI attribute or on `[StyledProperty]`; using one there does not compile.

## 🏷️ Naming the property with `PropertyName`

The property is named after the field, with the first letter in upper case (`private int count;` gives `Count`). That ties the field's name to the property's, which gets in the way when the class has a local or a parameter of the same name: analyzers report a local `state` that hides a field `state`. `PropertyName` names the property, so the field can be called anything:

```csharp
[DependencyProperty(PropertyName = "State")]
private DataGridFilterState stateValue;
```

This generates `StateProperty` and `public DataGridFilterState State`. The option exists on `[DependencyProperty]`, `[StyledProperty]` and `[AttachedProperty]` on fields, on all three platforms. A value that is not a valid identifier is ignored.

## 🔒 Non-public setters

`SetterAccessModifier` restricts the setter of the generated CLR property, for a value the control computes itself and consumers should only read from code:

```csharp
[DependencyProperty(SetterAccessModifier = PropertyAccessModifier.Private)]
private string selectedStateMessage;
```

Generated (WPF):

```csharp
public string SelectedStateMessage
{
    get => (string)GetValue(SelectedStateMessageProperty);
    private set => SetValue(SelectedStateMessageProperty, value);
}
```

`PropertyAccessModifier` (namespace `Atc.XamlToolkit`) has the values `Public` (default), `Internal`, `Protected`, `ProtectedInternal`, `PrivateProtected` and `Private`. On Avalonia use `[StyledProperty(SetterAccessModifier = ...)]`.

This only hides the C# setter. The property is still an ordinary, writable dependency property: XAML can set it and so can `SetValue(SelectedStateMessageProperty, ...)`. It is not a read-only dependency property with a `DependencyPropertyKey`.

## 🧮 Default values that are expressions

`DefaultValue` is for constants, and the generator fits its text to the property type: a `string` value is quoted, a `Brush` value gets `Brushes.` in front, and so on. A default that is code - a method call, a static member, a constructor - goes in `DefaultValueExpression`, which is emitted exactly as written:

```csharp
[DependencyProperty(DefaultValueExpression = "CreateFrozenBrush(40)")]
private Brush gridBrush;

[DependencyProperty(DefaultValueExpression = "new Thickness(1)")]
private Thickness itemMargin;

[DependencyProperty(DefaultValueExpression = "Array.Empty<object>()")]
private IEnumerable items;
```

```csharp
public static readonly DependencyProperty GridBrushProperty = DependencyProperty.Register(
    nameof(GridBrush),
    typeof(Brush),
    typeof(ZoomGridOverlay),
    new PropertyMetadata(defaultValue: CreateFrozenBrush(40)));
```

- **Do not refer to a static field of the same class.** The generated registration is a static field initialiser in another file of the partial class, and the order in which static fields in different files are initialised is not defined: the registration can run first and capture `null` or `0`. Refer to a constant, a static property or a method instead.
- The expression is evaluated once, in the static field initialiser, so it can only use static members. Names are resolved in the generated file: use a member of the class, a fully qualified name, or a type covered by the project's global usings.
- The value is **shared by every instance** of the control. Use something immutable: a frozen brush, an empty array, a value type. A mutable default such as `new List<string>()` or an unfrozen brush would be one object shared by all controls.
- When both `DefaultValue` and `DefaultValueExpression` are set, the expression is used.
- Works on WPF, WinUI and Avalonia (`[StyledProperty]`), and for `[AttachedProperty]`.

## 🏷️ Which metadata class is generated (WPF)

The generator picks the metadata class from the options that are set:

| Options set | Metadata class |
|-------------|----------------|
| none, or only `DefaultValue` and callbacks | `PropertyMetadata` |
| `Flags`, `DefaultUpdateSourceTrigger` or `IsAnimationProhibited` | `FrameworkPropertyMetadata` |
| `AddOwnerOf` with any override | `FrameworkPropertyMetadata` |

`MetadataType` overrides that choice:

```csharp
[DependencyProperty(MetadataType = DependencyPropertyMetadataType.UIPropertyMetadata, DefaultValue = 0d, PropertyChangedCallback = nameof(OnSpacingChanged))]
private double spacing;
```

```csharp
new UIPropertyMetadata(defaultValue: 0d, propertyChangedCallback: OnSpacingChanged)
```

| `MetadataType` | Result |
|----------------|--------|
| `Auto` (default) | The table above. |
| `PropertyMetadata` | `PropertyMetadata`. |
| `UIPropertyMetadata` | `UIPropertyMetadata`; it can also carry `IsAnimationProhibited`. |
| `FrameworkPropertyMetadata` | `FrameworkPropertyMetadata`, also when no flag is set (the same as `Flags = FrameworkPropertyMetadataOptions.None`). |

`PropertyMetadata` and `UIPropertyMetadata` cannot carry `Flags` or `DefaultUpdateSourceTrigger`, and `PropertyMetadata` cannot carry `IsAnimationProhibited`. When one of those is set, `MetadataType` is ignored, `FrameworkPropertyMetadata` is generated, and the generator reports `AtcXamlToolkit0013`. The option also works on `[AttachedProperty]` and together with `AddOwnerOf`.

The class does not change how the property behaves. It matters to a derived class that calls `OverrideMetadata`: metadata may be overridden with the same class or a more derived one, so a base property with `PropertyMetadata` accepts any override, while one with `FrameworkPropertyMetadata` requires `FrameworkPropertyMetadata`. Keep that in mind when a hand-written property that derived classes override is converted to the attribute.

## 🙈 Non-public properties and properties without a CLR wrapper

`AccessModifier` sets the accessibility of everything generated for the property: the `{Name}Property` field and the CLR property. Use it for a property that is an implementation detail of the control:

```csharp
[DependencyProperty(AccessModifier = PropertyAccessModifier.Private)]
private double internalViewportZoom;
```

```csharp
private static readonly DependencyProperty InternalViewportZoomProperty = DependencyProperty.Register(
    nameof(InternalViewportZoom),
    typeof(double),
    typeof(ZoomBox),
    new PropertyMetadata(defaultValue: 0d));

private double InternalViewportZoom
{
    get => (double)GetValue(InternalViewportZoomProperty);
    set => SetValue(InternalViewportZoomProperty, value);
}
```

- `SetterAccessModifier` can restrict the setter further, for example an `Internal` property with a `Private` setter. C# requires the setter to be more restrictive than the property; when both are the same, the generator leaves the modifier off the setter. A `SetterAccessModifier` that is not more restrictive than `AccessModifier` (a `Private` property with a `Public` setter, or `Protected` with `Internal`, which C# cannot compare) is also left off, and reported as `AtcXamlToolkit0013`.
- In a `sealed` class, `Protected` is written as `private`, for `AccessModifier` and for `SetterAccessModifier`.
- `GenerateClrAccessors = false` generates only the `{Name}Property` field, for a property that is read and written with `GetValue` / `SetValue`. The property is then registered under its name as a string, because there is no CLR property for `nameof`. XML documentation written on the attributed field goes to the `{Name}Property` field, as there is no other member to carry it.
- Both options work on WPF, WinUI and Avalonia (`[StyledProperty]`).
- A non-public property is still registered with the property system under its name; the modifier only limits what C# code can reach.

## 🔐 Read-only dependency properties (WPF)

A value that only the control may set, such as a computed summary or a state flag, should be a read-only dependency property. With `IsReadOnly = true` the generator registers it with `RegisterReadOnly` and writes through a `DependencyPropertyKey`:

```csharp
[DependencyProperty(IsReadOnly = true)]
private string summaryText;
```

Generated:

```csharp
private static readonly DependencyPropertyKey SummaryTextPropertyKey = DependencyProperty.RegisterReadOnly(
    nameof(SummaryText),
    typeof(string),
    typeof(MyControl),
    new PropertyMetadata());

public static readonly DependencyProperty SummaryTextProperty = SummaryTextPropertyKey.DependencyProperty;

public string SummaryText
{
    get => (string)GetValue(SummaryTextProperty);
    private set => SetValue(SummaryTextPropertyKey, value);
}
```

- The key is named `{Name}PropertyKey` and is private.
- The setter is `private` by default. `SetterAccessModifier` changes that, for example `SetterAccessModifier = PropertyAccessModifier.Internal`; an explicit `SetterAccessModifier = PropertyAccessModifier.Public` gives a public setter that writes through the key.
- The key is private. `PropertyKeyAccessModifier = PropertyAccessModifier.Internal` makes it reachable for another class of the assembly that sets the property through the key.
- Declare the field `readonly` to get no setter at all: `[DependencyProperty(IsReadOnly = true)] private readonly string summaryText;`. The class then writes with `SetValue(SummaryTextPropertyKey, value)`.
- Nothing outside the class can set the property: `SetValue(SummaryTextProperty, ...)`, XAML, styles and bindings that write to it all fail. Bindings that read it work as usual.
- `DefaultValue`, the callbacks and the metadata options work as for an ordinary property.
- This is different from `SetterAccessModifier` alone, which only hides the C# setter of an ordinary, writable property.
- **WPF only.** WinUI has no read-only dependency properties, and Avalonia's equivalent is a direct property, which the generator does not produce. The option does not exist on the WinUI attribute or on `[StyledProperty]`.

## 🤝 Sharing an existing property with `AddOwnerOf` (WPF)

A control that should expose a property another class already registered, such as `ComboBox.MaxDropDownHeight`, shares that property instead of registering a new one. Styles, inheritance and triggers written for the original property then also apply to the control:

```csharp
public partial class TokenBox : Control
{
    [DependencyProperty(AddOwnerOf = typeof(ComboBox))]
    private double maxDropDownHeight;
}
```

Generated:

```csharp
public static readonly DependencyProperty MaxDropDownHeightProperty = global::System.Windows.Controls.ComboBox.MaxDropDownHeightProperty.AddOwner(
    typeof(TokenBox));

public double MaxDropDownHeight
{
    get => (double)GetValue(MaxDropDownHeightProperty);
    set => SetValue(MaxDropDownHeightProperty, value);
}
```

- The shared property is `{Type}.{Name}Property`, so the field must have the name and type of the property being shared. The generator writes the type with its full name (`global::...`), so the generated file needs no using for it.
- Without further options no metadata is passed, and the default value and callbacks of the shared property stay as they are. The new owner then gets the metadata it inherits. For an owner that does not derive from the type that registered the property, that is the property's default metadata, whose class may differ from what a hand-written `AddOwner(typeof(X), new FrameworkPropertyMetadata(...))` gave; state `MetadataType = DependencyPropertyMetadataType.FrameworkPropertyMetadata` to pass a `FrameworkPropertyMetadata` that keeps the shared default.
- `DefaultValue`, `PropertyChangedCallback`, `CoerceValueCallback`, `GeneratePartialHooks` and `GenerateCoerceHook` override the metadata for the new owner only:

  ```csharp
  [DependencyProperty(AddOwnerOf = typeof(Path), PropertyChangedCallback = nameof(OnDataChanged))]
  private Geometry data;
  ```

  ```csharp
  public static readonly DependencyProperty DataProperty = global::System.Windows.Shapes.Path.DataProperty.AddOwner(
      typeof(PathIcon),
      new FrameworkPropertyMetadata(propertyChangedCallback: OnDataChanged));
  ```

- The metadata is a `FrameworkPropertyMetadata` unless `MetadataType` says otherwise. WPF requires the metadata type of the shared property or a type derived from it, and `FrameworkPropertyMetadata` fits every built-in property.
- `Flags`, `DefaultUpdateSourceTrigger` and `IsAnimationProhibited` can be set too. The metadata constructors that take them also take a default value; when the attribute states none, the generator passes the default the new owner would inherit (`X.{Name}Property.GetMetadata(typeof(Owner)).DefaultValue`), so the shared default is kept.
- `ValidateValueCallback` does not apply: validation belongs to the registration of the shared property. `IsReadOnly` does not apply either: the property is as writable as the shared one. Setting either is reported as `AtcXamlToolkit0013`.
- `[AttachedProperty(AddOwnerOf = typeof(TextElement))]` works the same way and generates the `Get{Name}` / `Set{Name}` methods for the shared property.
- **WPF only.**

## 🪝 Typed change hooks

A `PropertyChangedCallback` has to be a static method that receives the object and untyped event args, so it usually starts by casting both:

```csharp
[DependencyProperty(PropertyChangedCallback = nameof(OnIsReadOnlyChanged))]
private bool isReadOnly;

private static void OnIsReadOnlyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
{
    if (d is Rating control)
    {
        control.UpdateVisualState();
    }
}
```

With `GeneratePartialHooks = true` the generator writes that static method and calls a typed partial method on the instance instead:

```csharp
[DependencyProperty(GeneratePartialHooks = true)]
private bool isReadOnly;

partial void OnIsReadOnlyChanged(bool oldValue, bool newValue)
    => UpdateVisualState();
```

Generated (WPF and WinUI):

```csharp
private static void OnIsReadOnlyPropertyChangedCallback(
    DependencyObject d,
    DependencyPropertyChangedEventArgs e)
    => ((Rating)d).OnIsReadOnlyChanged((bool)e.OldValue, (bool)e.NewValue);

partial void OnIsReadOnlyChanged(bool oldValue, bool newValue);
```

- The hook is named `On{PropertyName}Changed` and gets the old and the new value in the property's type.
- If you do not implement the partial method, the compiler removes the call, so the option costs nothing when unused.
- On Avalonia use `[StyledProperty(GeneratePartialHooks = true)]`; the hook has the same shape and the generator subscribes it in the static constructor it already generates for callbacks.
- `PropertyChangedCallback = nameof(...)` still works, for example for one callback shared by several properties. When both are set, the named callback runs first and the hook second.
- On WPF and WinUI the generated callback casts the object directly (`((Rating)d)`). That is not the same as a hand-written callback that tests the type with `if (d is Rating control)` or `as` and does nothing for another object: the generated one throws an `InvalidCastException` there. This matters for a property that can be set on other types, for example one that other classes share with `AddOwner` or that inherits its value down the tree. Keep such a callback as a named `PropertyChangedCallback` when it is converted.
- For attached properties see [AttachedProperty.md](AttachedProperty.md#-typed-change-hooks).

## 🎚️ Typed coerce hook (WPF)

`GenerateCoerceHook = true` gives you a typed method that decides the effective value, instead of a static `CoerceValueCallback` that takes and returns `object`:

```csharp
public partial class RangeControl : UserControl
{
    [DependencyProperty(GenerateCoerceHook = true)]
    private double maximum;

    private partial double CoerceMaximum(double value)
        => value < 0 ? 0 : value;
}
```

Generated (besides the registration with `coerceValueCallback: CoerceMaximumPropertyCallback`):

```csharp
private static object CoerceMaximumPropertyCallback(
    DependencyObject d,
    object baseValue)
    => ((RangeControl)d).CoerceMaximum((double)baseValue);

private partial double CoerceMaximum(double value);
```

- The method **must be implemented**. It returns a value, so unlike the change hooks it cannot be left out; forgetting it is a compile error (CS8795).
- It is a separate option, not part of `GeneratePartialHooks`, for that reason.
- If `CoerceValueCallback` is also set, that callback runs first and its result is passed to the typed method.
- **WPF only.** WinUI and Avalonia styled properties registered by the generator have no coerce callback.

## 🗂️ `Category` and `Description` for designers

`Category` and `Description` put `[Category]` and `[Description]` (from `System.ComponentModel`) on the generated CLR property, for property grids and designers:

```csharp
[DependencyProperty(Category = "Layout", Description = "The width of the pane.")]
private double paneWidth;
```

```csharp
[Category("Layout")]
[Description("The width of the pane.")]
public double PaneWidth
{
    get => (double)GetValue(PaneWidthProperty);
    set => SetValue(PaneWidthProperty, value);
}
```

The generated file gets `using System.ComponentModel;`. Both options exist on WPF, WinUI and Avalonia (`[StyledProperty]`), on the field form and the class-level form. For `[AttachedProperty]` the attributes go on the `Get{Name}` and `Set{Name}` methods, see [AttachedProperty.md](AttachedProperty.md).

## 📚 XML documentation generation

When the backing field has XML doc comments, the generator copies them to the generated CLR property, and the `{Name}Property` field gets an "Identifies the ... dependency property." summary:

```csharp
/// <summary>
/// The width of the pane when it is open.
/// </summary>
[DependencyProperty(DefaultValue = 240d)]
private double openPaneLength;
```

generates:

```csharp
/// <summary>
/// Identifies the <see cref="OpenPaneLength"/> dependency property.
/// </summary>
public static readonly DependencyProperty OpenPaneLengthProperty = DependencyProperty.Register(
    ...);

/// <summary>
/// The width of the pane when it is open.
/// </summary>
public double OpenPaneLength
{
    ...
}
```

The same applies to `[StyledProperty]` on Avalonia ("Identifies the ... styled property."). On a static owner class, the docs go on the generated `Get{Name}` / `Set{Name}` methods.

For fields **without** docs, and for the class-level form (which has no field), the generator can emit a default summary instead. Since framework-element APIs are typically shipped as public NuGet packages, the generator can emit a default `/// <summary>` on the generated CLR property. The behaviour is **opt-in** so existing snapshots stay byte-identical:

```csharp
[DependencyProperty(GenerateDocumentation = true)]
private bool isRunning;
```

generates:

```csharp
/// <summary>
/// Identifies the <see cref="IsRunning"/> dependency property.
/// </summary>
public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register(
    ...);

/// <summary>
/// Gets or sets the IsRunning.
/// </summary>
public bool IsRunning
{
    get => (bool)GetValue(IsRunningProperty);
    set => SetValue(IsRunningProperty, Atc.XamlToolkit.BooleanBoxes.Box(value));
}
```

The same flag works on the class-level form:

```csharp
[DependencyProperty<bool>("IsRunning", GenerateDocumentation = true)]
public partial class MyControl : UserControl
{
}
```

To enable defaults across the assembly without writing the flag on every attribute, opt in once:

```csharp
[assembly: GenerateDocumentationDefault]
```

After this, plain `[DependencyProperty]` (and `[ObservableProperty]`, `[RelayCommand]`, `[AttachedProperty]`, `[StyledProperty]`) emit the default summary. Per-attribute `GenerateDocumentation = false` still wins as an explicit opt-out.

Docs on the field always take precedence: `GenerateDocumentation` is a *fallback*, not a replacement.

---

## 📌 Summary

This example demonstrates how to use **advanced metadata** with dependency properties via source generators, allowing:

- ✔️ **Automatic property change notifications**

- ✔️ **Value coercion and validation**

- ✔️ **Optimized UI performance with layout invalidation**

- ✔️ **Flexible data binding behavior**

- ✔️ **Control over animation support**

### 🚀 Why Use Atc.XamlToolkit’s Source Generators?

- ✅ **Eliminates boilerplate** – Just declare the property, and the generator handles the rest.

- ✅ **Ensures consistency** – Less room for human error.

- ✅ **Improves Maintainability:** Reduces the likelihood of errors with auto-generated boilerplate.

- ✅ **Streamlines Development:** Focus more on business logic rather than repetitive code patterns.

---

## 🔎 Behind the scenes

### 📝 Human-Written Code - for simple example

#### Constructor-Based Declaration

```csharp
[DependencyProperty<bool>("IsRunning")]
public partial class MyControl : UserControl
{
}
```

#### Field-Level Declaration

```csharp
public partial class MyControl : UserControl
{
    [DependencyProperty]
    private bool isRunning;
}
```

**In these example:**

- The attribute signals the source generator to create a dependency property named `IsRunning` (inferred from the attribute parameter or the field name).
- The generator automatically creates the corresponding `DependencyProperty` field and CLR property wrapper.

### ⚙️ Auto-Generated Code - for simple example

The source generator will produce code equivalent to:

```csharp
public partial class MyControl
{
    public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register(
        nameof(IsRunning),
        typeof(bool),
        typeof(MyControl),
        new PropertyMetadata(defaultValue: Atc.XamlToolkit.BooleanBoxes.FalseBox));

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, Atc.XamlToolkit.BooleanBoxes.Box(value));
    }
}
```

### 📝 Human-Written Code - for complex example

```csharp

[DependencyProperty<bool>(
    "IsRunning",
    DefaultValue = false,
    PropertyChangedCallback = nameof(PropertyChangedCallback),
    CoerceValueCallback = nameof(CoerceValueCallback),
    Flags = FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
    DefaultUpdateSourceTrigger = UpdateSourceTrigger.Default,
    IsAnimationProhibited = true,
    ValidateValueCallback = nameof(ValidateValueCallback))]
public partial class MyControl : UserControl
{
    private static void PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        throw new NotImplementedException();
    }

    private static object CoerceValueCallback(DependencyObject d, object baseValue)
    {
        throw new NotImplementedException();
    }

    private static bool ValidateValueCallback(object value)
    {
        throw new NotImplementedException();
    }
}
```

**In this example:**

- `[DependencyProperty<bool>("IsRunning")]`
  - Declares a **dependency property** named `IsRunning` of type `bool` for `MyControl`.
  - The source generator will automatically create a `DependencyProperty` field and a CLR property wrapper.

- `DefaultValue = false`
  - Specifies the default value of `IsRunning` as `false`.
  - This means that when an instance of `MyControl` is created, `IsRunning` will be `false` unless explicitly set.

- `PropertyChangedCallback = nameof(PropertyChangedCallback)`
  - Assigns a **property changed callback method**, which is invoked whenever the property's value changes.
  - In this example, `PropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)` is defined as a static method.
  - This method allows you to **respond to property changes**, such as triggering UI updates or executing business logic.

- `CoerceValueCallback = nameof(CoerceValueCallback)`
  - Assigns a **coerce value callback method**, which is called before setting the property’s value.
  - This function allows validation, restricting the range of acceptable values, or adjusting the value based on other conditions.
  - For instance, if `IsRunning` should never be `true` under specific circumstances, the `CoerceValueCallback` could enforce that rule.

- `Flags = FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender`
  - Configures how the property affects the **WPF layout system** when it changes.
  - `AffectsMeasure`: Triggers a re-measure of the UI element if this property changes.
  - `AffectsRender`: Causes a re-render of the control when this property is modified.
  - These options ensure that any UI elements depending on `IsRunning` will **recalculate their size and appearance** accordingly.

- `FrameworkPropertyMetadataOptions`
  - A flag-based enumeration used to specify additional **property behavior** in WPF.
  - Other possible values include:
    - `AffectsParentMeasure`: Causes a layout pass on the parent when the property changes.
    - `AffectsArrange`: Forces an arrange pass when the property changes.
    - `Inherits`: Allows the property value to propagate down the visual tree.
    - `BindsTwoWayByDefault`: Sets the default binding mode to **TwoWay**.
  - These flags **optimize performance** by ensuring layout changes only occur when necessary.

- `DefaultUpdateSourceTrigger = UpdateSourceTrigger.Default`
  - Specifies how **data binding** updates the property's source.
  - The `Default` value means that the **default behavior of the property type** is used.
  - Other possible values:
    - `PropertyChanged`: Updates the source immediately when the property changes.
    - `LostFocus`: Updates the source when the control loses focus (e.g., leaving a text box).
    - `Explicit`: Requires manual invocation of `BindingExpression.UpdateSource()`.

- `IsAnimationProhibited = true`
  - Prevents animations from affecting this property.
  - Some dependency properties allow animations to change their values smoothly over time.
  - By setting `IsAnimationProhibited = true`, you ensure that **no animations** can modify `IsRunning`.
  - This is useful for properties where **instant updates are required**, such as boolean state changes.

- `ValidateValueCallback = nameof(ValidateValueCallback)`
  - Assigns a **validation callback method**, which ensures that only valid values are assigned to the dependency property.
  - This function **executes before** the property value is set, allowing you to **reject invalid values** before they are applied.
  - The `ValidateValueCallback` method should return a `bool`:
    - `true`: The value is accepted and applied to the property.
    - `false`: The value is considered invalid, and an exception is thrown.

---

### ⚙️ Auto-Generated Code - for complex example

The source generator will produce code equivalent to:

```csharp
public partial class MyControl
{
    public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register(
        nameof(IsRunning),
        typeof(bool),
        typeof(MyControl),
        new FrameworkPropertyMetadata(
            defaultValue: Atc.XamlToolkit.BooleanBoxes.FalseBox,
            propertyChangedCallback: PropertyChangedCallback,
            coerceValueCallback: CoerceValueCallback,
            flags: FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
            defaultUpdateSourceTrigger: UpdateSourceTrigger.Default,
            isAnimationProhibited: true),
        validateValueCallback: ValidateValueCallback);

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, Atc.XamlToolkit.BooleanBoxes.Box(value));
    }
}
```

---

## 🌐 Avalonia StyledProperty Example

### 📝 Human-Written Code - Avalonia

For Avalonia, use the `[StyledProperty]` attribute with field-level declaration:

> **Tip:** We recommend moving toolkit usings (e.g. `using Atc.XamlToolkit.Controls.Attributes;`) into your `GlobalUsings.cs` file so they don't need to be repeated in every file.

```csharp
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Atc.XamlToolkit.Controls.Attributes;

namespace MyApp.Controls;

public partial class ColorPickerControl : UserControl
{
    public ColorPickerControl()
    {
        InitializeComponent();
    }

    [StyledProperty(DefaultValue = "Colors.Blue", PropertyChangedCallback = nameof(OnColorChanged))]
    private Color selectedColor;

    [StyledProperty(PropertyChangedCallback = nameof(OnColorComponentChanged))]
    private byte red;

    [StyledProperty(PropertyChangedCallback = nameof(OnColorComponentChanged))]
    private byte green;

    [StyledProperty(PropertyChangedCallback = nameof(OnColorComponentChanged))]
    private byte blue;

    private static void OnColorChanged(
        AvaloniaObject d,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not ColorPickerControl control)
        {
            return;
        }

        var color = (Color)e.NewValue;
        control.Red = color.R;
        control.Green = color.G;
        control.Blue = color.B;
    }

    private static void OnColorComponentChanged(
        AvaloniaObject d,
        AvaloniaPropertyChangedEventArgs e)
    {
        if (d is not ColorPickerControl control)
        {
            return;
        }

        var newColor = Color.FromRgb(control.Red, control.Green, control.Blue);
        if (control.SelectedColor != newColor)
        {
            control.SelectedColor = newColor;
        }
    }
}
```

### ⚙️ Auto-Generated Code - Avalonia

The source generator will produce code equivalent to:

```csharp
public partial class ColorPickerControl
{
    public static readonly StyledProperty<Color> SelectedColorProperty = Avalonia.AvaloniaProperty.Register<ColorPickerControl, Color>(
        nameof(SelectedColor),
        defaultValue: Colors.Blue);

    public Color SelectedColor
    {
        get => (Color)GetValue(SelectedColorProperty)!;
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly StyledProperty<byte> RedProperty = Avalonia.AvaloniaProperty.Register<ColorPickerControl, byte>(
        nameof(Red),
        defaultValue: (byte)0);

    public byte Red
    {
        get => (byte)GetValue(RedProperty)!;
        set => SetValue(RedProperty, value);
    }

    public static readonly StyledProperty<byte> GreenProperty = Avalonia.AvaloniaProperty.Register<ColorPickerControl, byte>(
        nameof(Green),
        defaultValue: (byte)0);

    public byte Green
    {
        get => (byte)GetValue(GreenProperty)!;
        set => SetValue(GreenProperty, value);
    }

    public static readonly StyledProperty<byte> BlueProperty = Avalonia.AvaloniaProperty.Register<ColorPickerControl, byte>(
        nameof(Blue),
        defaultValue: (byte)0);

    public byte Blue
    {
        get => (byte)GetValue(BlueProperty)!;
        set => SetValue(BlueProperty, value);
    }

    static ColorPickerControl()
    {
        SelectedColorProperty.Changed.AddClassHandler<ColorPickerControl>(OnColorChanged);
        RedProperty.Changed.AddClassHandler<ColorPickerControl>(OnColorComponentChanged);
        GreenProperty.Changed.AddClassHandler<ColorPickerControl>(OnColorComponentChanged);
        BlueProperty.Changed.AddClassHandler<ColorPickerControl>(OnColorComponentChanged);
    }
}
```

**Key Differences in Avalonia:**

- Uses `StyledProperty<T>` instead of `DependencyProperty`
- Registration uses `Avalonia.AvaloniaProperty.Register<TOwner, TValue>()`
- No `BooleanBoxes` - plain bool values are used
- Property changed callbacks registered in static constructor via `.Changed.AddClassHandler<T>()`
- Simpler metadata system - no flags, coercion or validation: those options do not exist on `[StyledProperty]` (see the options reference above for what does)
- The getter ends with the null-forgiving operator (`GetValue(...)!`)
