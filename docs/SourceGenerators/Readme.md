# 🛠 Source Generators

`Atc.XamlToolkit` ships three Roslyn source generators that eliminate boilerplate around MVVM and XAML framework-element plumbing. They run at compile time, work in the IDE for live IntelliSense, and target `netstandard2.0` so they load in any .NET workload.

## Decision tree

| You want to… | Use | Lives in | Page |
|---|---|---|---|
| Add `INotifyPropertyChanged` to a class that can't inherit `ObservableObject` | `[INotifyPropertyChanged]` | Plain class | [../Mvvm/Readme.md#-inpc-on-plain-classes-with-inotifypropertychanged](../Mvvm/Readme.md#-inpc-on-plain-classes-with-inotifypropertychanged) |
| Generate `INotifyPropertyChanged` properties from fields | `[ObservableProperty]` | View model | [ViewModel.md](ViewModel.md) |
| Generate `ICommand` properties from methods | `[RelayCommand]` | View model | [ViewModel.md](ViewModel.md) |
| Track a property that depends on others | `[ComputedProperty]` | View model | [ViewModel.md](ViewModel.md) |
| Wrap a DTO with `INotifyPropertyChanged` + `IsDirty` | `[ObservableDtoViewModel]` | View model | [ViewModel.md](ViewModel.md#-wrapping-dtos-with-observabledtoviewmodel) |
| Generate a WPF / WinUI `DependencyProperty` | `[DependencyProperty]` | Custom control / behavior | [DependencyProperty.md](DependencyProperty.md) |
| Generate an Avalonia `StyledProperty` | `[StyledProperty]` | Custom control / behavior | (see [DependencyProperty.md](DependencyProperty.md) — same generator, Avalonia branch) |
| Generate an attached property | `[AttachedProperty]` | Static helper / behavior | [AttachedProperty.md](AttachedProperty.md) |
| Generate a WPF routed event | `[RoutedEvent]` | Custom control | [RoutedEvents.md](RoutedEvents.md) |

## Platform support matrix

| Attribute | WPF | WinUI 3 | Avalonia | Notes |
|---|---|---|---|---|
| `[INotifyPropertyChanged]` | ✅ | ✅ | ✅ | Class-level attribute. Adds `PropertyChanged` event + `RaisePropertyChanged` / `OnPropertyChanged` / `Set<T>` helpers without requiring `ObservableObject` inheritance. Composes with `[ObservableProperty]`. |
| `[ObservableProperty]` | ✅ | ✅ | ✅ | Same generator across platforms. |
| `[RelayCommand]` | ✅ | ✅ | ✅ | Async-cancellation features supported on all three. |
| `[ComputedProperty]` | ✅ | ✅ | ✅ | Auto-detects dependencies in the getter expression. |
| `[ObservableDtoViewModel]` | ✅ | ✅ | ✅ | Re-uses `ViewModelBase`'s `IsDirty` / validation infra. |
| `[DependencyProperty]` | ✅ | ✅ | ❌ | Generates `DependencyProperty.Register(...)` + `FrameworkPropertyMetadata`. |
| `[StyledProperty]` | ❌ | ❌ | ✅ | Generates `AvaloniaProperty.Register<T,…>(...)`. Use this on Avalonia where you'd use `[DependencyProperty]` on WPF/WinUI. |
| `[AttachedProperty]` | ✅ | ✅ | ✅ | On Avalonia the owner class **must not** be `static` and must inherit from `AvaloniaObject`. |
| `[RoutedEvent]` | ✅ | ❌ (skipped) | ❌ (skipped) | Generator emits no code on non-WPF platforms — use plain .NET events on WinUI/Avalonia. |

## How class-naming gates the framework-element generator

`[DependencyProperty]`, `[AttachedProperty]`, `[StyledProperty]`, and `[RoutedEvent]` only fire on classes that look like a control or a behavior. The check (`HasAnythingAroundFrameworkElement` in the generator) accepts:

1. Classes inheriting from `UserControl`, `DependencyObject`, `FrameworkElement`, **or**
2. Class names ending in `Attach`, `Behavior`, or `Helper`, **or**
3. Classes that contain `[RoutedEvent]` attributes.

If you put `[DependencyProperty]` on, say, `MyService` and nothing happens, this is why — rename to `MyServiceHelper` or inherit from `DependencyObject`.

## Where the generated code lives

After a build, you can read the generated files at:

- WPF: `obj/{Configuration}/net10.0-windows/generated/Atc.XamlToolkit.SourceGenerators/`
- WinUI: `obj/{Configuration}/net10.0-windows10.0.19041.0/generated/Atc.XamlToolkit.SourceGenerators/`
- Avalonia: `obj/{Configuration}/net10.0/generated/Atc.XamlToolkit.SourceGenerators/`

If a change to a generator doesn't show up, run `dotnet build-server shutdown` to force the analyzer host to reload.

## Common pitfalls

- **Class isn't `partial`** — every generator emits a partial class. The compiler error message points at the missing `partial` keyword on your declaration.
- **Field naming for `[ObservableProperty]`** — fields must be camelCase (`firstName`); the generator produces PascalCase properties (`FirstName`). Validation attributes can sit on either the field or the generated property.
- **`[ComputedProperty]` recompute** — dependencies are inferred from the getter expression at compile time. If you reach into another object (`Customer.Address.City`), the generator only sees `Customer`; raise `RaisePropertyChanged(nameof(YourComputedProp))` manually for transitive changes.
- **`[RoutedEvent]` on non-WPF** — silently skipped; if you need the same source file to compile across platforms, that's by design. If you need event semantics on WinUI/Avalonia, declare a normal CLR event.

## Diagnostics

The generator surfaces these diagnostics at compile time so common misuses are caught up front instead of silently producing nothing.

| Id | Severity | When it fires | What to fix |
|---|---|---|---|
| `AtcXamlToolkit0001` | Warning | Two `[RelayCommand]` methods in the same view model resolve to the same generated command name. | Pass an explicit `commandName` to the second attribute, or rename the method. |
| `AtcXamlToolkit0002` | Warning | A class contains `[ObservableProperty]`, `[RelayCommand]`, `[ComputedProperty]`, or `[INotifyPropertyChanged]` but is not declared `partial`. | Add the `partial` keyword to the class declaration. |
| `AtcXamlToolkit0003` | Warning | `[ObservableProperty]` is on a field that is not `private` (e.g., `public`, `internal`, `protected`). The generator silently skips non-private fields. | Mark the field `private`. |
| `AtcXamlToolkit0004` | Warning | `[ObservableProperty]` is on a PascalCase field (e.g., `private string Name;`). The generator silently skips PascalCase fields because the inferred property name would collide. | Rename the field to camelCase (`name`). |
| `AtcXamlToolkit0005` | Warning | `[NotifyPropertyChangedFor("X")]` references a property `X` that is neither declared on the class nor generated by another `[ObservableProperty]` field. Today the user gets a confusing `nameof(X)` compile error inside the generated file; this diagnostic points back at the attribute argument. | Remove the reference, fix the typo, or declare the property. |
| `AtcXamlToolkit0006` | Warning | `[NotifyCanExecuteChangedFor("X")]` references a command `X` that is neither declared on the class nor generated by a `[RelayCommand]` method. Same shape as `0005`, but for commands. The diagnostic understands the `[RelayCommand]` naming rules (handler-suffix stripping, automatic `Command` suffix, custom name override). | Remove the reference, fix the typo, or declare the command. |

## Cross-references

- [`ViewModelBase` and validation](../Mvvm/Readme.md)
- [Commands overview](../Command/Readme.md)
- [Behaviors](../Behaviors/Readme.md)
