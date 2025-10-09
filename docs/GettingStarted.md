# 🚀 Getting Started with Atc.XamlToolkit

This guide will help you get started with Atc.XamlToolkit for building MVVM applications in WPF, WinUI, or Avalonia.

## Installation

### For WPF Projects

```powershell
dotnet add package Atc.XamlToolkit.Wpf
```

### For WinUI Projects

```powershell
dotnet add package Atc.XamlToolkit.WinUI
```

### For Avalonia Projects

```powershell
dotnet add package Atc.XamlToolkit.Avalonia
```

## Your First ViewModel

### Step 1: Create a ViewModel

Create a new class that inherits from `ViewModelBase`:

```csharp
using Atc.XamlToolkit.Mvvm;

namespace MyApp.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string userName = "Guest";

    [ObservableProperty]
    private bool isLoading;

    [RelayCommand]
    private void SayHello()
    {
        MessageBox.Show($"Hello, {UserName}!");
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            await Task.Delay(2000); // Simulate loading
            UserName = "John Doe";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### Step 2: Create the View

**For WPF:**

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:viewModels="clr-namespace:MyApp.ViewModels"
        Title="My App" Height="300" Width="400">

    <Window.DataContext>
        <viewModels:MainViewModel />
    </Window.DataContext>

    <StackPanel Margin="20">
        <TextBlock Text="User Name:" />
        <TextBox Text="{Binding UserName, UpdateSourceTrigger=PropertyChanged}"
                 Margin="0,5,0,10" />

        <Button Content="Say Hello"
                Command="{Binding SayHelloCommand}"
                Margin="0,0,0,10" />

        <Button Content="Load Data"
                Command="{Binding LoadDataAsyncCommand}"
                Margin="0,0,0,10" />

        <ProgressBar IsIndeterminate="True"
                     Height="20"
                     Visibility="{Binding IsLoading,
                                         Converter={StaticResource BoolToVisibilityConverter}}" />
    </StackPanel>
</Window>
```

**For WinUI:**

```xml
<Window x:Class="MyApp.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:viewModels="using:MyApp.ViewModels"
        Title="My App">

    <Window.DataContext>
        <viewModels:MainViewModel />
    </Window.DataContext>

    <StackPanel Margin="20">
        <TextBlock Text="User Name:" />
        <TextBox Text="{Binding UserName, Mode=TwoWay}"
                 Margin="0,5,0,10" />

        <Button Content="Say Hello"
                Command="{Binding SayHelloCommand}"
                Margin="0,0,0,10" />

        <Button Content="Load Data"
                Command="{Binding LoadDataAsyncCommand}"
                Margin="0,0,0,10" />

        <ProgressBar IsIndeterminate="True"
                     Height="20"
                     Visibility="{Binding IsLoading,
                                         Converter={StaticResource BoolToVisibilityConverter}}" />
    </StackPanel>
</Window>
```

**For Avalonia:**

```xml
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:MyApp.ViewModels"
        x:Class="MyApp.MainWindow"
        x:DataType="vm:MainViewModel"
        Title="My App"
        Width="400" Height="300">

    <Design.DataContext>
        <vm:MainViewModel />
    </Design.DataContext>

    <StackPanel Margin="20">
        <TextBlock Text="User Name:" />
        <TextBox Text="{Binding UserName}"
                 Margin="0,5,0,10" />

        <Button Content="Say Hello"
                Command="{Binding SayHelloCommand}"
                Margin="0,0,0,10" />

        <Button Content="Load Data"
                Command="{Binding LoadDataAsyncCommand}"
                Margin="0,0,0,10" />

        <ProgressBar IsIndeterminate="True"
                     Height="20"
                     IsVisible="{Binding IsLoading}" />
    </StackPanel>
</Window>
```

### What Just Happened?

The source generators automatically created:

1. **UserName Property** - Full property with INotifyPropertyChanged
2. **IsLoading Property** - Full property with INotifyPropertyChanged
3. **SayHelloCommand** - RelayCommand that calls SayHello()
4. **LoadDataAsyncCommand** - RelayCommandAsync that calls LoadDataAsync()

No boilerplate code required! 🎉

## Working with Commands

### Basic Commands

```csharp
public partial class MyViewModel : ViewModelBase
{
    [RelayCommand]
    private void Save()
    {
        // Synchronous save logic
    }
}
```

### Commands with Parameters

```csharp
public partial class MyViewModel : ViewModelBase
{
    [RelayCommand]
    private void DeleteItem(int itemId)
    {
        // Delete logic
    }
}
```

XAML:

```xml
<Button Content="Delete"
        Command="{Binding DeleteItemCommand}"
        CommandParameter="123" />
```

### Async Commands

```csharp
public partial class MyViewModel : ViewModelBase
{
    [RelayCommand]
    private async Task SaveAsync()
    {
        await _repository.SaveAsync();
    }
}
```

### Commands with CanExecute

```csharp
public partial class MyViewModel : ViewModelBase
{
    [ObservableProperty]
    private string userName;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        // Save logic
    }

    private bool CanSave()
    {
        return !string.IsNullOrWhiteSpace(UserName);
    }
}
```

The command automatically refreshes when `UserName` changes!

## Property Dependencies

### Notify Other Properties

```csharp
public partial class PersonViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    private string firstName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    private string lastName;

    public string FullName => $"{FirstName} {LastName}";
}
```

### Notify Commands

```csharp
public partial class PersonViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool hasChanges;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private void Save()
    {
        // Save logic
    }

    private bool CanSave() => HasChanges;
}
```

## Using the Messenger

### Sending Messages

```csharp
public class OrderViewModel : ViewModelBase
{
    private void CompleteOrder()
    {
        // Complete the order...

        // Notify other ViewModels
        Messenger.Default.Send(
            new GenericMessage<Order>(currentOrder));
    }
}
```

### Receiving Messages

```csharp
public class InventoryViewModel : ViewModelBase
{
    public InventoryViewModel()
    {
        // Register for messages
        Messenger.Default.Register<GenericMessage<Order>>(
            this,
            OnOrderCompleted);
    }

    private void OnOrderCompleted(GenericMessage<Order> message)
    {
        var order = message.Content;
        // Update inventory
    }

    protected override void Cleanup()
    {
        Messenger.Default.Unregister(this);
        base.Cleanup();
    }
}
```

## Using Value Converters

### In WPF

```xml
xmlns:converters="clr-namespace:Atc.XamlToolkit.ValueConverters;assembly=Atc.XamlToolkit.Wpf"

<TextBlock Text="Data loaded!"
           Visibility="{Binding IsDataLoaded,
                               Converter={x:Static converters:BoolToVisibilityVisibleValueConverter.Instance}}" />
```

### In Avalonia

```xml
xmlns:converters="clr-namespace:Atc.XamlToolkit.ValueConverters;assembly=Atc.XamlToolkit.Avalonia"

<TextBlock Text="Data loaded!"
           IsVisible="{Binding IsDataLoaded,
                              Converter={x:Static converters:BoolToVisibilityVisibleValueConverter.Instance}}" />
```

## WPF-Specific Features

### Dependency Properties

```csharp
public partial class CustomControl : UserControl
{
    public CustomControl()
    {
        InitializeComponent();
    }

    [DependencyProperty<string>("Title", DefaultValue = "Untitled")]
    [DependencyProperty<bool>("IsExpanded", DefaultValue = true)]
}
```

### Attached Properties

```csharp
[AttachedProperty<bool>("IsEnabled")]
public static partial class MyBehavior
{
}
```

XAML:

```xml
<Grid local:MyBehavior.IsEnabled="True">
    <!-- Content -->
</Grid>
```

### Routed Events

```csharp
public partial class CustomButton : Button
{
    [RoutedEvent(RoutingStrategy.Bubble)]
    private static readonly RoutedEvent itemClicked;
}
```

## Design-Time Support

### Show Sample Data in Designer

```csharp
public partial class ProductListViewModel : ViewModelBase
{
    public ProductListViewModel()
    {
        if (DesignModeHelper.IsInDesignMode)
        {
            // Sample data for designer
            Products = new ObservableCollection<Product>
            {
                new Product { Name = "Sample Product 1", Price = 19.99m },
                new Product { Name = "Sample Product 2", Price = 29.99m }
            };
        }
        else
        {
            // Load real data
            LoadProducts();
        }
    }
}
```

## Best Practices

### 1. Use Partial Classes

Always mark ViewModels as `partial` when using source generators:

```csharp
public partial class MyViewModel : ViewModelBase
{
    // Source generators need partial classes
}
```

### 2. Follow Naming Conventions

- Private fields should be camelCase: `userName`
- Generated properties will be PascalCase: `UserName`
- Commands get "Command" suffix: `SaveCommand`

### 3. Always Unregister Messages

```csharp
protected override void Cleanup()
{
    Messenger.Default.Unregister(this);
    base.Cleanup();
}
```

### 4. Use CanExecute for Commands

```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private void Save() { }

private bool CanSave() => /* condition */;
```

### 5. Notify Dependencies

```csharp
[ObservableProperty]
[NotifyPropertyChangedFor(nameof(FullName))]
private string firstName;
```

## Common Patterns

### Loading Data Pattern

```csharp
public partial class DataViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private ObservableCollection<Item> items = new();

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        IsLoading = true;

        try
        {
            var data = await _repository.GetAllAsync();
            Items.Clear();
            foreach (var item in data)
            {
                Items.Add(item);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }
}
```

### Form Validation Pattern

```csharp
public partial class FormViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private string email;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SubmitCommand))]
    private string password;

    [RelayCommand(CanExecute = nameof(CanSubmit))]
    private async Task SubmitAsync()
    {
        await _authService.LoginAsync(Email, Password);
    }

    private bool CanSubmit()
    {
        return !string.IsNullOrWhiteSpace(Email)
            && !string.IsNullOrWhiteSpace(Password)
            && Password.Length >= 8;
    }
}
```

### Master-Detail Pattern

```csharp
public partial class MasterDetailViewModel : ViewModelBase
{
    [ObservableProperty]
    private ObservableCollection<Customer> customers = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasSelectedCustomer))]
    [NotifyCanExecuteChangedFor(nameof(EditCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private Customer? selectedCustomer;

    public bool HasSelectedCustomer => SelectedCustomer != null;

    [RelayCommand(CanExecute = nameof(HasSelectedCustomer))]
    private void Edit()
    {
        // Edit selected customer
    }

    [RelayCommand(CanExecute = nameof(HasSelectedCustomer))]
    private void Delete()
    {
        if (SelectedCustomer != null)
        {
            Customers.Remove(SelectedCustomer);
        }
    }
}
```

## Next Steps

Now that you've got the basics, explore these topics:

1. **[MVVM Framework](Mvvm/Readme.md)** - Deep dive into MVVM components
2. **[Messaging System](Messaging/Readme.md)** - Advanced messaging patterns
3. **[Source Generators](SourceGenerators/ViewModel.md)** - All generator options
4. **[Value Converters](ValueConverters/Readme.md)** - Complete converter reference
5. **[Performance](Performance/Readme.md)** - Optimization techniques
6. **[Utilities](Utilities/Readme.md)** - Helper classes and utilities

## Sample Projects

Check out the sample projects in the repository:

- **Atc.XamlToolkit.WpfSample** - WPF sample application
- **Atc.XamlToolkit.AvaloniaSample** - Avalonia sample application

These samples demonstrate real-world usage of all features.

## Need Help?

- 📖 [Documentation](../README.md)
- 🐛 [Report Issues](https://github.com/atc-net/atc-xaml-toolkit/issues)
- 💬 [Discussions](https://github.com/atc-net/atc-xaml-toolkit/discussions)
- 🌟 [ATC-NET Organization](https://atc-net.github.io/)

Happy coding! 🚀
