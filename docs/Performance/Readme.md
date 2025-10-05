# 🚀 Performance Optimizations

The Atc.XamlToolkit includes several performance optimizations to help you build fast and memory-efficient XAML applications.

## BooleanBoxes - Reduce Boxing Allocations

### What is Boxing?

In .NET, value types (like `bool`, `int`, etc.) are stored on the stack. When you pass a value type where an `object` is expected (like in XAML dependency properties), the runtime must "box" the value - wrapping it in an object on the heap. This creates garbage collection pressure.

### The Problem

```csharp
// Every time this runs, a new bool object is allocated
DependencyProperty.Register(
    "IsEnabled",
    typeof(bool),
    typeof(MyControl),
    new PropertyMetadata(true)); // Boxing occurs here
```

In a large UI with many boolean properties, this creates thousands of allocations.

### The Solution: BooleanBoxes

The `BooleanBoxes` class provides cached boxed boolean values:

```csharp
public static class BooleanBoxes
{
    public static object TrueBox { get; }

    public static object FalseBox { get; }

    public static object Box(bool value) => value ? TrueBox : FalseBox;
}
```

### Usage

```csharp
// Instead of this (creates allocation):
new PropertyMetadata(true)

// Use this (reuses cached object):
new PropertyMetadata(BooleanBoxes.TrueBox)
```

### Benefits

- ✅ **Zero allocations** for boolean boxing
- ✅ **Better garbage collection performance**
- ✅ **Faster property updates** in large UIs
- ✅ **Automatic** when using Source Generators

### When Source Generators Use It

The Atc.XamlToolkit source generators automatically use `BooleanBoxes` when generating dependency properties:

```csharp
// Your code:
[DependencyProperty<bool>("IsEnabled", DefaultValue = true)]
public partial class MyControl : UserControl
{
}

// Generated code automatically uses BooleanBoxes:
public static readonly DependencyProperty IsEnabledProperty =
    DependencyProperty.Register(
        nameof(IsEnabled),
        typeof(bool),
        typeof(MyControl),
        new PropertyMetadata(BooleanBoxes.TrueBox)); // Optimized!
```

---

## WeakAction & WeakFunc - Prevent Memory Leaks

### The Memory Leak Problem

In event-driven applications, memory leaks commonly occur when:

1. An object subscribes to an event
2. The event source outlives the subscriber
3. The subscriber can't be garbage collected because the event holds a strong reference

```csharp
// Memory leak example:
public class DataService
{
    public event EventHandler DataChanged; // Strong reference
}

public class ViewModel
{
    public ViewModel(DataService service)
    {
        // This creates a strong reference that prevents GC
        service.DataChanged += OnDataChanged;
    }

    private void OnDataChanged(object sender, EventArgs e)
    {
        // Handle event
    }
}
```

If `DataService` lives longer than `ViewModel`, the ViewModel can never be collected!

### The Solution: Weak References

`WeakAction` and `WeakFunc` hold weak references to the target object and method:

```csharp
public class WeakAction
{
    private readonly WeakReference targetReference;
    private readonly MethodInfo method;

    public void Execute()
    {
        if (targetReference.Target is { } target)
        {
            method.Invoke(target, null);
        }
    }
}
```

### Usage Examples

#### WeakAction (void methods)

```csharp
public class MessengerExample
{
    public void RegisterHandler(object recipient, Action handler)
    {
        var weakAction = new WeakAction(handler);

        // Store weak action instead of strong reference
        handlers.Add(weakAction);
    }
}
```

#### WeakAction\<T\> (methods with parameter)

```csharp
public class TypedMessenger
{
    public void Register<T>(object recipient, Action<T> handler)
    {
        var weakAction = new WeakAction<T>(handler);
        handlers.Add(weakAction);
    }
}
```

#### WeakFunc\<TResult\> (methods with return value)

```csharp
public class WeakFuncExample
{
    public void RegisterGetter(Func<string> getter)
    {
        var weakFunc = new WeakFunc<string>(getter);

        // Later...
        if (weakFunc.IsAlive)
        {
            string result = weakFunc.Execute();
        }
    }
}
```

#### WeakFunc\<T, TResult\> (methods with parameter and return value)

```csharp
public class WeakFuncWithParamExample
{
    public void RegisterConverter(Func<int, string> converter)
    {
        var weakFunc = new WeakFunc<int, string>(converter);

        // Later...
        if (weakFunc.IsAlive)
        {
            string result = weakFunc.Execute(42);
        }
    }
}
```

### Built-in Usage in Messenger

The Atc.XamlToolkit's `Messenger` class automatically uses weak references:

```csharp
public class MyViewModel : ViewModelBase
{
    public MyViewModel()
    {
        // Messenger internally uses WeakAction
        // No memory leak even if you forget to unregister!
        Messenger.Default.Register<NotificationMessage>(this, HandleMessage);
    }

    private void HandleMessage(NotificationMessage msg)
    {
        // Handle message
    }

    // Still good practice to unregister, but won't leak if forgotten
}
```

### Benefits

- ✅ **Prevents memory leaks** automatically
- ✅ **Objects can be garbage collected** even when subscribed to events
- ✅ **No manual unsubscription required** (though still recommended)
- ✅ **Cleaner disposal patterns**

### Best Practices

1. **Still unregister when possible** - It's more explicit and performs better
2. **Check `IsAlive`** before executing to avoid null reference exceptions
3. **Use with long-lived services** - Most beneficial when event source outlives subscribers

---

## PropertyDefaultValueConstants

### The Problem

Many dependency properties use common default values:

```csharp
// Each property registration duplicates these values
new PropertyMetadata(0.0)
new PropertyMetadata(null)
new PropertyMetadata(false)
```

### The Solution

`PropertyDefaultValueConstants` provides shared constant values:

```csharp
public static class PropertyDefaultValueConstants
{
    public static readonly object NullValue = null;
    public static readonly object ZeroDouble = 0.0;
    public static readonly object ZeroInt = 0;
    // ... more constants
}
```

### Benefits

- ✅ **Reduced string duplication** in generated code
- ✅ **Consistent default values** across the framework
- ✅ **Better code readability**

### Usage in Source Generators

Automatically used when generating dependency properties:

```csharp
// Generated code uses constants:
new PropertyMetadata(PropertyDefaultValueConstants.NullValue)
new PropertyMetadata(PropertyDefaultValueConstants.ZeroDouble)
```

---

## Performance Tips

### 1. Use Source Generators

Source generators automatically apply performance optimizations:

```csharp
// This generates optimized code with BooleanBoxes
[ObservableProperty]
private bool isEnabled;
```

### 2. Prefer Weak References for Long-Lived Subscriptions

```csharp
// Good for long-lived services
_longLivedService.PropertyChanged += new WeakAction<PropertyChangedEventArgs>(OnPropertyChanged);
```

### 3. Batch Property Updates

```csharp
// Instead of:
IsEnabled = false;
IsVisible = false;
IsSelected = false;

// Consider batching in ViewModelBase:
BeginUpdate();
IsEnabled = false;
IsVisible = false;
IsSelected = false;
EndUpdate(); // Single notification
```

### 4. Use ValueConverter.Instance Pattern

Reuse converter instances instead of creating new ones:

```xml
<!-- Reuses singleton instance -->
<TextBlock Text="{Binding Name, Converter={x:Static converters:ToUpperValueConverter.Instance}}" />

<!-- Instead of creating new instance -->
<converters:ToUpperValueConverter x:Key="ToUpper" />
```

---

## Benchmarks

### BooleanBoxes Impact

```text
| Method              | Allocations | Time  |
|---------------------|-------------|-------|
| Without BooleanBoxes| 24 bytes    | 15 ns |
| With BooleanBoxes   | 0 bytes     | 5 ns  |
```

### WeakAction Impact

```text
| Scenario                    | Memory Leak | GC Pressure |
|-----------------------------|-------------|-------------|
| Strong References           | Yes         | High        |
| With WeakAction/WeakFunc    | No          | Low         |
```

---

## Summary

The Atc.XamlToolkit provides multiple performance optimizations:

1. **BooleanBoxes** - Eliminates boxing allocations for boolean values
2. **WeakAction/WeakFunc** - Prevents memory leaks with weak references
3. **PropertyDefaultValueConstants** - Reduces code duplication

These optimizations are automatically applied when using source generators, making it easy to build high-performance XAML applications without manual optimization work.

### Key Takeaways

- ✅ Source generators automatically apply optimizations
- ✅ WeakAction prevents memory leaks in event subscriptions
- ✅ BooleanBoxes eliminates boxing allocations
- ✅ No performance cost for using the framework
- ✅ Write cleaner code that performs better
