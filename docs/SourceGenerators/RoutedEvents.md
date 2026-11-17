# ⚙️ RoutedEvents with Source Generation

> ❗ This feature is only supported in WPF ❗

In WPF, **routed events** enable events to travel up or down the visual tree, allowing parent or ancestor elements to handle events raised by child elements. This mechanism simplifies event handling in complex user interfaces by reducing the need to wire up events on every control.

Traditionally, creating a routed event involves writing repetitive boilerplate code using EventManager.RegisterRoutedEvent and manually defining event add/remove accessors. With Atc.XamlToolkit's source generators, you can simply annotate your class or field, and the generator will automatically produce the code necessary for registering and exposing the event.

> **Note:** Routed events are a WPF-specific concept. Avalonia and WinUI have different event systems and do not support routed events in the same way. For WinUI and Avalonia, use standard .NET events instead.

> **Note:** `[RoutedEvent]` is picked up in any `partial` class of a WPF project, whatever it is called. In a non-static class the generated event calls `AddHandler` / `RemoveHandler`, so the class has to derive from `UIElement` (any control) or `ContentElement` for the generated code to compile. A class that is not `partial` gets no generated code and is reported as `AtcXamlToolkit0002`. The field must be `private`; a field that is not is skipped without a diagnostic.

---

## 🔍 Platform Support

### Why WPF Only?

Routed events are fundamentally a WPF-specific feature that relies on infrastructure not available in other XAML frameworks:

**WPF (✅ Full Support):**
- Native `RoutedEvent` type and `EventManager.RegisterRoutedEvent()` API
- Supports three routing strategies: Bubble (child → parent), Tunnel (parent → child), and Direct
- Custom event handler types with typed `RoutedEventArgs`
- Built-in event routing through the visual tree
- The `[RoutedEvent]` attribute generates all necessary boilerplate

**WinUI 3 (❌ No Support):**
- Does NOT have `RoutedEvent` or `EventManager` classes
- Microsoft.UI.Xaml uses standard .NET events exclusively
- No event routing infrastructure for custom events
- Built-in framework events (like `Click`, `Loaded`) exist but cannot be replicated for custom scenarios
- **Alternative:** Use standard .NET events with `EventHandler` or `EventHandler<TEventArgs>`

**Avalonia (⚠️ Different System):**
- Has `RoutedEventArgs` but uses a different event registration mechanism
- Cannot create custom routed events using `EventManager` (doesn't exist)
- Uses `AddHandler` with pre-defined events from framework classes like `InputElement`
- Custom events rely on standard .NET event patterns
- **Alternative:** Use standard .NET events or leverage Avalonia's built-in event routing for framework-provided events

### Source Generator Behavior

The source generator automatically detects your target platform and:

- **WPF Projects:** Generates full routed event code with `EventManager.RegisterRoutedEvent()`, routing strategies, and CLR event wrappers.
- **WinUI / Avalonia Projects:** Skips routed-event generation **and emits diagnostic `AtcXamlToolkit0010`** as a build-time warning, telling you to use a standard CLR event instead.

`AtcXamlToolkit0010` is the safety net that turns the platform skip from a silent inertness into an explicit message — so you find out at compile time, not at runtime when the event you wrote never raises.

The `[RoutedEvent]` attribute class only exists in the WPF package (`Atc.XamlToolkit.Wpf`). In a WinUI or Avalonia project the attribute itself therefore does not compile (CS0616 on WinUI, where `RoutedEvent` resolves to `Microsoft.UI.Xaml.RoutedEvent`; CS0246 on Avalonia), and `AtcXamlToolkit0010` is reported next to that error to say why. A source file with `[RoutedEvent]` cannot be shared unchanged between a WPF project and a WinUI or Avalonia project; put the field inside `#if` or in a WPF-only file.

```text
warning AtcXamlToolkit0010: Field 'tap' is decorated with [RoutedEvent] but the project targets Avalonia. The source generator silently skips routed-event generation on non-WPF platforms because WinUI 3 and Avalonia have no equivalent of WPF's RoutedEvent / EventManager pattern. Replace the field with a standard CLR event (e.g. 'public event RoutedEventHandler ItemSelected;') or remove the attribute.
```

---

## 🚀 Defining a Routed Event

There are two common approaches to declare a routed event using source generation.

### ✨ Field-Level Declaration

Annotate a *static* `RoutedEvent` field with the `[RoutedEvent]` attribute, allowing the generator to infer the event name from the field name (pascal‑case the first letter):

```csharp
public partial class CustomButton : Button
{
    [RoutedEvent(RoutingStrategy.Bubble)]
    private static readonly RoutedEvent tap;
}
```

### ⚙️ Auto-Generated Code

The source generator will produce code equivalent to:

```csharp
public partial class CustomButton
{
    public static readonly RoutedEvent TapEvent = EventManager.RegisterRoutedEvent(
        name: nameof(Tap),
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(RoutedEventHandler),
        ownerType: typeof(CustomButton));

    public event RoutedEventHandler Tap
    {
        add => AddHandler(TapEvent, value);
        remove => RemoveHandler(TapEvent, value);
    }
}
```

When the field has XML doc comments, the generator copies them to the generated event, and the `{Name}Event` field gets an `Identifies the <see cref="{Name}"/> routed event.` summary.

### 📚 Default documentation

With `GenerateDocumentation = true`, or `[assembly: GenerateDocumentationDefault]` for the whole project, the generated members get default summaries when the field has no XML doc of its own, so a project that reports CS1591 does not have to document every field by hand:

| Member | Summary |
|--------|---------|
| `{Name}Event` | `Identifies the <see cref="{Name}"/> routed event.` |
| `{Name}` event | `Occurs when the {Name} routed event is raised.` |
| `Raise{Name}` method | `Raises the <see cref="{Name}"/> routed event.` |

`GenerateDocumentation = false` turns it off for one event when the assembly default is on. XML doc comments on the field always win for the event.

### 📣 Generating a method that raises the event

`GenerateRaiseMethod = true` adds a method that creates the event args and raises the event. Its shape follows the handler type:

```csharp
[RoutedEvent(GenerateRaiseMethod = true)]
private static readonly RoutedEvent opened;

[RoutedEvent(HandlerType = typeof(RoutedPropertyChangedEventHandler<int>), GenerateRaiseMethod = true)]
private static readonly RoutedEvent valueChanged;

[RoutedEvent(HandlerType = typeof(EventHandler<DeviceErrorRoutedEventArgs>), GenerateRaiseMethod = true)]
private static readonly RoutedEvent deviceError;
```

```csharp
protected void RaiseOpened()
    => RaiseEvent(new RoutedEventArgs(OpenedEvent, this));

protected void RaiseValueChanged(int oldValue, int newValue)
    => RaiseEvent(new RoutedPropertyChangedEventArgs<int>(oldValue, newValue, ValueChangedEvent));

protected void RaiseDeviceError(DeviceErrorRoutedEventArgs e)
{
    e.RoutedEvent = DeviceErrorEvent;
    RaiseEvent(e);
}
```

- The handler type of the event is written with its namespace: `public event System.Windows.RoutedPropertyChangedEventHandler<int> ValueChanged`, `public event System.EventHandler<DeviceErrorRoutedEventArgs> DeviceError`.
- The method is `protected`, or `private` in a sealed class. `RaiseMethodAccessModifier` states another accessibility, for example `RaiseMethodAccessModifier = PropertyAccessModifier.Private` in a public control whose API should not grow a protected method.
- For any other handler type, such as a custom delegate, no method is generated: the constructor of its event args cannot be known. The generator reports `AtcXamlToolkit0013`; raise the event yourself with `RaiseEvent(...)`.

### 🤝 Sharing an existing event with `AddOwnerOf`

A control that should expose an event another class already registered adds itself as an owner. A handler for the original event then also receives the event raised by the control:

```csharp
public partial class DrivePicker : UserControl
{
    [RoutedEvent(AddOwnerOf = typeof(DevicePickerEvents), HandlerType = typeof(EventHandler<DeviceErrorRoutedEventArgs>))]
    private static readonly RoutedEvent deviceError;
}
```

```csharp
public static readonly RoutedEvent DeviceErrorEvent = global::DevicePickerEvents.DeviceErrorEvent.AddOwner(typeof(DrivePicker));

public event System.EventHandler<DeviceErrorRoutedEventArgs> DeviceError
{
    add => AddHandler(DeviceErrorEvent, value);
    remove => RemoveHandler(DeviceErrorEvent, value);
}
```

The shared event is `{Type}.{Name}Event`, so the field must have the name of the event being shared. The type is written with its full name, `global::` and namespace (`global::MyApp.DevicePickerEvents`). State the `HandlerType` of the shared event; the routing strategy is that of the shared event and the positional argument is ignored, without a diagnostic. `GenerateRaiseMethod` can be combined with `AddOwnerOf`.

### 🧷 Routed events in a static class (attached events)

A static class cannot have an instance event. For a `[RoutedEvent]` field in a static class the generator registers the event and emits methods that attach a handler to an element:

```csharp
public static partial class DataGridFilter
{
    [RoutedEvent(HandlerType = typeof(DataGridFilterChangedEventHandler))]
    private static readonly RoutedEvent filterChanged;
}
```

```csharp
public static readonly RoutedEvent FilterChangedEvent = EventManager.RegisterRoutedEvent(
    name: "FilterChanged",
    routingStrategy: RoutingStrategy.Bubble,
    handlerType: typeof(DataGridFilterChangedEventHandler),
    ownerType: typeof(DataGridFilter));

public static void AddFilterChangedHandler(DependencyObject element, DataGridFilterChangedEventHandler handler)
{
    if (element is UIElement uiElement)
    {
        uiElement.AddHandler(FilterChangedEvent, handler);
    }
    else if (element is ContentElement contentElement)
    {
        contentElement.AddHandler(FilterChangedEvent, handler);
    }
}

// RemoveFilterChangedHandler is generated the same way.
```

A class that only holds events for other classes to share with `AddOwnerOf` needs no handler methods; `GenerateHandlerMethods = false` leaves them out and generates the `{Name}Event` field alone.

This is the pattern XAML needs for `local:DataGridFilter.FilterChanged="..."`. `GenerateRaiseMethod` has no effect in a static class and is reported as `AtcXamlToolkit0013`; raise the event on the element with `element.RaiseEvent(...)`.

### 🛠️ Specifying a custom delegate type

If you want a strongly‑typed event handler (instead of the generic RoutedEventHandler) add the HandlerType named argument:

```csharp
public partial class NumericBox : Control
{
    // The event will have the delegate type NumericBoxChangedRoutedEventHandler
    [RoutedEvent(HandlerType = typeof(NumericBoxChangedRoutedEventHandler))]
    private static readonly RoutedEvent valueIncremented;
}
```

The source generator will produce code equivalent to:

```csharp
public partial class NumericBox
{
    public static readonly RoutedEvent ValueIncrementedEvent = EventManager.RegisterRoutedEvent(
        name: nameof(ValueIncremented),
        routingStrategy: RoutingStrategy.Bubble,
        handlerType: typeof(NumericBoxChangedRoutedEventHandler),
        ownerType: typeof(NumericBox));

    public event NumericBoxChangedRoutedEventHandler ValueIncremented
    {
        add => AddHandler(ValueIncrementedEvent, value);
        remove => RemoveHandler(ValueIncrementedEvent, value);
    }
}
```


---

## 🖥️ XAML Usage Example

Once declared, the routed event can be utilized in your XAML like any standard event:

```xaml
<Window xmlns:local="clr-namespace:YourNamespace">
    <Grid>
        <local:CustomButton Tap="CustomButton_TapHandler" />
        <local:NumericBox ValueIncremented="NumericBox_OnValueIncremented"/>
    </Grid>
</Window>
```

And in your code-behind, you can define the event handler:

```csharp
private void CustomButton_TapHandler(object sender, RoutedEventArgs e)
{
    // Handle the Tap event here.
}

private void NumericBox_OnValueIncremented(
    object sender,
    NumericBoxChangedRoutedEventArgs e) // 🎯 strong type!
{
    Debug.WriteLine($"Old={e.OldValue}, New={e.NewValue}");
}
```

## 🧩 Attribute reference

| Argument        | Position/Name | Type            | Default                    | Description                                |
|-----------------|---------------|-----------------|----------------------------|--------------------------------------------|
| routingStrategy | positional    | RoutingStrategy | Bubble                     | How the event propagates.                  |
| HandlerType     | named         | Type            | typeof(RoutedEventHandler) | CLR delegate type for the generated event. |
| AddOwnerOf      | named         | Type            | none                       | Share the event of this type instead of registering a new one. |
| GenerateRaiseMethod | named     | bool            | false                      | Generate a `Raise{Name}` method. |
| GenerateHandlerMethods | named  | bool            | true                       | In a static class: generate `Add{Name}Handler` / `Remove{Name}Handler`. |
| RaiseMethodAccessModifier | named | PropertyAccessModifier | protected (private when sealed) | Accessibility of the `Raise{Name}` method. |
| GenerateDocumentation | named   | bool            | assembly default           | Emit default summaries on the generated members. |

```csharp
[RoutedEvent]                         // Bubble + RoutedEventHandler
[RoutedEvent(RoutingStrategy.Tunnel)] // Tunnel + RoutedEventHandler
[RoutedEvent(HandlerType = typeof(MyHandler))]
[RoutedEvent(RoutingStrategy.Direct, HandlerType = typeof(MyHandler))]
```

## 📌 Summary

- ✔️ **Simplified Declaration:** Use the [RoutedEvent] attribute to eliminate manual boilerplate.

- ✔️ **Automatic Registration:** The source generator auto-creates the `RoutedEvent` field and event accessors.

- ✔️ **Flexible Integration:** The generated code supports standard `WPF` event patterns, making it easy to wire up events in `XAML`.

- ✔️ **Enhanced Consistency:** Centralized auto-generation minimizes human error and improves maintainability.

### 🚀 Why Use Atc.XamlToolkit’s Source Generators?

- ✅ **Eliminates Redundant Code:** Automatically generates registration and event accessor methods.

- ✅ **Ensures Consistency:** Guarantees all routed events adhere to best practices.

- ✅ **Improves Maintainability:** Reduces the likelihood of errors with auto-generated boilerplate.

- ✅ **Streamlines Development:** Focus more on business logic rather than repetitive code patterns.
