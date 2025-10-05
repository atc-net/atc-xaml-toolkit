# 🔧 Utilities and Helper Classes

The Atc.XamlToolkit provides several utility classes and helpers to streamline common XAML development tasks.

## DesignModeHelper

### Overview

The `DesignModeHelper` class helps you detect whether your application is running in design-time mode (Visual Studio Designer, Blend, etc.) or runtime mode.

### Why Use It?

When building custom controls or ViewModels, you often need different behavior in the designer versus runtime:

- **Design-time**: Show sample data, skip service initialization, avoid exceptions
- **Runtime**: Use real data, initialize services, normal operation

### API

```csharp
public static class DesignModeHelper
{
    /// <summary>
    /// Gets a value indicating whether the code is running in design mode.
    /// </summary>
    public static bool IsInDesignMode { get; }
}
```

### Usage Examples

#### Example 1: Skip Service Initialization in Designer

```csharp
public class MyViewModel : ViewModelBase
{
    public MyViewModel()
    {
        if (DesignModeHelper.IsInDesignMode)
        {
            // Use sample data for designer
            Users = new ObservableCollection<User>
            {
                new User { Name = "Sample User 1", Age = 25 },
                new User { Name = "Sample User 2", Age = 30 }
            };
        }
        else
        {
            // Load real data at runtime
            LoadUsersFromDatabase();
        }
    }
}
```

#### Example 2: Avoid Exceptions in Designer

```csharp
public class DataService
{
    public DataService()
    {
        if (DesignModeHelper.IsInDesignMode)
        {
            // Don't initialize database connection in designer
            return;
        }

        // Runtime initialization
        InitializeDatabaseConnection();
    }
}
```

#### Example 3: Custom Control with Design-Time Support

```csharp
public class ChartControl : UserControl
{
    public ChartControl()
    {
        InitializeComponent();

        if (DesignModeHelper.IsInDesignMode)
        {
            // Show sample chart in designer
            DisplaySampleData();
        }
    }

    private void DisplaySampleData()
    {
        DataPoints = new[]
        {
            new DataPoint { X = 0, Y = 10 },
            new DataPoint { X = 1, Y = 20 },
            new DataPoint { X = 2, Y = 15 }
        };
    }
}
```

#### Example 4: Conditional Resource Loading

```csharp
public class ResourceManager
{
    public ResourceManager()
    {
        if (DesignModeHelper.IsInDesignMode)
        {
            // Use embedded resources for designer
            LoadEmbeddedResources();
        }
        else
        {
            // Load from file system or remote at runtime
            LoadExternalResources();
        }
    }
}
```

### Platform Support

Available in both:

- ✅ **Atc.XamlToolkit.Wpf**
- ✅ **Atc.XamlToolkit.Avalonia**

### Best Practices

1. **Use for initialization logic** - Skip expensive operations in designer
2. **Provide sample data** - Make controls look good in designer
3. **Avoid exceptions** - Prevent designer crashes due to missing resources
4. **Keep it simple** - Don't over-complicate design-time logic

---

## Base Converter Classes

### Overview

Creating custom value converters is common in XAML applications. The toolkit provides base classes that simplify converter implementation.

### ValueConverterBase

Base class for single-value converters implementing `IValueConverter`.

#### API

```csharp
public abstract class ValueConverterBase : IValueConverter
{
    protected abstract object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture);

    protected virtual object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

#### Usage Example

```csharp
public class TemperatureConverter : ValueConverterBase
{
    protected override object Convert(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (value is double celsius)
        {
            // Convert Celsius to Fahrenheit
            return celsius * 9.0 / 5.0 + 32.0;
        }

        return DependencyProperty.UnsetValue;
    }

    protected override object ConvertBack(
        object value,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (value is double fahrenheit)
        {
            // Convert Fahrenheit to Celsius
            return (fahrenheit - 32.0) * 5.0 / 9.0;
        }

        return DependencyProperty.UnsetValue;
    }
}
```

### MultiValueConverterBase

Base class for multi-value converters implementing `IMultiValueConverter`.

#### API

```csharp
public abstract class MultiValueConverterBase : IMultiValueConverter
{
    protected abstract object Convert(
        object[] values,
        Type targetType,
        object parameter,
        CultureInfo culture);

    protected virtual object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

#### Usage Example

```csharp
public class FullNameConverter : MultiValueConverterBase
{
    protected override object Convert(
        object[] values,
        Type targetType,
        object parameter,
        CultureInfo culture)
    {
        if (values.Length >= 2 &&
            values[0] is string firstName &&
            values[1] is string lastName)
        {
            return $"{firstName} {lastName}";
        }

        return string.Empty;
    }

    protected override object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture)
    {
        if (value is string fullName)
        {
            var parts = fullName.Split(' ', 2);
            return new object[]
            {
                parts.Length > 0 ? parts[0] : string.Empty,
                parts.Length > 1 ? parts[1] : string.Empty
            };
        }

        return new object[] { string.Empty, string.Empty };
    }
}
```

### XAML Usage

```xml
<!-- Single value converter -->
<TextBlock Text="{Binding Temperature,
    Converter={StaticResource TemperatureConverter}}" />

<!-- Multi-value converter -->
<TextBlock>
    <TextBlock.Text>
        <MultiBinding Converter="{StaticResource FullNameConverter}">
            <Binding Path="FirstName" />
            <Binding Path="LastName" />
        </MultiBinding>
    </TextBlock.Text>
</TextBlock>
```

### Benefits

- ✅ **Cleaner code** - Focus on conversion logic, not boilerplate
- ✅ **Consistent error handling** - Base classes handle edge cases
- ✅ **Easy testing** - Test conversion logic directly
- ✅ **Type safety** - Protected abstract methods are easier to implement

---

## Error Handling - IErrorHandler

### Overview

The `IErrorHandler` interface provides a contract for centralized error handling in RelayCommands.

### API

```csharp
public interface IErrorHandler
{
    void HandleError(Exception exception);
}
```

### Usage Example

#### Step 1: Implement Error Handler

```csharp
public class GlobalErrorHandler : IErrorHandler
{
    private readonly ILogger logger;

    public GlobalErrorHandler(ILogger logger)
    {
        this.logger = logger;
    }

    public void HandleError(Exception exception)
    {
        // Log the error
        logger.LogError(exception, "Command execution failed");

        // Show user-friendly message
        MessageBox.Show(
            $"An error occurred: {exception.Message}",
            "Error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }
}
```

#### Step 2: Use with RelayCommandAsync

```csharp
public class MyViewModel : ViewModelBase
{
    private readonly IDataService dataService;
    private readonly IErrorHandler errorHandler;

    public MyViewModel(
        IDataService dataService,
        IErrorHandler errorHandler)
    {
        this.dataService = dataService;
        this.errorHandler = errorHandler;
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        try
        {
            // This might throw
            await dataService.LoadAsync();
        }
        catch (Exception ex)
        {
            errorHandler.HandleError(ex);
        }
    }
}
```

#### Step 3: Or Pass to Constructor

```csharp
public class MyViewModel : ViewModelBase
{
    private IRelayCommandAsync? saveCommand;

    public MyViewModel(IErrorHandler errorHandler)
    {
        // Pass error handler to command
        saveCommand = new RelayCommandAsync(
            async () => await SaveAsync(),
            () => CanSave(),
            errorHandler); // Error handler parameter
    }
}
```

### Benefits

- ✅ **Centralized error handling** - One place for all error logic
- ✅ **Consistent UX** - All errors handled the same way
- ✅ **Easy to test** - Mock error handler in tests
- ✅ **Separation of concerns** - ViewModels don't handle UI errors

---

## IExecuteWithObject Interfaces

### Overview

Low-level interfaces for dynamic command execution.

### API

```csharp
public interface IExecuteWithObject
{
    void Execute(object parameter);
}

public interface IExecuteWithObjectAndResult
{
    object Execute(object parameter);
}
```

### Usage

These interfaces are primarily used internally by the framework but can be useful for advanced scenarios:

```csharp
public class DynamicCommandExecutor
{
    public void ExecuteIfPossible(object command, object parameter)
    {
        if (command is IExecuteWithObject executor)
        {
            executor.Execute(parameter);
        }
    }
}
```

---

## Summary

The Atc.XamlToolkit utilities provide:

### DesignModeHelper

- ✅ Detect design-time vs runtime
- ✅ Provide better designer experience
- ✅ Prevent designer crashes

### Base Converter Classes

- ✅ Simplify custom converter creation
- ✅ Reduce boilerplate code
- ✅ Consistent error handling

### IErrorHandler

- ✅ Centralized error handling
- ✅ Consistent user experience
- ✅ Better testability

### Best Practices

1. **Use DesignModeHelper** for design-time initialization
2. **Inherit from base converter classes** when creating custom converters
3. **Implement IErrorHandler** for centralized error management
4. **Keep utilities simple** and focused on single responsibility

These utilities work together to make XAML development more productive and maintainable.
