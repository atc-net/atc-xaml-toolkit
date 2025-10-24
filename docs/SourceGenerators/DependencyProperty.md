# ⚙️ DependencyProperties with SourceGeneration

> ✅ This feature is supported in `WPF`, `WinUI 3`, and `Avalonia` ✅

In WPF and WinUI 3, **dependency properties** are a specialized type of property that extends the functionality of standard CLR properties. In Avalonia, the equivalent is **styled properties**. These properties support features such as data binding, animation, and property value inheritance, which are integral to the XAML property system. However, defining these properties traditionally involves verbose boilerplate code. To streamline this process, source generators can automatically generate the necessary code, reducing errors and improving maintainability.

> **Note:** For Avalonia, use the `[StyledProperty]` attribute instead of `[DependencyProperty]`. The source generator will automatically create `StyledProperty<T>` registrations with the appropriate Avalonia-specific code.

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
  - (Optionally) `INotifyPropertyChanged` support if configured.

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
- If you specify WPF-only features in WinUI, they will be ignored by the generator

**Avalonia:**
- Uses `[StyledProperty]` attribute instead of `[DependencyProperty]`
- Generates `StyledProperty<T>` fields instead of `DependencyProperty`
- Uses `Avalonia.AvaloniaProperty.Register<>()` for registration
- Property changed callbacks are registered in a static constructor via `.Changed.AddClassHandler<T>()`
- Does NOT use `BooleanBoxes` (uses plain bool values)
- Supports `DefaultValue` and `PropertyChangedCallback` parameters
- Control must inherit from `AvaloniaObject` (or `UserControl`, `Control`, etc.)

The source generator automatically detects your platform and generates the appropriate code.

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
        new FrameworkPropertyMetadata(defaultValue: BooleanBoxes.TrueBox);

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, BooleanBoxes.Box(value));
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
            defaultValue: BooleanBoxes.FalseBox,
            propertyChangedCallback: PropertyChangedCallback,
            coerceValueCallback: CoerceValueCallback,
            flags: FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
            defaultUpdateSourceTrigger: UpdateSourceTrigger.Default,
            isAnimationProhibited: true,
            validateValueCallback = ValidateValueCallback));

    public bool IsRunning
    {
        get => (bool)GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, BooleanBoxes.Box(value));
    }
}
```

---

## 🌐 Avalonia StyledProperty Example

### 📝 Human-Written Code - Avalonia

For Avalonia, use the `[StyledProperty]` attribute with field-level declaration:

```csharp
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Atc.XamlToolkit.Mvvm.Attributes;

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
        get => (Color)GetValue(SelectedColorProperty);
        set => SetValue(SelectedColorProperty, value);
    }

    public static readonly StyledProperty<byte> RedProperty = Avalonia.AvaloniaProperty.Register<ColorPickerControl, byte>(
        nameof(Red),
        defaultValue: (byte)0);

    public byte Red
    {
        get => (byte)GetValue(RedProperty);
        set => SetValue(RedProperty, value);
    }

    public static readonly StyledProperty<byte> GreenProperty = Avalonia.AvaloniaProperty.Register<ColorPickerControl, byte>(
        nameof(Green),
        defaultValue: (byte)0);

    public byte Green
    {
        get => (byte)GetValue(GreenProperty);
        set => SetValue(GreenProperty, value);
    }

    public static readonly StyledProperty<byte> BlueProperty = Avalonia.AvaloniaProperty.Register<ColorPickerControl, byte>(
        nameof(Blue),
        defaultValue: (byte)0);

    public byte Blue
    {
        get => (byte)GetValue(BlueProperty);
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
- Simpler metadata system - only `DefaultValue` and callbacks supported (no flags, coercion, validation)
