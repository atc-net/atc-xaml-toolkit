# ⚙️ AttachedProperty with SourceGeneration

> ✅ This feature is supported in `WPF`, `WinUI 3`, and `Avalonia` ✅

In WPF, WinUI 3, and Avalonia, **attached properties** are a type of dependency/styled property that allows properties to be defined in one class but used in another. They are widely used in scenarios like behaviors, layout configurations, and interactions where a property needs to be applied to multiple elements without modifying their class definitions. Traditionally, defining attached properties requires boilerplate code, but source generators can automate this process, reducing errors and improving maintainability.

> **Note:** `[AttachedProperty]` works in any `partial` class, whatever it is called. (Earlier versions required a class name ending with "Attach", "Behavior" or "Helper", or a base class of `UserControl`, `DependencyObject` or `FrameworkElement`.)

> **Note:** A class that is not `partial` gets no generated code and is reported as `AtcXamlToolkit0002`. An attributed field must be `private` and start with a lower-case letter or an underscore; a field that is not `private`, or that starts with an upper-case letter, is skipped without a diagnostic.

---

## 🚀 Defining an Attached Property

### ✨ Creating a Simple Attached Property

You can now define an attached property using one of two approaches: via a constructor-based attribute or by annotating a backing field.

#### Defined by constructor

In this approach, you pass the property name and type as parameters to the attribute. The source generator then creates the corresponding property registration and accessor methods.

```csharp
[AttachedProperty<bool>("IsDraggable")]
public static partial class DragBehavior
{
}
```

#### Defined by Field-Level Attribute

Alternatively, you can declare the attached property on a private field. Here, you only annotate the field with `[AttachedProperty]` and the source generator will infer the property name from the field name (e.g., `isDraggable` becomes `IsDraggable`) as well as its type.

```csharp
public static partial class DragBehavior
{
    [AttachedProperty]
    private static bool isDraggable;
}
```

This method reduces redundancy by eliminating the need to specify the property name and type explicitly—streamlining your code even further.

### 🔍 What's Happening Here?

- The `[AttachedProperty]` attribute triggers the source generator to automatically create:
  - A static `DependencyProperty` field (e.g., `IsDraggableProperty`).
  - Static `GetIsDraggable` and `SetIsDraggable` methods, which allow any UI element to use the property.

### 🖥️ XAML Example

```xml
<UserControl xmlns:local="clr-namespace:MyApp.MyUserControl"
    x:Name="UcMyUserControl">
    <Grid local:DragBehavior.IsDraggable="True">
        <TextBlock Text="Drag Me!" />
    </Grid>
</UserControl>
```

This allows the `IsDraggable` property to be applied to any UI element dynamically.

---

## 🔧 Platform-Specific Considerations

### WPF vs WinUI 3 vs Avalonia

While the source generator provides a unified API for all platforms, there are some differences in the generated code:

**WPF:**
- Supports all `FrameworkPropertyMetadataOptions` flags
- Supports `ValidateValueCallback`, `CoerceValueCallback`, `DefaultUpdateSourceTrigger`, and `IsAnimationProhibited`
- Uses `FrameworkPropertyMetadata` for advanced scenarios
- Uses `BooleanBoxes` for boolean default values (performance optimization)
- Generated Get/Set methods use `DependencyObject` as the parameter type

**WinUI 3:**
- Uses `PropertyMetadata` (simpler than WPF's `FrameworkPropertyMetadata`)
- Does NOT support: `ValidateValueCallback`, `CoerceValueCallback`, `Flags`, `DefaultUpdateSourceTrigger`, or `IsAnimationProhibited`
- Uses `BooleanBoxes` for boolean default values (performance optimization)
- Generated Get/Set methods use `DependencyObject` as the parameter type
- The WPF-only options do not exist on the WinUI attribute, so using one is a compile error (CS0246), not something the generator ignores

**Avalonia:**
- Uses generic type parameters in property registration (e.g., `RegisterAttached<TOwner, THost, TValue>`)
- Uses plain `bool` values instead of `BooleanBoxes`
- Generated Get/Set methods use `AvaloniaObject` as the parameter type
- **Important:** Owner class must inherit from `AvaloniaObject` (cannot be static)
- Does NOT support WPF-specific features like `ValidateValueCallback`, `CoerceValueCallback`, or metadata flags: those options do not exist on the Avalonia attribute, so using one is a compile error (CS0246)

The source generator automatically detects your platform and generates the appropriate code.

---

## 🧩 Options reference

The options of `[AttachedProperty]`. The class-level form `[AttachedProperty<T>("Name")]` takes the same options, except that the property name is the constructor argument instead of `PropertyName`.

| Option | Type | Default | Platforms | Description |
|--------|------|---------|-----------|-------------|
| `PropertyName` | `string` | field name, first letter in upper case | WPF, WinUI, Avalonia (field form) | Name of the generated property. |
| `DefaultValue` | `object` | default of the property type | WPF, WinUI, Avalonia | Default value as a constant. |
| `DefaultValueExpression` | `string` | none | WPF, WinUI, Avalonia | Default value as C# source text, emitted unchanged. Wins over `DefaultValue`. |
| `PropertyChangedCallback` | `string` | none | WPF, WinUI, Avalonia | Name of a static method that is called when the value changes. |
| `GeneratePartialHooks` | `bool` | `false` | WPF, WinUI, Avalonia | Generate a typed `static partial void On{Name}Changed(d, T oldValue, T newValue)` hook. |
| `AccessModifier` | `PropertyAccessModifier` | `Public` | WPF, WinUI, Avalonia | Accessibility of the `{Name}Property` field and the `Get{Name}` / `Set{Name}` methods. |
| `SetterAccessModifier` | `PropertyAccessModifier` | `Public` (`Private` with `IsReadOnly`) | WPF, WinUI, Avalonia | Accessibility of the `Set{Name}` method. |
| `GenerateClrAccessors` | `bool` | `true` | WPF, WinUI, Avalonia | `false` generates only the `{Name}Property` field. |
| `Category` | `string` | none | WPF, WinUI, Avalonia | Puts `[Category("...")]` on `Get{Name}` and `Set{Name}`. |
| `Description` | `string` | none | WPF, WinUI, Avalonia | Puts `[Description("Get: ...")]` on `Get{Name}` and `[Description("Set: ...")]` on `Set{Name}`. |
| `GenerateDocumentation` | `bool` | `false`, or `true` with `[assembly: GenerateDocumentationDefault]` | WPF, WinUI, Avalonia | Emit default XML summaries when the field has none. |
| `TargetType` | `Type` | `DependencyObject` | WPF, WinUI | Type of the element parameter of `Get{Name}` / `Set{Name}`. |
| `NullElementHandling` | `NullElementHandling` | `Default` | WPF, WinUI | What the accessors do with a `null` element. |
| `ElementParameterName` | `string` | `element` | WPF, WinUI | Name of the element parameter of `Get{Name}` / `Set{Name}`. |
| `BrowsableForTargetType` | `bool` | `false` | WPF | Put `[AttachedPropertyBrowsableForType(typeof(TargetType))]` on `Get{Name}`. |
| `CoerceValueCallback` | `string` | none | WPF | Name of a static coerce callback. |
| `GenerateCoerceHook` | `bool` | `false` | WPF | Generate a typed `private static partial T Coerce{Name}(DependencyObject d, T value)` method that the class must implement. |
| `ValidateValueCallback` | `string` | none | WPF | Name of a static validation callback. |
| `Flags` | `FrameworkPropertyMetadataOptions` | `None` | WPF | Metadata flags; makes the metadata a `FrameworkPropertyMetadata`. |
| `DefaultUpdateSourceTrigger` | `UpdateSourceTrigger` | `Default` | WPF | Default update trigger for bindings. |
| `IsAnimationProhibited` | `bool` | `false` | WPF | Prohibit animation of the property. |
| `MetadataType` | `DependencyPropertyMetadataType` | `Auto` | WPF | The metadata class that is created, see "Which metadata class is generated" in [DependencyProperty.md](DependencyProperty.md). |
| `IsReadOnly` | `bool` | `false` | WPF | Register a read-only attached property with a `DependencyPropertyKey`. |
| `PropertyKeyAccessModifier` | `PropertyAccessModifier` | private | WPF | Accessibility of the `{Name}PropertyKey` field of a read-only property. |
| `AddOwnerOf` | `Type` | none | WPF | Share the existing property of this type instead of registering a new one. |

An option does not exist on the attribute of a platform that is not listed for it; using it there does not compile.

`Category` and `Description` are for property grids and designers. The generated file gets `using System.ComponentModel;`:

```csharp
[AttachedProperty(Category = "Layout", Description = "The gap.")]
private static double gap;
```

```csharp
[Category("Layout")]
[Description("Get: The gap.")]
public static double GetGap(DependencyObject element)
    => (double)element.GetValue(GapProperty);

[Category("Layout")]
[Description("Set: The gap.")]
public static void SetGap(DependencyObject element, double value)
    => element?.SetValue(GapProperty, value);
```

## 📝 Documentation of the accessors

XML documentation on the field is copied to both generated methods. Write it the way a property is documented, starting with "Gets or sets": the generator then writes "Gets ..." on `Get{Name}` and "Sets ..." on `Set{Name}`. Any other text is used for both methods as it stands.

```csharp
/// <summary>
/// Gets or sets the grow factor of the element inside its panel.
/// </summary>
[AttachedProperty]
private static double grow;
```

`<param>` and `<returns>` tags cannot be written on a field (the compiler reports them), so the generated methods have a summary only.

With `GenerateClrAccessors = false` there are no methods; the documentation of the field then goes to the `{Name}Property` field.

## 🎯 Typing the accessors with `TargetType` (WPF, WinUI)

By default the generated `Get{Name}` and `Set{Name}` methods take a `DependencyObject`. `TargetType` narrows the parameter to the kind of element the property is meant for:

```csharp
public static partial class CheckBoxHelper
{
    [AttachedProperty(TargetType = typeof(UIElement))]
    private static CornerRadius checkCornerRadius;
}
```

```csharp
public static CornerRadius GetCheckCornerRadius(global::System.Windows.UIElement element)
    => (CornerRadius)element.GetValue(CheckCornerRadiusProperty);

public static void SetCheckCornerRadius(global::System.Windows.UIElement element, CornerRadius value)
    => element?.SetValue(CheckCornerRadiusProperty, value);
```

The type is written with its full name (on WinUI `global::Microsoft.UI.Xaml.UIElement`). This only changes the signature of the two methods. The property can still be set on any object from XAML, and the change hooks still receive a `DependencyObject`.

On WPF, add `BrowsableForTargetType = true` to also put `[AttachedPropertyBrowsableForType(typeof(...))]` with the target type on the `Get{Name}` method, so designers offer the property only on elements of that type:

```csharp
[AttachedProperty(TargetType = typeof(CheckBox), BrowsableForTargetType = true)]
private static double checkSize;
```

```csharp
[AttachedPropertyBrowsableForType(typeof(global::System.Windows.Controls.CheckBox))]
public static double GetCheckSize(global::System.Windows.Controls.CheckBox element)
    => (double)element.GetValue(CheckSizeProperty);
```

`BrowsableForTargetType` without `TargetType` has no effect, and is not reported.

## 🚫 What the accessors do with a `null` element (WPF, WinUI)

`NullElementHandling` decides what the generated `Get{Name}` and `Set{Name}` methods do when they are called with `null`:

| Value | `Get{Name}(null)` | `Set{Name}(null, value)` |
|-------|-------------------|--------------------------|
| `Default` | throws `NullReferenceException` | does nothing |
| `ThrowArgumentNullException` | throws `ArgumentNullException` for `element` | throws `ArgumentNullException` for `element` |
| `None` | throws `NullReferenceException` | throws `NullReferenceException` |

```csharp
[AttachedProperty(NullElementHandling = NullElementHandling.ThrowArgumentNullException)]
private static double spacing;
```

```csharp
public static double GetSpacing(DependencyObject element)
{
    global::System.ArgumentNullException.ThrowIfNull(element);
    return (double)element.GetValue(SpacingProperty);
}

public static void SetSpacing(DependencyObject element, double value)
{
    global::System.ArgumentNullException.ThrowIfNull(element);
    element.SetValue(SpacingProperty, value);
}
```

`ElementParameterName` renames the element parameter of both methods (the default is `element`), for example `ElementParameterName = "obj"` gives `GetSpacing(DependencyObject obj)`. It matters when existing callers use named arguments, and it is the name an `ArgumentNullException` reports. A name that is not a valid identifier, or `value` (the name of the second parameter of `Set{Name}`), is not used: the parameter is then called `element`, without a diagnostic.

`ThrowArgumentNullException` is the behaviour to prefer in a public API. `None` exists so a hand-written accessor that uses the element directly can be converted without changing what it does. `NullElementHandling` is in the `Atc.XamlToolkit` namespace.

## 🙈 Non-public attached properties and per-element state

`AccessModifier` sets the accessibility of the `{Name}Property` field and of the `Get{Name}` / `Set{Name}` methods; `SetterAccessModifier` can give the `Set{Name}` method a different one. `GenerateClrAccessors = false` generates only the field. Together they cover an attached property that a class uses to keep its own state on an element:

```csharp
public static partial class DataGridFilter
{
    [AttachedProperty(AccessModifier = PropertyAccessModifier.Private, GenerateClrAccessors = false)]
    private static DataGridFilterState state;
}
```

```csharp
private static readonly DependencyProperty StateProperty = DependencyProperty.RegisterAttached(
    "State",
    typeof(DataGridFilterState),
    typeof(DataGridFilter));
```

Both options work on WPF, WinUI and Avalonia.

## 🔐 Read-only attached properties (WPF)

`[AttachedProperty(IsReadOnly = true)]` registers the property with `RegisterAttachedReadOnly`. The generator emits a private `{Name}PropertyKey`, the public `{Name}Property` taken from the key, a public `Get{Name}` method, and a `Set{Name}` method that writes through the key and is `private` unless `SetterAccessModifier` says otherwise:

```csharp
public static partial class DataGridFilter
{
    [AttachedProperty(IsReadOnly = true)]
    private static bool isFiltered;

    internal static void MarkAsFiltered(DependencyObject element)
        => SetIsFiltered(element, true);
}
```

`SetterAccessModifier` can also be used without `IsReadOnly`, to make the `Set{Name}` method of an ordinary attached property non-public.

## 🪝 Typed change hooks

`GeneratePartialHooks = true` generates the static property-changed callback and calls a typed partial method. An attached property has no owner instance, so the hook is static and receives the object the property is attached to:

```csharp
public static partial class DragBehavior
{
    [AttachedProperty(GeneratePartialHooks = true)]
    private static bool isDraggable;

    static partial void OnIsDraggableChanged(DependencyObject d, bool oldValue, bool newValue)
    {
        if (d is UIElement element)
        {
            // attach or detach the drag handlers
        }
    }
}
```

Generated (WPF and WinUI):

```csharp
private static void OnIsDraggablePropertyChangedCallback(
    DependencyObject d,
    DependencyPropertyChangedEventArgs e)
    => OnIsDraggableChanged(d, (bool)e.OldValue, (bool)e.NewValue);

static partial void OnIsDraggableChanged(DependencyObject d, bool oldValue, bool newValue);
```

On Avalonia the first parameter is an `AvaloniaObject`. `PropertyChangedCallback = nameof(...)` can still be used, alone or together with the hook (the named callback then runs first). The hook for ordinary dependency properties is described in [DependencyProperty.md](DependencyProperty.md#-typed-change-hooks).

## 🎚️ Typed coerce hook (WPF)

`[AttachedProperty(GenerateCoerceHook = true)]` generates the static coerce callback and a typed method that also receives the object the property is set on. The class must implement it:

```csharp
public static partial class ZoomBehavior
{
    [AttachedProperty(GenerateCoerceHook = true)]
    private static double zoomFactor;

    private static partial double CoerceZoomFactor(DependencyObject d, double value)
        => value > 4 ? 4 : value;
}
```

If `CoerceValueCallback` is also set, that callback runs first and its result is passed to the typed method.

## 📚 XML documentation generation

When the backing field has XML doc comments, the generator copies them to the generated `Get{Name}` / `Set{Name}` methods, and the `{Name}Property` field gets an "Identifies the {Name} attached property." summary:

```csharp
public static partial class DragBehavior
{
    /// <summary>
    /// Allows the element to be dragged.
    /// </summary>
    [AttachedProperty]
    private static bool isDraggable;
}
```

For fields without docs, attached property accessors are public APIs that ship to consumers, so the generator can emit a default `/// <summary>` on the static `Get{Name}` / `Set{Name}` methods. The behaviour is **opt-in** to keep existing snapshots byte-identical:

```csharp
public static partial class DragBehavior
{
    [AttachedProperty(GenerateDocumentation = true)]
    private static bool isDraggable;
}
```

generates:

```csharp
/// <summary>
/// Identifies the IsDraggable attached property.
/// </summary>
public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.RegisterAttached(
    ...);

/// <summary>
/// Gets the IsDraggable attached property value.
/// </summary>
public static bool GetIsDraggable(DependencyObject element)
    => (bool)element.GetValue(IsDraggableProperty);

/// <summary>
/// Sets the IsDraggable attached property value.
/// </summary>
public static void SetIsDraggable(DependencyObject element, bool value)
    => element?.SetValue(IsDraggableProperty, Atc.XamlToolkit.BooleanBoxes.Box(value));
```

The same flag works on the class-level generic form (`[AttachedProperty<T>("Name", GenerateDocumentation = true)]`).

To flip the default across the entire assembly, apply once:

```csharp
[assembly: GenerateDocumentationDefault]
```

Plain `[AttachedProperty]` (and `[DependencyProperty]`, `[StyledProperty]`, `[ObservableProperty]`, `[RelayCommand]`) then emit the default summary by default. Per-attribute `GenerateDocumentation = false` still wins as an explicit opt-out.

Docs on the field always take precedence: `GenerateDocumentation` is a *fallback*, not a replacement.

---

## 📌 Summary

This example demonstrates **advanced metadata** handling for attached properties with source generation, enabling:

- ✔️ **Automatic property registration**

- ✔️ **Flexible application to various UI elements**

- ✔️ **Custom property value coercion and validation**

- ✔️ **Efficient UI updates**

- ✔️ **Simplified code structure**

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
[AttachedProperty<bool>("IsDraggable")]
public static partial class DragBehavior
{
}
```

#### Field-Level Declaration

```csharp
public static partial class DragBehavior
{
    [AttachedProperty]
    private static bool isDraggable;
}
```

**In this example:**

- The attribute declares an attached property named `IsDraggable` of type `bool` for the `DragBehavior` class.

- In the field-level declaration, the generator infers the property name (capitalizing the field name) and type from the field itself.

- The source generator then creates the necessary registration code and accessor methods.

### ⚙️ Auto-Generated Code - for simple example (WPF / WinUI)

The source generator will produce code equivalent to:

```csharp
public static partial class DragBehavior
{
    public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.RegisterAttached(
        "IsDraggable",
        typeof(bool),
        typeof(DragBehavior),
        new PropertyMetadata(defaultValue: Atc.XamlToolkit.BooleanBoxes.FalseBox));

    public static bool GetIsDraggable(DependencyObject element)
        => (bool)element.GetValue(IsDraggableProperty);

    public static void SetIsDraggable(DependencyObject element, bool value)
        => element?.SetValue(IsDraggableProperty, Atc.XamlToolkit.BooleanBoxes.Box(value));
}
```

On WinUI the field and the registration call are written with the full type name: `public static readonly Microsoft.UI.Xaml.DependencyProperty IsDraggableProperty = Microsoft.UI.Xaml.DependencyProperty.RegisterAttached(`. The rest is the same.

### ⚙️ Auto-Generated Code - for simple example (Avalonia)

Avalonia uses a different registration API. The owner must inherit from `AvaloniaObject` and **cannot be static**, so the source declaration changes shape:

```csharp
[AttachedProperty<bool>("IsDraggable")]
public partial class DragBehavior : AvaloniaObject
{
}
```

The generator emits:

```csharp
public partial class DragBehavior : AvaloniaObject
{
    public static readonly Avalonia.AvaloniaProperty IsDraggableProperty =
        Avalonia.AvaloniaProperty.RegisterAttached<DragBehavior, AvaloniaObject, bool>(
            "IsDraggable",
            defaultValue: false);

    public static bool GetIsDraggable(AvaloniaObject element)
        => (bool)element.GetValue(IsDraggableProperty)!;

    public static void SetIsDraggable(AvaloniaObject element, bool value)
        => element?.SetValue(IsDraggableProperty, value);
}
```

**Key differences from the WPF/WinUI shape:**

- `RegisterAttached<TOwner, THost, TValue>` uses **generic type parameters** instead of `typeof(...)` arguments. The middle type parameter (`AvaloniaObject`) is the host element type — what the property can be applied to.
- **No `BooleanBoxes`** — Avalonia stores plain `bool` values; the `Get`/`Set` methods pass them through unboxed.
- **Get/Set methods take `AvaloniaObject`**, not `DependencyObject`.
- The `Get` accessor uses the **null-forgiving operator** (`element.GetValue(...)!`), for value types and reference types alike, because `AvaloniaObject.GetValue` returns `object?`.

### 📝 Human-Written Code - for complex example

```csharp
[AttachedProperty<bool>(
    "IsDraggable",
    DefaultValue = false,
    PropertyChangedCallback = nameof(PropertyChangedCallback),
    CoerceValueCallback = nameof(CoerceValueCallback),
    Flags = FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
    DefaultUpdateSourceTrigger = UpdateSourceTrigger.Default,
    IsAnimationProhibited = true,
    ValidateValueCallback = nameof(ValidateValueCallback))]
public partial class DragBehavior
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

- `[AttachedProperty<bool>("IsDraggable")]`
  - Declares a **dependency property** named `IsDraggable` of type `bool` for the `DragBehavior` class.
  - Unlike a regular dependency property, an **attached property** is **not tied to a single class** but can be applied to any **UI element**.
  - The source generator will automatically create:
    - A `DependencyProperty` field for `IsDraggableProperty`.
    - **Static** `GetIsDraggable` **and** `SetIsDraggable` **methods**, allowing other controls to use this property dynamically.

- `DefaultValue = false`
  - Specifies the default value of `IsDraggable` as `false`, meaning that **elements are not draggable unless explicitly enabled**.

- `PropertyChangedCallback = nameof(PropertyChangedCallback)`
  - Assigns a **property changed callback method**, that is triggered **whenever the** `IsDraggable` **value changes**.
  - This allows dynamic behavior updates—e.g., adding or removing event handlers for drag operations.

- `CoerceValueCallback = nameof(CoerceValueCallback)`
  - Called before the property value is assigned.
  - This method can **modify the value before applying it**.
  - Example: If an element **must not be draggable** under certain conditions, the `CoerceValueCallback` can force the value back to `false`.

- `Flags = FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender`
  - Specifies that changes to `IsDraggable` affect the UI layout and rendering.
  - `AffectsMeasure`: Triggers a re-measure of the UI element if this property changes.
  - `AffectsRender`: Causes a re-render of the control when this property is modified.
  - Example: If `IsDraggable` changes, **drag indicators or visual cues might need updating**.

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
  - By setting `IsAnimationProhibited = true`, you ensure that **no animations** can modify `IsDraggable`.
  - This is useful for properties where **instant updates are required**, such as boolean state changes.

- `ValidateValueCallback = nameof(ValidateValueCallback)`
  - Assigns a **validation callback method**, which ensures that only valid values are assigned to the dependency property.
  - This function **executes before** the property value is set, allowing you to **reject invalid values** before they are applied.
  - The `ValidateValueCallback` method should return a `bool`:
    - `true`: The value is accepted and applied to the property.
    - `false`: The value is considered invalid, and an exception is thrown.

### ⚙️ Auto-Generated Code - for complex example (WPF)

The source generator will produce equivalent code:

```csharp
public partial class DragBehavior
{
    public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.RegisterAttached(
        "IsDraggable",
        typeof(bool),
        typeof(DragBehavior),
        new FrameworkPropertyMetadata(
            defaultValue: Atc.XamlToolkit.BooleanBoxes.FalseBox,
            propertyChangedCallback: PropertyChangedCallback,
            coerceValueCallback: CoerceValueCallback,
            flags: FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
            defaultUpdateSourceTrigger: UpdateSourceTrigger.Default,
            isAnimationProhibited: true),
        validateValueCallback: ValidateValueCallback);

    public static bool GetIsDraggable(DependencyObject element)
        => (bool)element.GetValue(IsDraggableProperty);

    public static void SetIsDraggable(DependencyObject element, bool value)
        => element?.SetValue(IsDraggableProperty, Atc.XamlToolkit.BooleanBoxes.Box(value));
}
```

`validateValueCallback` is an argument of `RegisterAttached`, after the metadata. The generated file has `using System.Windows;` and `using System.Windows.Data;` (for `UpdateSourceTrigger`).

### ⚙️ Auto-Generated Code - for complex example (Avalonia)

Avalonia's registration surface is **smaller** by design — there are no flags, no `ValidateValueCallback`, no `CoerceValueCallback`, no `DefaultUpdateSourceTrigger`, no `IsAnimationProhibited`. Those options do not exist on the Avalonia `[AttachedProperty]` attribute, so specifying one is a compile error (CS0246). Of the options in the WPF example, `DefaultValue` and `PropertyChangedCallback` remain; the [options reference](#-options-reference) lists everything the Avalonia attribute has.

`PropertyChangedCallback` is wired up differently: instead of being passed into the registration call, it's bound through `Property.Changed.AddClassHandler<AvaloniaObject>(...)` in a generated **static field initializer**. The handler runs for every `AvaloniaObject` the property is attached to, and the class can still declare its own static constructor:

```csharp
[AttachedProperty<bool>(
    "IsDraggable",
    DefaultValue = false,
    PropertyChangedCallback = nameof(PropertyChangedCallback))]
public partial class DragBehavior : AvaloniaObject
{
    private static void PropertyChangedCallback(
        AvaloniaObject d,
        AvaloniaPropertyChangedEventArgs e)
    {
        // …
    }
}
```

…produces:

```csharp
public partial class DragBehavior : AvaloniaObject
{
    public static readonly Avalonia.AvaloniaProperty IsDraggableProperty =
        Avalonia.AvaloniaProperty.RegisterAttached<DragBehavior, AvaloniaObject, bool>(
            "IsDraggable",
            defaultValue: false);

    private static readonly System.IDisposable isDraggablePropertyChangedSubscription =
        IsDraggableProperty.Changed.AddClassHandler<AvaloniaObject>(PropertyChangedCallback);

    public static bool GetIsDraggable(AvaloniaObject element)
        => (bool)element.GetValue(IsDraggableProperty)!;

    public static void SetIsDraggable(AvaloniaObject element, bool value)
        => element?.SetValue(IsDraggableProperty, value);
}
```

**Why a static field initializer?** Avalonia separates the property *definition* from the *change subscription*. The `Changed.AddClassHandler<AvaloniaObject>(...)` call wires the callback once when the type loads. It is typed to `AvaloniaObject`, not to the owning class, because an attached property is set on other objects. A field initializer is used instead of a static constructor so that the class can declare a static constructor of its own.

**The handler signature** also differs from WPF: `(AvaloniaObject, AvaloniaPropertyChangedEventArgs)` instead of `(DependencyObject, DependencyPropertyChangedEventArgs)`. Inside, you typically pattern-match on the first argument to the host type (e.g., `if (d is Control element) { ... }`) — see `sample/Atc.XamlToolkit.AvaloniaSample/SampleControls/FrameworkElements/AttachedPropertyComponents/HighlightBehavior.cs` for a runnable example.
