# ⚙️ RoutedEvents with Source Generation

> ❗ This feature is only supported in WPF ❗

In WPF, **routed events** enable events to travel up or down the visual tree, allowing parent or ancestor elements to handle events raised by child elements. This mechanism simplifies event handling in complex user interfaces by reducing the need to wire up events on every control.

Traditionally, creating a routed event involves writing repetitive boilerplate code using EventManager.RegisterRoutedEvent and manually defining event add/remove accessors. With Atc.XamlToolkit's source generators, you can simply annotate your class or field, and the generator will automatically produce the code necessary for registering and exposing the event.

> **Note:** Routed events are a WPF-specific concept. Avalonia and WinUI have different event systems and do not support routed events in the same way. For WinUI and Avalonia, use standard .NET events instead.

> **Important:** Classes using `[RoutedEvent]` must either inherit from `UserControl`, `DependencyObject`, or `FrameworkElement`, OR have a class name ending with **"Attach"**, **"Behavior"**, or **"Helper"**. This requirement ensures the source generator can properly detect and process your routed events.

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

```text
warning AtcXamlToolkit0010: Field 'tap' is decorated with [RoutedEvent] but the project targets Avalonia. The source generator silently skips routed-event generation on non-WPF platforms because WinUI 3 and Avalonia have no equivalent of WPF's RoutedEvent / EventManager pattern. Replace the field with a standard CLR event (e.g. 'public event RoutedEventHandler ItemSelected;') or remove the attribute.
```

---

## 🚀 Defining a Routed Event

There are two common approaches to declare a routed event using source generation.

### ✨ Field-Level Declaration

Annotate a *static* `RoutedEvent` field with the `[RoutedEvent]` attribute, allowing the generator to infer the event name from the field name (pascal‑case the first letter):

```csharp
public static partial class CustomButton
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

### 🛠️ Specifying a custom delegate type

If you want a strongly‑typed event handler (instead of the generic RoutedEventHandler) add the HandlerType named argument:

```csharp
public partial class NumericBox
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
        handlerType: typeof(RoutedEventHandler),
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
