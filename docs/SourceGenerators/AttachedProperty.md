# ⚙️ AttachedProperty with SourceGeneration

> ❗ This feature is only supported in `WPF` for now ❗

In WPF, **attached properties** are a type of dependency property that allows properties to be defined in one class but used in another. They are widely used in scenarios like behaviors, layout configurations, and interactions where a property needs to be applied to multiple elements without modifying their class definitions. Traditionally, defining attached properties requires boilerplate code, but source generators can automate this process, reducing errors and improving maintainability.

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
    private bool isDraggable;
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
    private bool isDraggable
}
```

**In this example:**

- The attribute declares an attached property named `IsDraggable` of type `bool` for the `DragBehavior` class.

- In the field-level declaration, the generator infers the property name (capitalizing the field name) and type from the field itself.

- The source generator then creates the necessary registration code and accessor methods.

### ⚙️ Auto-Generated Code - for simple example

The source generator will produce code equivalent to:

```csharp
public static partial class DragBehavior
{
    public static readonly DependencyProperty IsDraggableProperty = DependencyProperty.RegisterAttached(
        "IsDraggable",
        typeof(bool),
        typeof(DragBehavior),
        new PropertyMetadata(defaultValue: BooleanBoxes.FalseBox));

    public static bool GetIsDraggable(UIElement element)
        => (bool)element.GetValue(IsDraggableProperty);

    public static void SetIsDraggable(UIElement element, bool value)
        => element?.SetValue(IsDraggableProperty, value);
}
```

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

### ⚙️ Auto-Generated Code - for complex example

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
        set => SetValue(IsDraggableProperty, value);
    }
}
```
