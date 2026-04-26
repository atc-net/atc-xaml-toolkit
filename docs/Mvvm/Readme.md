# 🧱 MVVM in WPF, WinUI, and Avalonia

## 🖼️ For WPF

Windows Presentation Foundation (WPF) fully supports the **Model-View-ViewModel (MVVM)** pattern, which promotes a clear separation of concerns between the UI and business logic.

The **Atc.XamlToolkit.Wpf** library provides a robust foundation for implementing MVVM effectively, reducing boilerplate code and simplifying development.

## 🪟 For WinUI

Windows UI Library 3 (WinUI 3) fully supports the **Model-View-ViewModel (MVVM)** pattern, which promotes a clear separation of concerns between the UI and business logic.

The **Atc.XamlToolkit.WinUI** library provides a robust foundation for implementing MVVM effectively, reducing boilerplate code and simplifying development.

## 🌐 For Avalonia

Avalonia fully supports the **Model-View-ViewModel (MVVM)** pattern, which promotes a clear separation of concerns between the UI and business logic.

The **Atc.XamlToolkit.Avalonia** library provides a robust foundation for implementing MVVM effectively, reducing boilerplate code and simplifying development.

## ⚙️ Features

The `Atc.XamlToolkit.Wpf`, `Atc.XamlToolkit.WinUI`, or `Atc.XamlToolkit.Avalonia` library offers a variety of base classes and utilities to streamline MVVM implementation:

| 🧩 Component                   | 📋 Description                                                                |
|--------------------------------|--------------------------------------------------------------------------------|
| `ViewModelBase`                | A base class for ViewModels.                                                   |
| `MainWindowViewModelBase`      | A base class for the main window ViewModel.                                    |
| `ViewModelDialogBase`          | A base class for dialog ViewModels.                                            |
| `ObservableValidator`          | A base class with `INotifyPropertyChanged` + `INotifyDataErrorInfo` (no UI-state members or messenger). |
| `ObservableObject`             | A base class for observable objects implementing `INotifyPropertyChanged`.     |
| `[INotifyPropertyChanged]`     | An attribute that adds INPC scaffolding to a class you can't make inherit from `ObservableObject`. |
| `RelayCommand`                 | A command supporting `CanExecute`.                                             |
| `RelayCommand<T>`              | A command with a generic parameter and `CanExecute`.                           |
| `RelayCommandAsync`            | An asynchronous command supporting `CanExecute`.                               |
| `RelayCommandAsync<T>`         | An asynchronous command with a generic parameter and `CanExecute`.             |

📖 For detailed information about commands, refer to the [RelayCommand documentation](../SourceGenerators/ViewModel.md).

📖 For cancellation token support in async commands, see the [Async Command Cancellation](../Command/AsyncCommandCancellation.md) guide.

📖 For wrapping DTOs with ViewModels, see the [ObservableDtoViewModel documentation](../SourceGenerators/ViewModel.md#-wrapping-dtos-with-observabledtoviewmodel).

💡 **Tip:** The `ObservableDtoViewModel` generator automatically adds `IsDirty` tracking to your ViewModels when inheriting from `ViewModelBase`, helping you track changes in your forms and data. See the [Change Tracking with IsDirty](../SourceGenerators/ViewModel.md#-change-tracking-with-isdirty) section for more details.

---

## 🧬 INPC on plain classes with `[INotifyPropertyChanged]`

When your class needs `INotifyPropertyChanged` but **cannot** inherit from `ObservableObject` or `ViewModelBase` — for example, a domain entity that already has a base class, a DTO produced by another tool, or a third-party type you partial-extend — annotate the class with `[INotifyPropertyChanged]` and the source generator emits the INPC scaffolding directly on it.

### What gets generated

Given:

```csharp
using Atc.XamlToolkit.Mvvm;

[INotifyPropertyChanged]
public partial class Customer
{
}
```

The generator emits a partial that adds:

| Member | Visibility | Purpose |
|---|---|---|
| `event PropertyChangedEventHandler? PropertyChanged` | `public` | The INPC contract. |
| `RaisePropertyChanged([CallerMemberName] string?)` | `protected` | Fires `PropertyChanged` for the caller's property. Compatible with source-generated `[ObservableProperty]` setters that emit `RaisePropertyChanged(...)` calls. |
| `OnPropertyChanged([CallerMemberName] string?)` | `protected` | Alias for `RaisePropertyChanged` — convenience for hand-written setters. |
| `Set<T>(ref T field, T newValue, [CallerMemberName] string?)` | `protected` | Equality-checked setter helper. Returns `true` if the value changed and `PropertyChanged` was fired. |

The generated `RaisePropertyChanged` shares the process-wide [`PropertyChangedEventArgsCache`](../../src/Atc.XamlToolkit/Mvvm/PropertyChangedEventArgsCache.cs), so allocation behaviour matches `ObservableObject`.

### Combining with `[ObservableProperty]`

`[INotifyPropertyChanged]` is fully compatible with `[ObservableProperty]` on fields — you can opt a class into INPC and still use the field-to-property generator without any inheritance:

```csharp
using Atc.XamlToolkit.Mvvm;

[INotifyPropertyChanged]
public partial class Customer
{
    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    private string lastName = string.Empty;

    public string FullName => $"{FirstName} {LastName}";
}
```

The `[ObservableProperty]` setter emits `RaisePropertyChanged(nameof(FirstName))` calls that resolve against the `RaisePropertyChanged` method generated by `[INotifyPropertyChanged]`. Two generated files combine into the single partial class.

### When to choose what

| Scenario | Recommended |
|---|---|
| New ViewModel, full toolkit features (commands, messenger, IsDirty, validation) | Inherit from `ViewModelBase` |
| New ViewModel, INPC + validation only, no UI-state or messenger | Inherit from `ObservableValidator` |
| New plain observable, INPC only | Inherit from `ObservableObject` |
| Existing class with a fixed base class — opt into INPC without changing inheritance | `[INotifyPropertyChanged]` |
| DTO-shaped class that wraps another DTO with change tracking | `[ObservableDtoViewModel]` |

### Requirements

- The class **must be declared `partial`** (the generator emits `AtcXamlToolkit0002` warning if it is not).
- The attribute targets classes only (`AttributeTargets.Class`).

---

## 🪶 Weak event listening

`Atc.XamlToolkit.Mvvm` ships a thin sugar layer on top of `WeakReference` for the classic "consumer-side" leak: a short-lived view-model subscribes to a long-lived model's event and forgets to unsubscribe before being collected. The model's event keeps the view-model rooted forever.

The `Messenger` already covers this for pub/sub messaging — these helpers cover the cases where you can't change the source (you're listening to `INotifyCollectionChanged` on a `List<T>` someone else gave you, or `INotifyPropertyChanged` on a model you don't own).

### `WeakCollectionChangedListener` / `WeakPropertyChangedListener`

```csharp
using Atc.XamlToolkit.Mvvm;

public sealed class CustomerViewModel : ViewModelBase
{
    private IDisposable? lineItemsListener;

    public void AttachTo(ObservableCollection<LineItem> lineItems)
    {
        lineItemsListener = WeakCollectionChangedListener.Subscribe(
            lineItems,
            this,
            static (vm, _, e) => vm.OnLineItemsChanged(e));
    }

    private void OnLineItemsChanged(NotifyCollectionChangedEventArgs e)
    {
        // Handle add/remove/reset…
    }

    public override void Dispose()
    {
        // Optional eager teardown — otherwise GC of `this` will let the
        // listener auto-unsubscribe on the next raise.
        lineItemsListener?.Dispose();
        base.Dispose();
    }
}
```

The same shape works for `INotifyPropertyChanged`:

```csharp
using var listener = WeakPropertyChangedListener.Subscribe(
    model,
    this,
    static (vm, _, e) =>
    {
        if (e.PropertyName == nameof(Model.Status))
        {
            vm.RefreshFromStatus();
        }
    });
```

### Why a `static` lambda

The first argument of the callback is the live subscriber. **Pass a static lambda** (`static (s, sender, args) => s.…`) so the lambda doesn't accidentally capture `this` — which would defeat the weak-reference contract by re-introducing a strong root.

### `WeakEventListener<TSubscriber, TEventArgs>` (generic primitive)

For `EventHandler<TEventArgs>`-shaped events that aren't INPC/INotifyCollectionChanged, build on the generic primitive directly:

```csharp
using var listener = new WeakEventListener<MyViewModel, JobCompletedEventArgs>(
    this,
    static (vm, _, e) => vm.OnJobCompleted(e),
    h => jobRunner.Completed += h,
    h => jobRunner.Completed -= h);
```

### Lifetime semantics

- **Subscriber held weakly.** When the subscriber is collected, the next raise auto-unsubscribes the listener from the source.
- **Source held strongly via the closure.** Don't store the listener as a static — that pins the source for the process lifetime. Store it as an instance field and dispose it (or let the field reference die with the instance).
- **Disposal is idempotent.** Calling `Dispose()` more than once is safe.
- **Not a fit for fire-and-forget without a handle.** If you don't keep the returned `IDisposable` alive, GC may collect the listener before the first event raise. Keep the field while you're interested.

---

## 🪟 MainWindowViewModelBase

`MainWindowViewModelBase` extends `ViewModelBase` with the lifecycle and chrome glue that almost every desktop app needs: a Loaded hook that auto-maximises when the window is at least as large as the screen, a Closing hook that drives orderly shutdown, an F11 fullscreen toggle, and an `ApplicationExitCommand` for menu / button binding. Each platform package ships its own implementation behind a shared interface (`IMainWindowViewModelBase`).

### Lifecycle hooks (all platforms)

| Member | When to wire | Default behavior |
|---|---|---|
| `OnLoaded(sender, e)` | `Loaded` (WPF / Avalonia) or `Activated` / `Loaded` (WinUI 3) | Auto-maximises if the host element is at least as large as the primary screen's working area. |
| `OnClosing(sender, e)` | `Closing` (WPF / Avalonia) or `Closed` (WinUI 3) | Shuts down the application using the protected `ApplicationExitCode` (override to set a non-zero code). |
| `OnKeyDown(sender, e)` | `KeyDown` on the window or root element | Toggles fullscreen on **F11**. Override and call `base.OnKeyDown` first to add more shortcuts. |
| `OnKeyUp(sender, e)` | `KeyUp` on the window or root element | No-op extension point. |
| `ApplicationExitCommand` | Bind to a Close / Exit menu item | Invokes `OnClosing`. |

### Platform differences at a glance

| | WPF | WinUI 3 | Avalonia |
|---|---|---|---|
| `WindowState` property on the VM | ✅ `System.Windows.WindowState` | ❌ — chrome state is on `AppWindow.Presenter` (`OverlappedPresenter`) | ✅ `Avalonia.Controls.WindowState` |
| Shutdown call | `Application.Current.Shutdown(exitCode)` | `Application.Current.Exit()` (exit code ignored by the platform) | `IClassicDesktopStyleApplicationLifetime.TryShutdown(exitCode)` |
| F11 implementation | Sets `WindowState = Maximized/Normal` | `OverlappedPresenter.Maximize()` / `Restore()` via `WindowNative.GetWindowHandle` | Sets `WindowState = Maximized/Normal` |
| Auto-maximise check on Loaded | Compares against `SystemParameters.PrimaryScreenWidth/Height` | Compares against the primary `DisplayArea.WorkArea` | Compares against `Screens.Primary.WorkingArea` |

### WPF example

```csharp
public class MainWindowViewModel : MainWindowViewModelBase
{
    protected override int ApplicationExitCode => 0;

    // Add your top-level commands and properties here.
}
```

```xml
<Window x:Class="MyApp.MainWindow"
        WindowState="{Binding WindowState, Mode=TwoWay}"
        Loaded="MainWindow_OnLoaded"
        Closing="MainWindow_OnClosing"
        KeyDown="MainWindow_OnKeyDown"
        KeyUp="MainWindow_OnKeyUp">
    ...
</Window>
```

```csharp
private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
    => ((MainWindowViewModel)DataContext).OnLoaded(sender, e);

private void MainWindow_OnClosing(object sender, CancelEventArgs e)
    => ((MainWindowViewModel)DataContext).OnClosing(sender, e);

private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
    => ((MainWindowViewModel)DataContext).OnKeyDown(sender, e);

private void MainWindow_OnKeyUp(object sender, KeyEventArgs e)
    => ((MainWindowViewModel)DataContext).OnKeyUp(sender, e);
```

### WinUI 3 notes

- WinUI 3 has no `Window.WindowState` property; chrome state lives on `AppWindow.Presenter`. `MainWindowViewModelBase` resolves the `AppWindow` from the `Window` via `WindowNative.GetWindowHandle` + `Win32Interop.GetWindowIdFromWindow`, so the sender passed to `OnLoaded` / `OnKeyDown` **must** be the `Window` (or an element inside one) for the auto-maximise and F11 features to work.
- `Window.Closed` uses `WindowEventArgs`, not `CancelEventArgs`. Bridge it in your wiring code: construct a `new CancelEventArgs()` and forward to `OnClosing` (the platform won't honour `Cancel = true` from `Closed`, but the contract is preserved for cross-platform code).
- See the [WinUI threading remarks on `RelayCommandAsync`](../Command/Readme.md) for how `IsExecuting` bindings work with `x:Bind` on async commands.

### Avalonia example

```csharp
public class MainWindowViewModel : MainWindowViewModelBase
{
    protected override int ApplicationExitCode => 0;
}
```

```xml
<Window xmlns="https://github.com/avaloniaui"
        x:Class="MyApp.MainWindow"
        x:DataType="vm:MainWindowViewModel"
        WindowState="{Binding WindowState, Mode=TwoWay}">
    <Window.KeyBindings>
        ...
    </Window.KeyBindings>
</Window>
```

Wire `Loaded`, `Closing`, `KeyDown`, `KeyUp` from code-behind exactly like WPF (the event signature differs — `OnLoaded` takes `EventArgs` instead of `RoutedEventArgs`).

---

## ✅ Form Validation with Data Annotations

The `ViewModelBase` class implements `INotifyDataErrorInfo`, providing built-in support for validation using **Data Annotation attributes** from `System.ComponentModel.DataAnnotations`.

### 🎯 Key Features

- ✅ **Automatic validation** using standard Data Annotation attributes (`Required`, `Range`, `EmailAddress`, etc.)
- ✅ **Real-time validation** on property changes
- ✅ **Validation on initialization** option
- ✅ **Built-in error tracking** via `INotifyDataErrorInfo`
- ✅ **Performance optimized** with validation metadata caching
- ✅ **Framework support** for WPF, WinUI, and Avalonia

### 📋 Available Validation Attributes

Common validation attributes you can use:

| Attribute | Description | Example |
|-----------|-------------|---------|
| `Required` | Property must have a value | `[Required(ErrorMessage = "Name is required")]` |
| `MinLength` | Minimum string length | `[MinLength(2, ErrorMessage = "At least 2 characters")]` |
| `MaxLength` | Maximum string length | `[MaxLength(50, ErrorMessage = "Max 50 characters")]` |
| `Range` | Value must be within range | `[Range(18, 120, ErrorMessage = "Age 18-120")]` |
| `EmailAddress` | Must be valid email format | `[EmailAddress(ErrorMessage = "Invalid email")]` |
| `Phone` | Must be valid phone format | `[Phone(ErrorMessage = "Invalid phone number")]` |
| `RegularExpression` | Must match regex pattern | `[RegularExpression(@"^\d{5}$")]` |
| `StringLength` | String length constraints | `[StringLength(100, MinimumLength = 5)]` |
| `Url` | Must be valid URL | `[Url(ErrorMessage = "Invalid URL")]` |
| `Compare` | Must match another property | `[Compare(nameof(Password))]` |
| `CreditCard` | Must be valid credit card | `[CreditCard(ErrorMessage = "Invalid card")]` |

---

## 🖼️ WPF Validation Example

### WPF ViewModel Implementation

```csharp
using System.ComponentModel.DataAnnotations;
using Atc.XamlToolkit;

namespace MyApp.ViewModels;

public partial class PersonViewModel : ViewModelBase
{
    public PersonViewModel()
    {
        // Initialize validation system
        InitializeValidation(
            validateOnPropertyChanged: true,     // Validate as user types
            validateAllPropertiesOnInit: false); // Don't validate empty form
    }

    [ObservableProperty]
    [Required(ErrorMessage = "First name is required")]
    [MinLength(2, ErrorMessage = "First name must be at least 2 characters long")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    private string firstName = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    private string? lastName;

    [ObservableProperty]
    [Required(ErrorMessage = "Age is required")]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    private int? age;

    [ObservableProperty]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    private string? email;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        // Validate all properties before saving
        if (!ValidateAllProperties())
        {
            MessageBox.Show("Please fix validation errors before saving.", "Validation Error");
            return;
        }

        // Save logic here
        MessageBox.Show($"Saved: {FirstName} {LastName}", "Success");
    }

    private bool CanSave() => !HasErrors;
}
```

### WPF XAML View

```xml
<UserControl 
    x:Class="MyApp.Views.PersonView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="clr-namespace:MyApp.ViewModels">

    <UserControl.DataContext>
        <vm:PersonViewModel />
    </UserControl.DataContext>

    <UserControl.Resources>
        <!-- Validation Error Template -->
        <ControlTemplate x:Key="ValidationErrorTemplate">
            <DockPanel>
                <Border BorderBrush="Red" BorderThickness="2" CornerRadius="2">
                    <AdornedElementPlaceholder />
                </Border>
            </DockPanel>
        </ControlTemplate>

        <!-- TextBox Style with Validation -->
        <Style x:Key="ValidatedTextBox" TargetType="TextBox">
            <Setter Property="Validation.ErrorTemplate" 
                    Value="{StaticResource ValidationErrorTemplate}" />
            <Style.Triggers>
                <Trigger Property="Validation.HasError" Value="True">
                    <Setter Property="ToolTip">
                        <Setter.Value>
                            <Binding Path="(Validation.Errors).CurrentItem.ErrorContent" 
                                     RelativeSource="{RelativeSource Self}" />
                        </Setter.Value>
                    </Setter>
                </Trigger>
            </Style.Triggers>
        </Style>

        <!-- Error Display Template -->
        <DataTemplate x:Key="ValidationErrorTemplate" DataType="ValidationResult">
            <TextBlock Foreground="Red" Text="{Binding ErrorContent}" TextWrapping="Wrap" />
        </DataTemplate>

        <!-- Error ItemsControl Style -->
        <Style x:Key="ErrorDisplay" TargetType="ItemsControl">
            <Setter Property="Margin" Value="5,0,0,0" />
            <Setter Property="VerticalAlignment" Value="Center" />
            <Setter Property="ItemTemplate" Value="{StaticResource ValidationErrorTemplate}" />
        </Style>
    </UserControl.Resources>

    <Grid Margin="20">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="120" />
            <ColumnDefinition Width="250" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="10" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="10" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="10" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="20" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="10" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- First Name -->
        <TextBlock Grid.Row="0" Grid.Column="0" VerticalAlignment="Center">
            First Name *
        </TextBlock>
        <TextBox x:Name="FirstNameTextBox" 
                 Grid.Row="0" Grid.Column="1"
                 Style="{StaticResource ValidatedTextBox}"
                 Text="{Binding FirstName, 
                                UpdateSourceTrigger=PropertyChanged, 
                                ValidatesOnNotifyDataErrors=True, 
                                NotifyOnValidationError=True}" />
        <ItemsControl Grid.Row="0" Grid.Column="2"
                      ItemsSource="{Binding ElementName=FirstNameTextBox, Path=(Validation.Errors)}"
                      Style="{StaticResource ErrorDisplay}" />

        <!-- Last Name -->
        <TextBlock Grid.Row="2" Grid.Column="0" VerticalAlignment="Center">
            Last Name *
        </TextBlock>
        <TextBox x:Name="LastNameTextBox" 
                 Grid.Row="2" Grid.Column="1"
                 Style="{StaticResource ValidatedTextBox}"
                 Text="{Binding LastName, 
                                UpdateSourceTrigger=PropertyChanged, 
                                ValidatesOnNotifyDataErrors=True, 
                                NotifyOnValidationError=True}" />
        <ItemsControl Grid.Row="2" Grid.Column="2"
                      ItemsSource="{Binding ElementName=LastNameTextBox, Path=(Validation.Errors)}"
                      Style="{StaticResource ErrorDisplay}" />

        <!-- Age -->
        <TextBlock Grid.Row="4" Grid.Column="0" VerticalAlignment="Center">
            Age *
        </TextBlock>
        <TextBox x:Name="AgeTextBox" 
                 Grid.Row="4" Grid.Column="1"
                 Style="{StaticResource ValidatedTextBox}"
                 Text="{Binding Age, 
                                UpdateSourceTrigger=PropertyChanged, 
                                ValidatesOnNotifyDataErrors=True, 
                                NotifyOnValidationError=True}" />
        <ItemsControl Grid.Row="4" Grid.Column="2"
                      ItemsSource="{Binding ElementName=AgeTextBox, Path=(Validation.Errors)}"
                      Style="{StaticResource ErrorDisplay}" />

        <!-- Email -->
        <TextBlock Grid.Row="6" Grid.Column="0" VerticalAlignment="Center">
            Email
        </TextBlock>
        <TextBox x:Name="EmailTextBox" 
                 Grid.Row="6" Grid.Column="1"
                 Style="{StaticResource ValidatedTextBox}"
                 Text="{Binding Email, 
                                UpdateSourceTrigger=PropertyChanged, 
                                ValidatesOnNotifyDataErrors=True, 
                                NotifyOnValidationError=True}" />
        <ItemsControl Grid.Row="6" Grid.Column="2"
                      ItemsSource="{Binding ElementName=EmailTextBox, Path=(Validation.Errors)}"
                      Style="{StaticResource ErrorDisplay}" />

        <!-- Validation Status -->
        <TextBlock Grid.Row="8" Grid.Column="0" Grid.ColumnSpan="3" Margin="0,0,0,5">
            <Run Text="Has Errors: " />
            <Run FontWeight="Bold" Text="{Binding HasErrors, Mode=OneWay}" />
        </TextBlock>

        <!-- Save Button -->
        <Button Grid.Row="10" Grid.Column="1"
                Padding="10,5"
                Command="{Binding SaveCommand}"
                Content="Save" />
    </Grid>
</UserControl>
```

**Key WPF Binding Properties:**

- `ValidatesOnNotifyDataErrors=True` - Enables `INotifyDataErrorInfo` validation
- `NotifyOnValidationError=True` - Notifies when validation errors occur
- `UpdateSourceTrigger=PropertyChanged` - Validates as user types
- `Validation.ErrorTemplate` - Custom visual for validation errors
- `Validation.Errors` - Collection of validation error messages

---

## 🪟 WinUI Validation Example

### WinUI ViewModel Implementation

```csharp
using System.ComponentModel.DataAnnotations;
using Atc.XamlToolkit;

namespace MyApp.ViewModels;

public partial class PersonViewModel : ViewModelBase
{
    public PersonViewModel()
    {
        InitializeValidation(
            validateOnPropertyChanged: true,
            validateAllPropertiesOnInit: false);
    }

    [ObservableProperty]
    [Required(ErrorMessage = "First name is required")]
    [MinLength(2, ErrorMessage = "First name must be at least 2 characters long")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    private string firstName = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    private string? lastName;

    [ObservableProperty]
    [Required(ErrorMessage = "Age is required")]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    private int? age;

    [ObservableProperty]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    private string? email;

    // Helper properties for error display in WinUI
    public string? FirstNameError => GetErrorsForProperty(nameof(FirstName));
    public string? LastNameError => GetErrorsForProperty(nameof(LastName));
    public string? AgeError => GetErrorsForProperty(nameof(Age));
    public string? EmailError => GetErrorsForProperty(nameof(Email));

    private string? GetErrorsForProperty(string propertyName)
    {
        var errors = GetErrors(propertyName)?.Cast<string>().ToList();
        return errors?.Any() == true ? string.Join(", ", errors) : null;
    }

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!ValidateAllProperties())
        {
            var dialog = new ContentDialog
            {
                Title = "Validation Error",
                Content = "Please fix validation errors before saving.",
                CloseButtonText = "OK",
                XamlRoot = /* your XamlRoot */
            };
            await dialog.ShowAsync();
            return;
        }

        // Save logic here
    }

    private bool CanSave() => !HasErrors;
}
```

### WinUI XAML View

```xml
<UserControl
    x:Class="MyApp.Views.PersonView"
    xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="using:MyApp.ViewModels">

    <Grid Margin="20" ColumnSpacing="10" RowSpacing="10">
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="120" />
            <ColumnDefinition Width="250" />
            <ColumnDefinition Width="*" />
        </Grid.ColumnDefinitions>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="20" />
            <RowDefinition Height="Auto" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>

        <!-- First Name -->
        <TextBlock Grid.Row="0" Grid.Column="0" VerticalAlignment="Center">
            First Name *
        </TextBlock>
        <TextBox Grid.Row="0" Grid.Column="1"
                 Text="{x:Bind ViewModel.FirstName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
        <TextBlock Grid.Row="0" Grid.Column="2"
                   Margin="5,0,0,0"
                   VerticalAlignment="Center"
                   Foreground="Red"
                   Text="{x:Bind ViewModel.FirstNameError, Mode=OneWay}"
                   TextWrapping="Wrap" />

        <!-- Last Name -->
        <TextBlock Grid.Row="1" Grid.Column="0" VerticalAlignment="Center">
            Last Name *
        </TextBlock>
        <TextBox Grid.Row="1" Grid.Column="1"
                 Text="{x:Bind ViewModel.LastName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
        <TextBlock Grid.Row="1" Grid.Column="2"
                   Margin="5,0,0,0"
                   VerticalAlignment="Center"
                   Foreground="Red"
                   Text="{x:Bind ViewModel.LastNameError, Mode=OneWay}"
                   TextWrapping="Wrap" />

        <!-- Age -->
        <TextBlock Grid.Row="2" Grid.Column="0" VerticalAlignment="Center">
            Age *
        </TextBlock>
        <TextBox Grid.Row="2" Grid.Column="1"
                 Text="{x:Bind ViewModel.Age, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
        <TextBlock Grid.Row="2" Grid.Column="2"
                   Margin="5,0,0,0"
                   VerticalAlignment="Center"
                   Foreground="Red"
                   Text="{x:Bind ViewModel.AgeError, Mode=OneWay}"
                   TextWrapping="Wrap" />

        <!-- Email -->
        <TextBlock Grid.Row="3" Grid.Column="0" VerticalAlignment="Center">
            Email
        </TextBlock>
        <TextBox Grid.Row="3" Grid.Column="1"
                 Text="{x:Bind ViewModel.Email, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}" />
        <TextBlock Grid.Row="3" Grid.Column="2"
                   Margin="5,0,0,0"
                   VerticalAlignment="Center"
                   Foreground="Red"
                   Text="{x:Bind ViewModel.EmailError, Mode=OneWay}"
                   TextWrapping="Wrap" />

        <!-- Validation Status -->
        <TextBlock Grid.Row="5" Grid.Column="0" Grid.ColumnSpan="3">
            <Run Text="Has Errors: " />
            <Run FontWeight="Bold" Text="{x:Bind ViewModel.HasErrors, Mode=OneWay}" />
        </TextBlock>

        <!-- Save Button -->
        <Button Grid.Row="6" Grid.Column="1"
                Padding="10,5"
                Command="{x:Bind ViewModel.SaveCommand}"
                Content="Save" />
    </Grid>
</UserControl>
```

**WinUI Validation Notes:**

- WinUI uses `x:Bind` for compiled bindings (better performance)
- Helper properties (`FirstNameError`, etc.) provide error text for display
- `UpdateSourceTrigger=PropertyChanged` enables real-time validation
- Error messages displayed in separate `TextBlock` elements

---

## 🌐 Avalonia Validation Example

### Avalonia ViewModel Implementation

```csharp
using System.ComponentModel.DataAnnotations;
using Atc.XamlToolkit;

namespace MyApp.ViewModels;

public partial class PersonViewModel : ViewModelBase
{
    public PersonViewModel()
    {
        InitializeValidation(
            validateOnPropertyChanged: true,
            validateAllPropertiesOnInit: false);
    }

    [ObservableProperty]
    [Required(ErrorMessage = "First name is required")]
    [MinLength(2, ErrorMessage = "First name must be at least 2 characters long")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    private string firstName = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    private string? lastName;

    [ObservableProperty]
    [Required(ErrorMessage = "Age is required")]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    private int? age;

    [ObservableProperty]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    private string? email;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!ValidateAllProperties())
        {
            // Show validation error (implementation depends on your app)
            return;
        }

        // Save logic here
        await Task.CompletedTask;
    }

    private bool CanSave() => !HasErrors;
}
```

### Avalonia AXAML View

```xml
<UserControl 
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="clr-namespace:MyApp.ViewModels"
    x:Class="MyApp.Views.PersonView"
    x:DataType="vm:PersonViewModel">

    <UserControl.DataContext>
        <vm:PersonViewModel />
    </UserControl.DataContext>

    <Grid Margin="20" ColumnDefinitions="120,10,250" RowDefinitions="Auto,10,Auto,10,Auto,10,Auto,20,Auto,10,Auto">

        <!-- First Name -->
        <TextBlock Grid.Row="0" Grid.Column="0" VerticalAlignment="Center">
            First Name *
        </TextBlock>
        <TextBox Grid.Row="0" Grid.Column="2"
                 Text="{Binding FirstName}" />

        <!-- Last Name -->
        <TextBlock Grid.Row="2" Grid.Column="0" VerticalAlignment="Center">
            Last Name *
        </TextBlock>
        <TextBox Grid.Row="2" Grid.Column="2"
                 Text="{Binding LastName}" />

        <!-- Age -->
        <TextBlock Grid.Row="4" Grid.Column="0" VerticalAlignment="Center">
            Age *
        </TextBlock>
        <TextBox Grid.Row="4" Grid.Column="2"
                 Text="{Binding Age}" />

        <!-- Email -->
        <TextBlock Grid.Row="6" Grid.Column="0" VerticalAlignment="Center">
            Email
        </TextBlock>
        <TextBox Grid.Row="6" Grid.Column="2"
                 Text="{Binding Email}" />

        <!-- Validation Status -->
        <TextBlock Grid.Row="8" Grid.Column="0" Grid.ColumnSpan="3">
            <Run Text="Has Errors: " />
            <Run FontWeight="Bold" Text="{Binding HasErrors, Mode=OneWay}" />
        </TextBlock>

        <!-- Save Button -->
        <Button Grid.Row="10" Grid.Column="2"
                Padding="10,5"
                Command="{Binding SaveCommand}"
                Content="Save" />
    </Grid>
</UserControl>
```

**Avalonia Validation Notes:**

- Avalonia automatically supports `INotifyDataErrorInfo` validation
- Bindings automatically display validation errors
- Use `x:DataType` for compiled bindings (better performance)
- Visual error indicators appear automatically on controls with errors

---

## ⚙️ Validation Methods

### InitializeValidation

Configure validation behavior in your ViewModel constructor:

```csharp
public PersonViewModel()
{
    InitializeValidation(
        validateOnPropertyChanged: true,     // Validate each property as it changes
        validateAllPropertiesOnInit: false); // Don't validate empty form on load
}
```

**Parameters:**

- `validateOnPropertyChanged` - If `true`, validates properties automatically when they change
- `validateAllPropertiesOnInit` - If `true`, validates all properties immediately after initialization

### ValidateProperty

Manually validate a specific property:

```csharp
protected bool ValidateProperty(object? value, [CallerMemberName] string? propertyName = null)
```

**Example:**

```csharp
public string Email
{
    get => email;
    set
    {
        email = value;
        ValidateProperty(value);  // Manually trigger validation
        RaisePropertyChanged();
    }
}
```

### ValidateAllProperties

Validate all properties at once (useful before saving):

```csharp
protected bool ValidateAllProperties()
```

**Example:**

```csharp
[RelayCommand]
private void Save()
{
    if (!ValidateAllProperties())
    {
        MessageBox.Show("Please fix all validation errors.");
        return;
    }
    
    // Proceed with save
}
```

### INotifyDataErrorInfo Properties

`ViewModelBase` implements these properties:

```csharp
public bool HasErrors { get; }  // True if any validation errors exist
public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
public IEnumerable GetErrors(string? propertyName);  // Get errors for a property
```

---

## 💡 Validation Best Practices

### ✅ Do's

- ✅ **Use Data Annotations** for simple validation rules
- ✅ **Provide clear error messages** to guide users
- ✅ **Validate on property change** for immediate feedback
- ✅ **Check `HasErrors` before saving** to prevent invalid data
- ✅ **Call `ValidateAllProperties()`** before form submission
- ✅ **Use `CanExecute` with `HasErrors`** to enable/disable save buttons

### ❌ Don'ts

- ❌ Don't validate empty forms on initialization (users haven't started yet)
- ❌ Don't forget to call `InitializeValidation()` in constructor
- ❌ Don't use manual validation when Data Annotations work
- ❌ Don't ignore `HasErrors` when saving data

### 📝 Example: Complete Validation Workflow

```csharp
public partial class CustomerViewModel : ViewModelBase
{
    public CustomerViewModel()
    {
        // 1. Initialize validation
        InitializeValidation(
            validateOnPropertyChanged: true,
            validateAllPropertiesOnInit: false);
    }

    // 2. Add validation attributes
    [ObservableProperty]
    [Required(ErrorMessage = "Customer name is required")]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
    private string customerName = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    private string email = string.Empty;

    [ObservableProperty]
    [Range(1, 150, ErrorMessage = "Age must be between 1 and 150")]
    private int age;

    // 3. Use HasErrors for command CanExecute
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        // 4. Final validation check before save
        if (!ValidateAllProperties())
        {
            return;
        }

        // 5. Save valid data
        await _repository.SaveCustomerAsync(new Customer
        {
            Name = CustomerName,
            Email = Email,
            Age = Age
        });
    }

    private bool CanSave() => !HasErrors;
}
```

---

### 🚀 Getting started using `ViewModelBase`

Below is a simple example demonstrating how to create a ViewModel using `ViewModelBase`:

```csharp
public class MyViewModel : ViewModelBase
{
    private IRelayCommandAsync? saveCommand;

    public IRelayCommandAsync SaveCommand => saveCommand ??= new RelayCommandAsync(SaveCommandHandler, CanSaveCommandHandler);

    private string myProperty;

    public string MyProperty
    {
        get => myProperty;
        set
        {
            if (myProperty == value)
            {
                return;
            }

            myProperty = value;
            RaisePropertyChanged();
        }
    }

    private Task SaveCommandHandler()
    {
        return Task.CompletedTask;
    }

    public bool CanSaveCommandHandler()
    {
        return true;
    }
}
```
