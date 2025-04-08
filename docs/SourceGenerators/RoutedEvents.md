# ⚙️ RoutedEvents with Source Generation

> ❗ This feature is only supported in WPF for now ❗

In WPF, **routed events** enable events to travel up or down the visual tree, allowing parent or ancestor elements to handle events raised by child elements. This mechanism simplifies event handling in complex user interfaces by reducing the need to wire up events on every control.

Traditionally, creating a routed event involves writing repetitive boilerplate code using EventManager.RegisterRoutedEvent and manually defining event add/remove accessors. With Atc.XamlToolkit’s source generators, you can simply annotate your class or field, and the generator will automatically produce the code necessary for registering and exposing the event.

---

## 🚀 Defining a Routed Event

There are two common approaches to declare a routed event using source generation.

### ✨ Field-Level Declaration

Alternatively, you can mark a field with the `[RoutedEvent]` attribute, allowing the generator to infer the event name from the field name:

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
        name: "Tap",
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

---

## 🖥️ XAML Usage Example

Once declared, the routed event can be utilized in your XAML like any standard event:

```xaml
<Window xmlns:local="clr-namespace:YourNamespace">
    <Grid>
        <local:CustomButton Tap="CustomButton_TapHandler" />
    </Grid>
</Window>
```

And in your code-behind, you can define the event handler:

```csharp
private void CustomButton_TapHandler(object sender, RoutedEventArgs e)
{
    // Handle the Tap event here.
}
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
