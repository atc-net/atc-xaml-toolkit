# Upgrading to version 4

Version 4 has one change that can stop existing code from compiling, and a number of places where the same code now behaves differently. Most of the behaviour changes are corrections of defects; they are listed so that code which relied on the old behaviour can be found.

## Breaking changes

### `ViewModelBase.ViewModelId` is an instance property

`ViewModelId` was a static property that returned a new `Guid` on every read, so it could not identify anything. It is now an instance property that keeps one `Guid` for the lifetime of the view model.

```csharp
// Before
var id = ViewModelBase.ViewModelId;

// After
var id = viewModel.ViewModelId;
```

### The `Atc` package is version 4

The toolkit packages depend on `Atc` 4.1.0 (was 3.0.186). A project that references `Atc` itself gets the new major version through the toolkit; see the `Atc` release notes for its own changes.

## Behaviour changes

### Commands

| What | Before | Now |
|------|--------|-----|
| Text given as the parameter of `RelayCommand<T>` / `RelayCommandAsync<T>` | Converted with the current culture, so `CommandParameter="1.5"` in XAML was read as 15 on a machine with a decimal comma | Converted with the invariant culture, as XAML literals are. Text typed by a user in the local format ("1,5") has to be converted before it is passed as a command parameter, for example by binding to a numeric property |
| `RelayCommandAsync<T>.CanExecute(parameter)` | The predicate always received `default(T)` | The predicate receives the parameter |
| `RelayCommand<T>.CanExecute` with a parameter of another type | Returned `false` for a value that `Execute` would have converted, such as `"5"` for an `int` | Converts the parameter the way `Execute` does |
| A parameter that cannot be converted to `T` | `RelayCommandAsync<T>.CanExecute` returned `true`; some conversions threw from `Execute` | `CanExecute` returns `false` when the command has a predicate, and `Execute` does nothing (or hands the error to the `IErrorHandler`) |
| Enum and `Nullable<T>` command parameters | Not converted from text or numbers | Converted. A number or name the enum does not define is rejected; a `[Flags]` enum takes a combination of defined names |
| `ExecuteAsync` called directly on an async command | Did not change `IsExecuting` | Sets `IsExecuting` and raises `PropertyChanged` and `CanExecuteChanged`, as `Execute` does |
| Overlapping runs of an async command | Shared one cancellation token source; a cancelled token could be reused | Each run has its own token; `Cancel()` cancels every run in progress |
| WPF: `CanExecuteChanged` on `RelayCommandAsync` without a `canExecute` | Subscriptions were dropped, so `IsExecuting` changes did not requery | Subscriptions are kept |

### Weak delegates and messaging

| What | Before | Now |
|------|--------|-----|
| An exception thrown by a weakly held instance delegate (`WeakAction`, `WeakFunc`) | Wrapped in `TargetInvocationException` | The original exception. A `catch` block or `IErrorHandler` that unwrapped it has to be adjusted |
| `IsStatic` on a generic weak delegate created from a static method | `false` | `true` |
| `Messenger` clean-up of dead recipients | Requested on every `Send` | Requested when a dead recipient is met; after 64 requests that could not be scheduled it runs on the calling thread |

### MVVM

| What | Before | Now |
|------|--------|-----|
| `ObservableValidator`: `PropertyChanged` for `HasErrors` | Raised on every validation | Raised when `HasErrors` changes |
| `ObservableValidator`: `ErrorsChanged` | Raised on every validation of a property | Raised when the errors of the property change; identical messages are reported once |
| Validation attributes | One set of attribute instances per view model instance | Shared by all instances of a type. This matters only for a custom `ValidationAttribute` that keeps state |
| `ObservableObject` property-name check (debug builds) | Threw for the indexer name `Item[]` | Accepts it |

### Converters

| What | Before | Now |
|------|--------|-----|
| `MultiValueConverterBase`: a `null` input for a type that can hold null | Rejected | Passed to `Convert` |
| `MultiValueConverterBase`: an exception thrown by `Convert` | Wrapped in `UnexpectedTypeException` | The original exception |
| WPF and Avalonia multi-bool converters with an unset input | Threw | Return the platform's unset value |
| `BoolToWidthValueConverter` parameter | Parsed with the current culture | Parsed with the invariant culture |

### Platform packages

| What | Before | Now |
|------|--------|-----|
| WPF `Dispatcher.BeginInvokeIfRequired`: exception thrown by the action on the dispatcher thread | Went to the dispatcher's unhandled-exception handling; the returned task completed normally | Carried by the returned task |
| WPF `Dispatcher.BeginInvokeIfRequired`: called on the dispatcher thread | Exceptions and null arguments were stored in the returned task | Thrown to the caller |
| WinUI `InvokeAsyncIfRequired` / `BeginInvokeIfRequired`: the queue rejects the work | The returned task never completed | The returned task is canceled |
| `KeyboardNavigationBehavior` | Took the focus back from a child that received it | Leaves the focus with the child |
| `AnimationBehavior` with `AutoStart` on an element that is already loaded | Did not play | Plays |
| `FocusBehavior`, `AnimationBehavior` (Avalonia): the same trigger value set again | Ignored | Fires again |
| WPF `BindingErrorTraceListener` | Could raise several events for one binding error, with partial text | One event per error with the full text. `StartTrace` enables WPF binding tracing for the process |
| WinUI `MainWindowViewModelBase` key handling | Marked the key as handled even when the window state did not change | Marks it handled only when the window state was toggled |

### Source generators

| What | Before | Now |
|------|--------|-----|
| `[RelayCommand]` `ParameterValue` / `ParameterValues` with text | The text was written into the generated code as it stood, so `"x"` became the identifier `x` | Written as a string literal (`"x"`); `null` is passed as `null` |
| `[RelayCommand]` on a `void` method with `AutoSetIsBusy` and `ExecuteOnBackgroundThread` | `IsBusy` was reset before the work had finished, and an exception thrown by the method was lost | `IsBusy` stays set until the work has finished. An exception thrown by the method is no longer lost: it reaches the application's unhandled-exception handling |
| `[RelayCommand]` on a `Task` method with `SupportsCancellation` and `ExecuteOnBackgroundThread` | `ExecuteOnBackgroundThread` was ignored | The method runs on a background thread |
| `[ComputedProperty]` that depends on another computed property | Not notified when the underlying property changed | Notified |
| `[ObservableDtoViewModel]`: a DTO property whose setter the view model cannot call (`private set`, `init`) | A setter was generated that did not compile | The property is read-only on the view model |
| `[ObservableDtoViewModel]`: generic methods of the DTO | Forwarded with code that did not compile | Not forwarded |
| Async `[RelayCommand]` in a control, behavior or helper class | No `DisposeCommands()` or `Cancel{Name}()` | Generated, as in a view model |
| Generated file names | `Namespace.Class.g.cs` | Include the containing types of a nested class and the arity of a generic class |
| A class that is not `partial` but uses `[DependencyProperty]`, `[AttachedProperty]`, `[RoutedEvent]`, `[DefaultStyleKey]` or `[TemplatePartField]` | Nothing was generated and nothing was reported | Warning `AtcXamlToolkit0002` |
| `[ObservableProperty]` in a partial class without `ObservableObject` or `[INotifyPropertyChanged]` | Nothing was generated and nothing was reported | Warning `AtcXamlToolkit0014` |
| Avalonia: `[StyledProperty]` of a nullable reference type | Registered as `StyledProperty<string>`, with a compiler warning in the generated file | Registered as `StyledProperty<string?>` |

A project that builds with warnings as errors will fail on the new warnings where the generator used to skip a class silently. In each case the class was not getting its generated members before either.

### XAML styler

The same input and options can give different output:

| What | Before | Now |
|------|--------|-----|
| A markup extension followed by more text (`{0} of {1}`, `{Binding A} x`) | The text after the first closing brace was removed | Written as it stands |
| A `"` inside a markup extension | Written as `"`, which is invalid XML | Written as `&quot;` |
| Spacing between text and inline elements in a `TextBlock` | `Hello <Run />` could lose its space | Kept |
| Spaces between block elements on one line | Kept, leaving trailing spaces | Removed |
| `ReorderVSM = Last` (the default) without trailing whitespace | The last child element was removed | Kept |
| Setter reordering and thickness formatting | WPF namespaces only | Also Avalonia, MAUI and Xamarin.Forms: `Value="1 2 3 4"` becomes `1,2,3,4` with the default options |
| `StyleFile` on a file that is not UTF-8 and has no byte order mark | Rewritten with replacement characters | Throws `DecoderFallbackException` and leaves the file alone |
| `FindConfigFile` with a relative path | Searched from the path as given | Searched from the full path, including the working directory and its parents; returns an absolute path |
| Options changed after the service was created | Reordering and thickness options were ignored | Take effect |
