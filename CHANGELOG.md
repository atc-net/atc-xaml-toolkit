# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/) and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.1] — 2026-04-27

### Added

#### MVVM

- Weak event listener helpers for `INotifyPropertyChanged` and `INotifyCollectionChanged` so consumers can subscribe to event sources without keeping them alive past their normal lifetime.
- `IRecipient<T>` registration pattern on `Messenger`, complementing the existing action-based registration.

#### Source generators — new attributes

- `[INotifyPropertyChanged]` — class-level attribute that adds INPC scaffolding (`PropertyChanged` event, `RaisePropertyChanged`, `OnPropertyChanged`, `Set<T>`) to a partial class without inheriting from `ObservableObject`. Composes with `[ObservableProperty]` on the same class.
- `[NotifyCanExecuteChangedFor]` — like `[NotifyPropertyChangedFor]` but for command `CanExecute` re-evaluation.
- `[NotifyDataErrorInfo]` — opts a generated property setter into inline validation via `ObservableValidator.ValidateProperty`.

#### Source generators — new attribute options

- `[ObservableProperty(GeneratePartialHooks = true)]` — emits `partial void On{Name}Changing/Changed({Type} value)` declarations and unconditional setter calls.
- `[ObservableProperty(IsRequired = true)]` — generates the `required` keyword on the produced property.
- `[ObservableProperty(GenerateDocumentation = true)]` — emits a default `/// <summary>Gets or sets the {Name}.</summary>` on the generated property when no field-level XML doc is present. Also available on `[RelayCommand]`, `[DependencyProperty]`, `[AttachedProperty]`, and Avalonia's `[StyledProperty]`.
- `[assembly: GenerateDocumentationDefault]` — flips the `GenerateDocumentation` default for every supported attribute in the assembly. Per-attribute `GenerateDocumentation = false` still wins as an explicit opt-out.

#### Source-generator diagnostics (new IDs)

| Id | Severity | Triggers |
|---|---|---|
| `AtcXamlToolkit0002` | Warning | A class uses `[ObservableProperty]` / `[RelayCommand]` / `[ComputedProperty]` / `[INotifyPropertyChanged]` but isn't `partial`. |
| `AtcXamlToolkit0003` | Warning | `[ObservableProperty]` field is not `private`. |
| `AtcXamlToolkit0004` | Warning | `[ObservableProperty]` field is PascalCase. |
| `AtcXamlToolkit0005` | Warning | `[NotifyPropertyChangedFor("X")]` references a property that doesn't exist. |
| `AtcXamlToolkit0006` | Warning | `[NotifyCanExecuteChangedFor("X")]` references a command that doesn't exist. |
| `AtcXamlToolkit0007` | Warning | `[ComputedProperty]` getter has no detected dependencies. |
| `AtcXamlToolkit0008` | Warning | `[ComputedProperty]` getters mutually recurse — diagnostic message includes the shortest cycle. |
| `AtcXamlToolkit0009` | Warning | `[NotifyDataErrorInfo]` is on a class that doesn't transitively derive from `ObservableValidator`. |
| `AtcXamlToolkit0010` | Warning | `[RoutedEvent]` is on a WinUI 3 or Avalonia compilation (WPF-only attribute). |

#### Diagnostics

- `BindingErrorTraceListener` ported to **WinUI 3** (via `DebugSettings.BindingFailed`) and **Avalonia** (via `Avalonia.Logging.ILogSink` filtered for `LogArea.Binding`). All three platforms expose a unified `BindingErrorOccurred` event with shared `BindingErrorEventArgs`.
- WPF `BindingErrorTraceListener` gained the same `BindingErrorOccurred` event and an opt-out `ShowMessageBoxOnError` property (still defaults to `true` for backward compatibility).
- `docs/Diagnostics/BindingErrorTraceListener.md` documents the cross-platform surface.

#### Avalonia

- No-priority and `BeginInvoke` overloads on `DispatcherExtensions` for parity with WPF.

#### Tooling

- VS Code snippets in `snippets/vscode/atc-xaml-toolkit.code-snippets` (16 entries) and Visual Studio XML snippets in `snippets/visual-studio/` covering `[ObservableProperty]`, `[ComputedProperty]`, `[RelayCommand]`, `[DependencyProperty]`, `[AttachedProperty]`, `[StyledProperty]`, `[RoutedEvent]`, `[INotifyPropertyChanged]`, and `[ObservableDtoViewModel]`.

#### Samples

- Counter command demo, login form validation demo, and Messenger demo across all three platforms (WPF / WinUI / Avalonia).

### Changed

- **`ObservableValidator` extracted** from `ViewModelBase` so consumers can opt into `INotifyDataErrorInfo` validation without inheriting the full `ViewModelBase` surface. `ViewModelBase` now derives from `ObservableValidator`, so existing consumers see no behavioural change.
- **Messenger `WeakAction`/`WeakFunc`** snapshot their fields before invoke to avoid concurrent NREs from a parallel cleanup pass.
- **Messenger type-check chain** collapsed to a single `IsAssignableFrom` call.
- **Source-generator pipeline models** now use record-with-`EquatableArray<T>` value equality so the incremental cache works as designed — equivalent compilations no longer re-trigger downstream code emission.
- **Generator class layout refactor**: `FrameworkElementGenerator`, `ObservableDtoViewModelGenerator`, and `ViewModelGenerator` now contain only the main flow (`Initialize` / `Execute`). Predicate, transform, and diagnostic-pipeline registration live in dedicated `Generators/Helpers/` files. The `ViewModelDiagnosticHelper.RegisterAll(context)` one-liner registers all six diagnostic pipelines.

### Fixed

- **Messenger** `RequestCleanup` race and the `Default`/`Reset` interleave window.
- **Messenger** double-dispatch under concurrent register-during-send and lock contention reduced.
- **`RelayCommand<T>`** parameter coercion now guards `Convert.ChangeType` so unsupported conversions return without throwing.
- **`EventToCommandBehavior`** stabilised across WPF / WinUI / Avalonia (handler attach/detach lifetime parity).
- **Avalonia 12** removed `GotFocusEventArgs`; switched to `FocusChangedEventArgs`.
- **`ViewModelBase.ValidateAllProperties()`** missing lazy-init of the validation cache (caught by a backing-field test added in this branch).

### Performance

- `PropertyChangedEventArgs` cached per property name — saves an allocation per `RaisePropertyChanged` call.
- `AnimationBehavior` reuses two frozen `CubicEase` instances (WPF). WinUI deferred pending runtime verification.
- Source-generator pipeline models now have value equality (see *Changed*) — second-run compilations skip code emission for unchanged types.

### Documentation

- New: `docs/Command/ErrorHandling.md`, `docs/Command/AsyncCommandCancellation.md`, `docs/Diagnostics/BindingErrorTraceListener.md`, `docs/Mvvm/Readme.md` MainWindowViewModelBase guide, `docs/SourceGenerators/Readme.md` (architecture + diagnostics index).
- Enriched XML docs on `MainWindowViewModelBase`, the WinUI async commands, and `ObservableDtoViewModel`.
- `docs/SourceGenerators/AttachedProperty.md` and `DependencyProperty.md` document the GenerateDocumentation extension.
- `docs/SourceGenerators/RoutedEvents.md` updated to describe the new `AtcXamlToolkit0010` diagnostic.
- `snippets/README.md` per-IDE install guide.

### Tests

- Source-generator coverage: incremental-cache canary harness, generic-typed `[ObservableProperty]` fields (`List<T>`, nested `Dictionary<,List<>>`), primary-constructor classes, partial-class-split-across-files, `private readonly` fields, the `[INotifyPropertyChanged]` compile-integration test.
- Messaging: multi-recipient dispatch, token-based actions, GC'd-recipient handler-not-fired, concurrent register/send, registration-during-dispatch (the latter two uncovered the `WeakAction` / `WeakFunc` TOCTOU NREs fixed in this release).
- MVVM: `ObservableValidatorTests.cs` covers `InitializeValidation`, `ValidateAllProperties`, the `[NotifyDataErrorInfo]` lazy-init path, error/clear loops, and backing-field validation. `WeakEventListenerTests.cs` pins INPC/INCC unsubscribe semantics.
- Diagnostics: 5 Avalonia tests for `BindingErrorTraceListener` (forward, filter, level threshold, sink restore, idempotent close), 4 framework-element tests for `[RoutedEvent]` non-WPF diagnostic, 3+4 tests for `[ObservableProperty]` / `[RelayCommand]` `GenerateDocumentation`.
- Final tally: **1114 / 1114 passing**, Release build (warnings-as-errors) clean.

### Internal

- `EquatableArray<T>` adapted from CommunityToolkit.Mvvm — used as the value-equal collection wrapper for source-generator pipeline models.
- Pipeline lambdas are `static` and carry `WithTrackingName(...)` annotations so the cache verdict is observable in tests.

[3.1]: https://github.com/atc-net/atc-xaml-toolkit/compare/v3.0...v3.1
