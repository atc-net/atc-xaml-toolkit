# ⚙️ AttachedProperty with SourceGeneration

> ✅ This feature is supported in `WPF`, `WinUI 3`, and `Avalonia` ✅

In WPF, WinUI 3, and Avalonia, **attached properties** are a type of dependency/styled property that allows properties to be defined in one class but used in another. They are widely used in scenarios like behaviors, layout configurations, and interactions where a property needs to be applied to multiple elements without modifying their class definitions. Traditionally, defining attached properties requires boilerplate code, but source generators can automate this process, reducing errors and improving maintainability.

> **Important:** Classes using `[AttachedProperty]` must either inherit from `UserControl`, `DependencyObject`, or `FrameworkElement`, OR have a class name ending with **"Attach"**, **"Behavior"**, or **"Helper"** (e.g., `DragBehavior`, `WatermarkTextBoxBehavior`, `CheckBoxHelper`). This requirement ensures the source generator can properly detect and process your attached properties.

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
- If you specify WPF-only features in WinUI, they will be ignored by the generator

**Avalonia:**
- Uses generic type parameters in property registration (e.g., `RegisterAttached<TOwner, THost, TValue>`)
- Uses plain `bool` values instead of `BooleanBoxes`
- Generated Get/Set methods use `AvaloniaObject` as the parameter type
- **Important:** Owner class must inherit from `AvaloniaObject` (cannot be static)
- Does NOT support WPF-specific features like `ValidateValueCallback`, `CoerceValueCallback`, or metadata flags

The source generator automatically detects your platform and generates the appropriate code.

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
        new PropertyMetadata(defaultValue: BooleanBoxes.FalseBox));

    public static bool GetIsDraggable(DependencyObject element)
        => (bool)element.GetValue(IsDraggableProperty);

    public static void SetIsDraggable(DependencyObject element, bool value)
        => element?.SetValue(IsDraggableProperty, BooleanBoxes.Box(value));
}
```

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
- The `Get` accessor uses the **null-forgiving operator** (`element.GetValue(...)!`) for non-nullable value types because `AvaloniaObject.GetValue` returns `object?`.

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
    public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.Register(
        nameof(IsDraggable),
        typeof(bool),
        typeof(DragBehavior),
        new FrameworkPropertyMetadata(
            defaultValue: BooleanBoxes.FalseBox,
            propertyChangedCallback: PropertyChangedCallback,
            coerceValueCallback: CoerceValueCallback,
            flags: FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
            defaultUpdateSourceTrigger: UpdateSourceTrigger.Default,
            isAnimationProhibited: true,
            validateValueCallback = ValidateValueCallback));

    public bool IsDraggable
    {
        get => (bool)GetValue(IsDraggableProperty);
        set => SetValue(IsDraggableProperty, BooleanBoxes.Box(value));
    }
}
```

### ⚙️ Auto-Generated Code - for complex example (Avalonia)

Avalonia's registration surface is **smaller** by design — there are no flags, no `ValidateValueCallback`, no `CoerceValueCallback`, no `DefaultUpdateSourceTrigger`, no `IsAnimationProhibited`. Specifying any of those on `[AttachedProperty]` for an Avalonia target is a no-op (the generator silently drops them). The two surviving knobs are `DefaultValue` and `PropertyChangedCallback`.

`PropertyChangedCallback` is wired up differently: instead of being passed into the registration call, it's bound through `Property.Changed.AddClassHandler<TOwner>(...)` in a generated **static constructor**:

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
    static DragBehavior()
    {
        IsDraggableProperty.Changed.AddClassHandler<DragBehavior>(PropertyChangedCallback);
    }

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

**Why a static constructor?** Avalonia separates the property *definition* from per-class *change subscriptions*. The `Changed.AddClassHandler<T>(...)` call wires the callback once when the type loads — typed to the owning class so the callback receives a strongly-typed first argument. This is the canonical Avalonia pattern; the generator mirrors it directly.

**The handler signature** also differs from WPF: `(AvaloniaObject, AvaloniaPropertyChangedEventArgs)` instead of `(DependencyObject, DependencyPropertyChangedEventArgs)`. Inside, you typically pattern-match on the first argument to the host type (e.g., `if (d is Control element) { ... }`) — see `sample/Atc.XamlToolkit.AvaloniaSample/SampleControls/FrameworkElements/AttachedPropertyComponents/HighlightBehavior.cs` for a runnable example.
