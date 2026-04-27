# 🛠 Source Generators

`Atc.XamlToolkit` ships three Roslyn source generators that eliminate boilerplate around MVVM and XAML framework-element plumbing. They run at compile time, work in the IDE for live IntelliSense, and target `netstandard2.0` so they load in any .NET workload.

## Decision tree

| You want to… | Use | Lives in | Page |
|---|---|---|---|
| Add `INotifyPropertyChanged` to a class that can't inherit `ObservableObject` | `[INotifyPropertyChanged]` | Plain class | [../Mvvm/Readme.md#-inpc-on-plain-classes-with-inotifypropertychanged](../Mvvm/Readme.md#-inpc-on-plain-classes-with-inotifypropertychanged) |
| Generate `INotifyPropertyChanged` properties from fields | `[ObservableProperty]` | View model | [ViewModel.md](ViewModel.md) |
| Generate `ICommand` properties from methods | `[RelayCommand]` | View model | [ViewModel.md](ViewModel.md) |
| Track a property that depends on others | `[ComputedProperty]` | View model | [ViewModel.md](ViewModel.md) |
| Validate a property inline on every set | `[NotifyDataErrorInfo]` | View model field | [ViewModel.md](ViewModel.md#-inline-validation-with-notifydataerrorinfo) |
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
| `[NotifyDataErrorInfo]` | ✅ | ✅ | ✅ | Field-level companion to `[ObservableProperty]`. Emits an inline `ValidateProperty(value, nameof(...))` call in the setter so listeners see the up-to-date `HasErrors` state. Class must derive from `ObservableValidator` (or `ViewModelBase`). |
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
| `AtcXamlToolkit0007` | Warning | `[ComputedProperty]` is on a property whose getter doesn't reference any other property on the class — the generator silently filters such cases out, so the attribute does nothing. | Reference at least one `[ObservableProperty]` field or another property in the getter, or remove `[ComputedProperty]`. |
| `AtcXamlToolkit0008` | Warning | Two or more `[ComputedProperty]` getters mutually reference each other (`A => B + 1` and `B => A + 1`, or any longer cycle). The generator's incremental invalidation graph doesn't trigger an update loop, but evaluating any of them at runtime infinitely recurses until the stack overflows. The diagnostic message includes the shortest cycle path. | Break the cycle by extracting shared computation into a non-computed helper, or by making one of the properties an `[ObservableProperty]`-backed value. |
| `AtcXamlToolkit0009` | Warning | `[NotifyDataErrorInfo]` is on a field whose containing class doesn't transitively derive from `ObservableValidator` (the only base type that exposes the protected `ValidateProperty` method the generator calls in the setter). Combinations like `[INotifyPropertyChanged]` (plain class) + `[NotifyDataErrorInfo]` would otherwise produce an opaque `CS0103: name 'ValidateProperty' does not exist` inside the generated file. | Change the base class to `ObservableValidator` or `ViewModelBase`, or remove `[NotifyDataErrorInfo]`. |
| `AtcXamlToolkit0010` | Warning | `[RoutedEvent]` is on a field in a project that targets WinUI 3 or Avalonia. WPF's `RoutedEvent` / `EventManager` infrastructure does not exist on those platforms, so the generator skips routed-event emission — the field would otherwise look like it should produce an event but produce nothing. | Replace the field with a standard CLR event (e.g. `public event RoutedEventHandler ItemSelected;`) or remove the attribute. |

## Architecture & decisions

A small set of decisions about the diagnostics infrastructure — surfaced here so they don't have to be re-derived from git history.

### Where the diagnostics live

Toolkit-specific diagnostics — every `AtcXamlToolkit####` ID — ship from `Atc.XamlToolkit.SourceGenerators`. They're not in the external [`Atc.Analyzer`](https://www.nuget.org/packages/Atc.Analyzer) package, and they won't be:

- These diagnostics *only* make sense in the presence of toolkit attributes (`[ObservableProperty]`, `[RelayCommand]`, `[ComputedProperty]`, `[NotifyDataErrorInfo]`, `[INotifyPropertyChanged]`, …). Outside this package they describe nothing.
- `Atc.Analyzer` ships general C#/style rules (ATC1xx / ATC2xx) that apply across all .NET projects. Mixing toolkit-domain rules into it would force consumers to either accept domain-specific noise on unrelated projects, or maintain rule-suppression baselines per project type.
- Versioning is cleaner: a diagnostic can ship in the same release as the feature it guards. No cross-package coordination.

### Diagnostic ID convention

`AtcXamlToolkit####`, where `####` is a zero-padded sequential 4-digit suffix. Reserve a fresh ID at the time you add the diagnostic descriptor in `DiagnosticFactory.cs`; never recycle an ID, even if the diagnostic is later removed (consumers have suppressions in `.editorconfig` keyed to the ID).

The ID range is exclusive to this toolkit. There is no shared registry to consult — local uniqueness inside `DiagnosticFactory` is enough.

### How to add a new diagnostic

The existing pipelines in `ViewModelGenerator.cs` are the template. The pattern is:

1. Add a `DiagnosticDescriptor` to `DiagnosticFactory.cs` with the new ID and a `Create<NewName>` factory method.
2. Add a `SyntaxProvider` pipeline in `ViewModelGenerator.Initialize` consisting of a `predicate` (cheap syntax-shape filter — keep this purely syntactic, no semantic-model use) and a `transform` (semantic check that returns the diagnostic, or a list of them, or null).
3. Register the pipeline output via `context.RegisterSourceOutput(...)` and call `spc.ReportDiagnostic(...)` per emitted diagnostic.
4. Document it in the table above with severity, when-it-fires, and how-to-fix.
5. Add positive + negative tests in `ViewModelGeneratorTests.cs` that assert by `d.Id == "AtcXamlToolkit####"`.

### Code-fix providers (deferred)

Code-fix providers (the IDE light-bulb that *applies* a fix when a diagnostic fires) are deferred until there's user demand. The blocker isn't engineering — it's testing infrastructure: the standard `Microsoft.CodeAnalysis.CSharp.CodeFix.Testing.CSharpCodeFixVerifier<TAnalyzer, TCodeFix>` expects a `DiagnosticAnalyzer` as the first type parameter, not a source generator. Source-generator-emitted diagnostics flow through a different pipeline.

Two paths around this once we want the feature:

1. Ship a parallel `DiagnosticAnalyzer` that re-detects all 9 conditions, just to plug into the standard verifier. Real cost: every detection pipeline lives in two places.
2. Build a custom test harness on top of the existing `RunGenerator` infrastructure that feeds diagnostics into the code-fix engine manually.

Either is workable. Neither is justified by the build-time-warning UX we already ship — every existing diagnostic message names the fix in plain language.

## Cross-references

- [`ViewModelBase` and validation](../Mvvm/Readme.md)
- [Commands overview](../Command/Readme.md)
- [Behaviors](../Behaviors/Readme.md)
