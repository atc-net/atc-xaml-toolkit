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

| 🧩 Component              | 📋 Description                                                                |
|---------------------------|--------------------------------------------------------------------------------|
| `ViewModelBase`           | A base class for ViewModels.                                                   |
| `MainWindowViewModelBase` | A base class for the main window ViewModel.                                    |
| `ViewModelDialogBase`     | A base class for dialog ViewModels.                                            |
| `ObservableObject`        | A base class for observable objects implementing `INotifyPropertyChanged`.     |
| `RelayCommand`            | A command supporting `CanExecute`.                                             |
| `RelayCommand<T>`         | A command with a generic parameter and `CanExecute`.                           |
| `RelayCommandAsync`       | An asynchronous command supporting `CanExecute`.                               |
| `RelayCommandAsync<T>`    | An asynchronous command with a generic parameter and `CanExecute`.             |

📖 For detailed information about commands, refer to the [RelayCommand documentation](../SourceGenerators/ViewModel.md).

📖 For wrapping DTOs with ViewModels, see the [ObservableDtoViewModel documentation](../SourceGenerators/ViewModel.md#-wrapping-dtos-with-observabledtoviewmodel).

💡 **Tip:** The `ObservableDtoViewModel` generator automatically adds `IsDirty` tracking to your ViewModels when inheriting from `ViewModelBase`, helping you track changes in your forms and data. See the [Change Tracking with IsDirty](../SourceGenerators/ViewModel.md#-change-tracking-with-isdirty) section for more details.

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
