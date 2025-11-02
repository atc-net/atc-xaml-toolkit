# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Repository Overview

This is the **Atc.XamlToolkit** repository, a cross-platform MVVM toolkit for building WPF, WinUI 3, and Avalonia applications. It targets .NET 9 and includes source generators to eliminate boilerplate code.

**Solution File:** The repository uses `Atc.XamlToolkit.slnx`, the new XML-based Visual Studio solution format introduced in Visual Studio 2022 17.11+. All dotnet CLI commands work normally with this format.

## Build Commands

```bash
# Restore NuGet packages
dotnet restore

# Build in Debug mode
dotnet build

# Build in Release mode (warnings treated as errors)
dotnet build -c Release

# Build NuGet packages
dotnet pack -c Release

# Run all tests
dotnet test

# Run tests in Release mode
dotnet test -c Release --no-build

# Run tests for a specific project
dotnet test test/Atc.XamlToolkit.Tests/
dotnet test test/Atc.XamlToolkit.Wpf.Tests/
dotnet test test/Atc.XamlToolkit.WinUI.Tests/
dotnet test test/Atc.XamlToolkit.Avalonia.Tests/
dotnet test test/Atc.XamlToolkit.SourceGenerators.Tests/

# Run sample applications (for testing/debugging)
dotnet run --project sample/Atc.XamlToolkit.WpfSample/
dotnet run --project sample/Atc.XamlToolkit.WinUISample/
dotnet run --project sample/Atc.XamlToolkit.AvaloniaSample/
```

## Project Structure

The repository is organized into distinct platform-specific and shared packages:

### Core Projects (`./src/`)

- **Atc.XamlToolkit** - Base package with platform-agnostic MVVM infrastructure:
  - `ViewModelBase`, `ObservableObject`
  - Command interfaces and base implementations
  - Messaging system (Messenger, GenericMessage, etc.)
  - Data converters base classes
  - Performance utilities (BooleanBoxes, WeakAction)

- **Atc.XamlToolkit.Wpf** - WPF-specific implementation:
  - WPF command implementations (RelayCommand, RelayCommandAsync)
  - WPF-specific behaviors (EventToCommandBehavior, AnimationBehavior, FocusBehavior, KeyboardNavigationBehavior)
  - WPF value converters
  - MainWindowViewModelBase for WPF lifecycle management

- **Atc.XamlToolkit.WinUI** - WinUI 3-specific implementation:
  - WinUI command implementations
  - WinUI behaviors
  - WinUI value converters
  - MainWindowViewModelBase for WinUI lifecycle

- **Atc.XamlToolkit.Avalonia** - Avalonia-specific implementation:
  - Avalonia command implementations
  - Avalonia behaviors
  - Avalonia value converters
  - MainWindowViewModelBase for Avalonia lifecycle

- **Atc.XamlToolkit.SourceGenerators** - Roslyn source generators (netstandard2.0):
  - ViewModelGenerator - Processes `[ObservableProperty]`, `[ComputedProperty]`, `[RelayCommand]`
  - ObservableDtoViewModelGenerator - Processes `[ObservableDtoViewModel]`
  - FrameworkElementGenerator - Processes `[DependencyProperty]`, `[AttachedProperty]`, `[RoutedEvent]` (WPF only)

### Test Projects (`./test/`)

Each source project has a corresponding test project using xUnit, NSubstitute, FluentAssertions, and AutoFixture.

### Sample Projects (`./sample/`)

Sample applications demonstrating the toolkit for WPF, WinUI, and Avalonia.

**Sample Project Structure:**
- Each sample follows a consistent pattern with auto-discovery of views
- Views are organized in `SampleControls/[Category]/` folders (e.g., `SampleControls/FrameworkElements/`, `SampleControls/Behaviors/`)
- Component controls go in `[Category]Components/` subfolders
- MainWindowViewModel uses reflection to discover and display all views ending with "View" in `SampleControls` namespace
- Views automatically appear in TreeView navigation without manual registration

**Naming Conventions:**
- Views: `[Feature]View.xaml` and `[Feature]View.xaml.cs` (e.g., `AttachedPropertyView`, `DependencyPropertyView`)
- Components: `[ComponentName].xaml/.cs` in `[Category]Components/` folder
- Namespace: `Atc.XamlToolkit.[Platform]Sample.SampleControls.[Category]`
- Components namespace: `Atc.XamlToolkit.[Platform]Sample.SampleControls.[Category]Components`

**Platform XAML Differences:**
- WPF: Uses `clr-namespace:` for XML namespace declarations, no `Spacing` property on StackPanel
- WinUI: Uses `using:` for XML namespace declarations, has `Spacing` property
- Avalonia: Uses `clr-namespace:` and `using:` interchangeably, has `Spacing` property

## Architecture

### MVVM Pattern

The toolkit is built around the MVVM pattern with the following core components:

1. **ViewModelBase** - Base class for all ViewModels with:
   - INotifyPropertyChanged implementation via ObservableObject
   - INotifyDataErrorChanged implementation with validation support
   - Built-in properties: IsEnable, IsVisible, IsBusy, IsDirty, IsSelected
   - Messenger integration for decoupled communication
   - Validation infrastructure using DataAnnotations
   - All ViewModelBase properties are marked with `[JsonIgnore]` to prevent serialization issues

2. **ObservableObject** - Lightweight base class providing INotifyPropertyChanged

3. **Commands** - Command infrastructure with:
   - Synchronous: `RelayCommand`, `RelayCommand<T>`
   - Asynchronous: `RelayCommandAsync`, `RelayCommandAsync<T>` (with cancellation token support)
   - Base classes in core package, platform-specific implementations in platform packages
   - IErrorHandler interface for centralized error handling

4. **Messaging** - Pub/sub messaging system for decoupled communication:
   - Messenger (singleton or instance)
   - GenericMessage<T>, NotificationMessage, PropertyChangedMessage<T>
   - Weak references to prevent memory leaks

### Source Generator System

The source generators use Roslyn to analyze code and generate boilerplate at compile-time:

**Key Components:**
- **Inspectors** - Analyze syntax nodes to detect attributes and gather metadata
- **Builders** - Generate C# source code from metadata
- **Models** - Data structures representing the metadata for code generation

**Generated Code Patterns:**
- Properties from fields: `[ObservableProperty] private string name;` → `public string Name { get; set; }`
- Commands from methods: `[RelayCommand] private void Execute()` → `public IRelayCommand ExecuteCommand { get; }`
- Computed properties: `[ComputedProperty]` automatically detects dependencies and generates notifications
- DTO ViewModels: `[ObservableDtoViewModel]` wraps DTOs with INotifyPropertyChanged, IsDirty tracking, and validation

**Source Generator Attributes:**
- Located in `src/Atc.XamlToolkit/Mvvm/Attributes/`
- All attributes are runtime attributes (not compile-time only)
- Support callbacks, dependency tracking, and broadcasting changes via Messenger

### Platform Separation and Source Generator Platform Detection

Each platform package (Wpf, WinUI, Avalonia) has:
- Platform-specific command implementations (different ICommand interfaces)
- Platform-specific behaviors (using Microsoft.Xaml.Behaviors.* libraries)
- Platform-specific value converters (different IValueConverter interfaces)
- Shared namespace: All use `Atc.XamlToolkit` namespace via RootNamespace setting

**Critical Platform-Specific Features:**

1. **DependencyProperty / StyledProperty:**
   - WPF/WinUI: Use `[DependencyProperty]` → generates `DependencyProperty` with `FrameworkPropertyMetadata`
   - Avalonia: Use `[StyledProperty]` → generates `StyledProperty<T>` with `Avalonia.AvaloniaProperty.Register<>()`
   - Avalonia uses plain bool values (no BooleanBoxes) and registers callbacks in static constructor via `.Changed.AddClassHandler<T>()`

2. **AttachedProperty:**
   - WPF/WinUI: Static owner class allowed
   - Avalonia: Owner class MUST be non-static and inherit from `AvaloniaObject`
   - Avalonia doesn't use BooleanBoxes for boolean setters

3. **RoutedEvent (WPF ONLY):**
   - WPF: Full support with `EventManager.RegisterRoutedEvent()`, routing strategies (Bubble/Tunnel/Direct)
   - WinUI 3: NOT supported - no `RoutedEvent` or `EventManager` classes exist
   - Avalonia: NOT supported - different event system, cannot create custom routed events
   - Source generator automatically skips routed event generation for non-WPF platforms (see `FrameworkElementGenerator.cs:195-204`)
   - Use standard .NET events for WinUI and Avalonia instead

4. **WinUI Threading and DispatcherQueue:**
   - **Critical**: `DispatcherQueue.GetForCurrentThread()` returns `null` when called from non-UI threads
   - Source generator captures DispatcherQueue **once at the start** of command execution (when on UI thread)
   - The captured DispatcherQueue is reused in try/finally blocks for `AutoSetIsBusy` property updates
   - This prevents `RPC_E_WRONG_THREAD` (0x8001010E) COM exceptions when PropertyChanged events trigger x:Bind updates
   - Implementation in `CommandBuilderBase.cs`: `BuildExecuteExpressionForAutoSetIsBusy()` and `AppendDispatcherInvokeAsync()`
   - WPF and Avalonia don't have this issue as their dispatchers handle cross-thread calls differently

5. **IsExecuting Property Binding Support:**
   - **All async commands now implement `INotifyPropertyChanged`** to support `IsExecuting` property bindings
   - `RelayCommandAsyncBase` and `RelayCommandAsyncBase<T>` raise `PropertyChanged` when `IsExecuting` changes
   - **WinUI-specific implementation**:
     - `RelayCommandAsync` and `RelayCommandAsync<T>` override `OnPropertyChanged()` to marshal events to UI thread
     - Uses `DispatcherQueue.TryEnqueue()` when called from background threads to prevent cross-thread exceptions
     - **Critical for x:Bind**: WinUI's compiled bindings only subscribe to PropertyChanged on the root ViewModel
     - Source generator creates special properties for WinUI async commands that re-raise PropertyChanged on the ViewModel when `IsExecuting` changes
     - This allows `{x:Bind ViewModel.DoStuffCommand.IsExecuting, Mode=OneWay}` to work correctly
   - **WPF/Avalonia**: Standard `{Binding}` automatically subscribes to intermediate objects in property paths
   - Implementation: `CommandBuilderBase.cs:AppendPublicProperties()` generates full properties for WinUI async commands

**Platform Detection:**
- Source generators use `XamlPlatform` enum (Wpf, WinUI, Avalonia) to generate platform-appropriate code
- Detection logic in `FrameworkElementInspector.GetXamlPlatform()` based on project references

**Class Naming Requirements for FrameworkElementGenerator:**
- Classes using `[DependencyProperty]`, `[AttachedProperty]`, `[StyledProperty]`, or `[RoutedEvent]` must:
  - Inherit from `UserControl`, `DependencyObject`, `FrameworkElement`, OR
  - Have a class name ending with **"Attach"**, **"Behavior"**, or **"Helper"**, OR
  - Contain `[RoutedEvent]` attributes
- This requirement is enforced by `HasAnythingAroundFrameworkElement()` in `ClassDeclarationSyntaxExtensions.cs`
- Common patterns: `DragBehavior`, `WatermarkTextBoxBehavior`, `CheckBoxHelper`

## Code Style and Rules

This repository uses the **ATC coding rules** (v1.0.0) defined in `.editorconfig`:

- **C# 13.0** with **nullable reference types enabled**
- **File-scoped namespaces** (IDE0160)
- **var** for all types (IDE0007/IDE0008)
- **Expression-bodied members** preferred
- **Private fields** use camelCase
- **Public members** use PascalCase
- **Interfaces** prefixed with 'I'
- **Generic type parameters** prefixed with 'T'
- **No public/protected fields** except const/static readonly
- **4 spaces** indentation (except for specific file types)

**Analyzer Configuration:**
- Warnings are treated as errors in Release builds
- Multiple analyzers active: AsyncFixer, Asyncify, Meziantou, StyleCop, SonarAnalyzer
- AnalysisLevel: latest-All
- EnforceCodeStyleInBuild: true

**Important Suppressions:**
- IDE0058: Expression value is never used (disabled)
- SA1101: Prefix local calls with this (disabled)
- SA1200-SA1204: Ordering rules (disabled)
- CA1515: Make types internal (disabled for this library)

## Testing

- **Test Framework**: xUnit with Xunit.StaFact for UI thread tests
- **Mocking**: NSubstitute
- **Assertions**: FluentAssertions
- **Test Data**: AutoFixture with AutoFixture.Xunit2
- **Coverage**: coverlet.collector
- Tests for source generators use `Microsoft.CodeAnalysis.CSharp` to verify generated code

## Source Generator Development

When working on source generators:

1. **Target Framework**: netstandard2.0 (required for Roslyn analyzers)
2. **Testing**: Use `Atc.XamlToolkit.SourceGenerators.Tests` project
3. **Debugging**: Attach to a separate Visual Studio instance or use the sample projects
4. **Key Patterns**:
   - Inspectors detect and validate attributes on fields/methods/properties
   - Builders construct code using StringBuilder and CodeAnalysis utilities
   - Models store metadata extracted by inspectors
   - Extensions provide reusable CodeAnalysis operations

5. **Source Generator Architecture**:
   - `FrameworkElementGenerator` handles DependencyProperty, AttachedProperty, and RoutedEvent generation
   - `ViewModelGenerator` handles ObservableProperty, ComputedProperty, and RelayCommand generation
   - `ObservableDtoViewModelGenerator` handles DTO wrapper generation
   - All generators in `src/Atc.XamlToolkit.SourceGenerators/Generators/`

6. **Key Files**:
   - Inspectors: `src/Atc.XamlToolkit.SourceGenerators/Inspectors/`
   - Builders: `src/Atc.XamlToolkit.SourceGenerators/Extensions/Builder/`
   - Models: `src/Atc.XamlToolkit.SourceGenerators/Models/`
   - Generated code appears in `obj/Debug/net9.0[-windows]/generated/Atc.XamlToolkit.SourceGenerators/`

7. **Platform-Aware Generation**:
   - Always check `XamlPlatform` enum when generating platform-specific code
   - Use conditional logic for features that differ between platforms (e.g., BooleanBoxes, callback registration)
   - Validate that WPF-only features (like RoutedEvent) don't generate code for other platforms

8. **Viewing Generated Code**:
   - Generated files are in `obj/[Configuration]/[TargetFramework]/generated/Atc.XamlToolkit.SourceGenerators/`
   - For WPF projects: `obj/Debug/net9.0-windows/generated/Atc.XamlToolkit.SourceGenerators/`
   - For WinUI projects: `obj/Debug/net9.0-windows10.0.19041.0/generated/Atc.XamlToolkit.SourceGenerators/`
   - For Avalonia projects: `obj/Debug/net9.0/generated/Atc.XamlToolkit.SourceGenerators/`
   - After changes to source generators, run `dotnet build-server shutdown` to force regeneration

## Important Conventions

### ViewModelBase Properties

All built-in ViewModelBase properties (IsEnable, IsVisible, IsBusy, IsDirty, IsSelected) are marked with `[System.Text.Json.Serialization.JsonIgnore]` to prevent serialization issues when ViewModels are serialized.

### Validation

ViewModels support DataAnnotations validation:
- Call `InitializeValidation()` in constructor to enable
- Validation attributes can be on properties or backing fields (for source-generated properties)
- `ValidateProperty()` validates individual properties
- `ValidateAllProperties()` validates all properties
- `HasErrors` property indicates validation state

### Commands with Cancellation

Async commands support cancellation tokens:
- `RelayCommandAsync` can accept a `CancellationToken` parameter
- Commands automatically disable during execution
- `[RelayCommand(SupportsCancellation = true)]` generates cancel methods and cancel command properties
- When combined with `AutoSetIsBusy = true`, generates:
  - Automatic `IsBusy` property management with Dispatcher marshalling
  - `{CommandName}CancelCommand` property for XAML binding
  - `Cancel{CommandName}()` method for programmatic cancellation
  - Thread-safe UI updates via `Dispatcher.InvokeAsyncIfRequired()`
- Works with parameterized commands: generator creates correct lambda signatures (e.g., `async (query, cancellationToken) =>`)
- See `docs/Command/AsyncCommandCancellation.md` for details and comprehensive examples

### ObservableDtoViewModel

The `[ObservableDtoViewModel]` attribute generates wrapper ViewModels for DTOs:
- `IsDirty` property tracks changes
- `InnerModel` provides access to underlying DTO
- Supports `IgnorePropertyNames` and `IgnoreMethodNames` for selective generation
- `EnableValidationOnPropertyChanged` and `EnableValidationOnInit` copy validation attributes
- Works with `[ComputedProperty]` for automatic dependency detection

## Documentation

Comprehensive documentation is in the `./docs/` folder:
- Behaviors documentation in `docs/Behaviors/`
- Command documentation in `docs/Command/`
- Messaging documentation in `docs/Messaging/`
- MVVM documentation in `docs/Mvvm/`
- Source generator documentation in `docs/SourceGenerators/`
- Value converters documentation in `docs/ValueConverters/`

Refer to these when implementing new features or fixing bugs.
