# ⚙️ ViewModel with Source Generation

The **Atc.Wpf Source Generators** simplify ViewModel development by reducing boilerplate code for properties and commands. With attributes like `ObservableProperty` and `RelayCommand`, you can focus on business logic while automatically handling property change notifications and command implementations.

---

## 🚀 Setting Up Your First ViewModel

### ✨ Creating a Simple ViewModel

Let's start by defining a ViewModel using source generators.

```csharp
public partial class TestViewModel : ViewModelBase
{
    [ObservableProperty]
    private string name;
}
```

### 🔍 What's Happening Here?

- `ObservablePropertyAttribute` automatically generates the `Name` property, including `INotifyPropertyChanged` support.
- `RelayCommand` generates a `SayHelloCommand`, which can be bound to a button in the UI.

### 🖥️ XAML Binding Example

```xml
<UserControl xmlns:local="clr-namespace:MyApp.MyUserControl">
    <UserControl.DataContext>
        <local:TestViewModel/>
    </UserControl.DataContext>

    <StackPanel>

        <TextBox Text="{Binding Path=Name, UpdateSourceTrigger=PropertyChanged}" />

        <Button Content="Say Hello" Command="{Binding Path=SayHelloCommand}" />

    </StackPanel>
</UserControl>
```

This setup allows the UI to dynamically update when the Name property changes.

---

## 📌 Attributes for Property Source Generation

The `ObservableProperty` attribute automatically generates properties from private fields, including `INotifyPropertyChanged` support.

**ObservableProperty options:**

- `PropertyName` for customization.
- `DependentPropertyNames` for 1 to many other properties to be notified.
- `DependentCommandNames` for 1 to many other commands to be notified.
- `BeforeChangedCallback` is executed before the property value changes.
- `AfterChangedCallback` is executed after the property value changes.
- `UseIsDirty` automatically sets `IsDirty = true` when the property changes.

### 🛠 Quick Start: Using `ObservableProperty`

```csharp
// Generates a property named "Name"
[ObservableProperty()]
private string name;

// Generates a property named "MyName"
[ObservableProperty("MyName")]
private string name;

// Generates a property named "MyName" and notifies FullName and Age
[ObservableProperty(nameof(MyName), DependentPropertyNames = [nameof(FullName), nameof(Age)])]
private string name;

// Generates a property named "MyName" and notifies ApplyCommand and SaveCommand
[ObservableProperty(nameof(MyName), DependentCommandNames = [nameof(ApplyCommand) nameof(SaveCommand)])]
private string name;
```

### 🔔 Notifying Other Properties

```csharp
// Generates a property named "Name" and notifies FullName and Age
[ObservableProperty(DependentPropertyNames = [nameof(FullName), nameof(Age)])]

// Notifies the property "Email"
[NotifyPropertyChangedFor(nameof(Email))]

// Notifies multiple properties
[NotifyPropertyChangedFor(nameof(FullName), nameof(Age))]
```

**Note:**

- `NotifyPropertyChangedFor` ensures that when the annotated property changes, specified dependent properties also get notified.

### 🧮 Computed Properties with Automatic Dependency Detection

The `ComputedProperty` attribute automatically detects which properties a computed property depends on and ensures those dependencies trigger property change notifications. This eliminates the need to manually specify `NotifyPropertyChangedFor` on multiple properties.

**What is a Computed Property?**

A computed property is a property whose value is calculated from other properties. Common examples include:

- `FullName` derived from `FirstName` and `LastName`
- `TotalPrice` calculated from `UnitPrice` and `Quantity`
- `DisplayText` formatted from multiple data fields
- `IsValid` based on validation of other properties

**How It Works:**

```csharp
public partial class PersonViewModel : ViewModelBase
{
    [ObservableProperty]
    private string firstName = string.Empty;

    [ObservableProperty]
    private string? lastName;

    // Mark computed properties with [ComputedProperty]
    [ComputedProperty]
    public string FullName => $"{FirstName} {LastName}";
}
```

**Generated Code:**

The source generator automatically analyzes `FullName` and detects it depends on `FirstName` and `LastName`. It then generates setters that notify `FullName` when either dependency changes:

```csharp
public partial class PersonViewModel
{
    public string FirstName
    {
        get => firstName;
        set
        {
            if (firstName == value) return;
            firstName = value;
            RaisePropertyChanged(nameof(FirstName));
            RaisePropertyChanged(nameof(FullName));  // Automatically added!
        }
    }

    public string? LastName
    {
        get => lastName;
        set
        {
            if (lastName == value) return;
            lastName = value;
            RaisePropertyChanged(nameof(LastName));
            RaisePropertyChanged(nameof(FullName));  // Automatically added!
        }
    }
}
```

**Benefits of ComputedProperty:**

✅ **Automatic dependency detection** - No need to manually track which properties depend on each other  
✅ **Less boilerplate** - No repetitive `[NotifyPropertyChangedFor]` attributes on multiple properties  
✅ **Maintainable** - When you change the computed property implementation, dependencies are automatically updated  
✅ **Type-safe** - The generator analyzes your code, so typos in property names are impossible  
✅ **Works with expressions** - Supports expression-bodied properties, string interpolation, method calls, etc.

**Before (manual approach):**

```csharp
public partial class PersonViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]  // Manual
    private string firstName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]  // Manual
    private string? lastName;

    public string FullName => $"{FirstName} {LastName}";
}
```

**After (with ComputedProperty):**

```csharp
public partial class PersonViewModel : ViewModelBase
{
    [ObservableProperty]
    private string firstName;

    [ObservableProperty]
    private string? lastName;

    [ComputedProperty]  // Dependencies detected automatically!
    public string FullName => $"{FirstName} {LastName}";
}
```

**Advanced Examples:**

**Multiple computed properties:**

```csharp
public partial class OrderViewModel : ViewModelBase
{
    [ObservableProperty]
    private decimal unitPrice;

    [ObservableProperty]
    private int quantity;

    [ObservableProperty]
    private decimal taxRate;

    [ComputedProperty]
    public decimal Subtotal => UnitPrice * Quantity;

    [ComputedProperty]
    public decimal Tax => Subtotal * TaxRate;

    [ComputedProperty]
    public decimal Total => Subtotal + Tax;
}
```

When `UnitPrice` or `Quantity` changes, `Subtotal`, `Tax`, and `Total` all get notified automatically!

**Complex expressions:**

```csharp
public partial class UserViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? email;

    [ObservableProperty]
    private string? phoneNumber;

    [ObservableProperty]
    private bool termsAccepted;

    [ComputedProperty]
    public bool CanRegister => 
        !string.IsNullOrWhiteSpace(Email) && 
        !string.IsNullOrWhiteSpace(PhoneNumber) && 
        TermsAccepted;
}
```

**Using with ObservableDtoViewModel:**

ComputedProperty also works seamlessly with `ObservableDtoViewModel` for wrapping DTOs:

```csharp
public class PersonDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? BirthDate { get; set; }
}

[ObservableDtoViewModel(typeof(PersonDto))]
public partial class PersonViewModel : ViewModelBase
{
    [ComputedProperty]
    public string FullName => $"{FirstName} {LastName}".Trim();

    [ComputedProperty]
    public int? Age => BirthDate.HasValue 
        ? (int?)((DateTime.Now - BirthDate.Value).TotalDays / 365.25)
        : null;
}
```

**Best Practices:**

✅ **Use expression-bodied properties** - Keep computed properties simple and readable  
✅ **Mark all computed properties** - Consistently use `[ComputedProperty]` for all derived properties  
✅ **Keep logic simple** - Computed properties should be fast calculations, not heavy processing  
✅ **Combine with validation** - Use computed properties to derive `IsValid` or `CanSave` states

**What's Analyzed:**

The source generator automatically detects dependencies in:

- String interpolation: `$"{FirstName} {LastName}"`
- Property access: `FirstName + " " + LastName`
- Method calls with property arguments: `string.Join(" ", FirstName, LastName)`
- Conditional expressions: `FirstName ?? "Unknown"`
- Complex expressions: `(FirstName?.Length ?? 0) + (LastName?.Length ?? 0)`

**Note:** The generator only analyzes properties defined in the same class. External method calls or properties from other objects are not tracked as dependencies.

### 🔮 Callbacks

```csharp
// Calls DoStuff before the property changes
[ObservableProperty(BeforeChangedCallback = nameof(DoStuff))]

// Calls DoStuff after the property changes
[ObservableProperty(AfterChangedCallback = nameof(DoStuff))]

// Calls DoStuffA before and DoStuffB after the property changes
[ObservableProperty(
    BeforeChangedCallback = nameof(DoStuffA),
    AfterChangedCallback = nameof(DoStuffB))]

// Executes inline code before and after the property changes
// - Executes DoStuffA before the change
// - Executes event and DoStuffB after the change
[ObservableProperty(
    BeforeChangedCallback = "DoStuffA();",
    AfterChangedCallback = "EntrySelected?.Invoke(this, selectedEntry); DoStuffB();")]
```

### 🔄 Change Tracking with `UseIsDirty`

You can enable automatic change tracking for individual properties by setting `UseIsDirty = true`. When enabled, the generated property setter will automatically set `IsDirty = true` whenever the property value changes.

```csharp
// Automatically sets IsDirty = true when the name changes
[ObservableProperty(UseIsDirty = true)]
private string name;

// Combine with other options
[ObservableProperty(
    UseIsDirty = true, 
    DependentPropertyNames = [nameof(FullName)])]
private string firstName;
```

**Generated code includes IsDirty tracking:**

```csharp
public string Name
{
    get => name;
    set
    {
        if (name == value)
        {
            return;
        }

        name = value;
        RaisePropertyChanged(nameof(Name));
        IsDirty = true;  // Automatically added!
    }
}
```

**Use cases for UseIsDirty on individual properties:**

- **Form tracking** - Know when specific fields have been modified
- **Selective tracking** - Only track changes on certain properties
- **Validation triggers** - Trigger validation when key fields change
- **Save indicators** - Show visual indicators for modified fields
- **Undo/Redo** - Track which specific properties have changed

**Example with selective tracking:**

```csharp
public partial class CustomerEditViewModel : ViewModelBase
{
    // Tracked property - important data
    [ObservableProperty(UseIsDirty = true)]
    private string email;

    // Tracked property - important data
    [ObservableProperty(UseIsDirty = true)]
    private string phoneNumber;

    // Not tracked - UI state only
    [ObservableProperty]
    private bool isExpanded;

    // Not tracked - temporary filter
    [ObservableProperty]
    private string searchText;

    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        await repository.UpdateAsync(new CustomerDto 
        { 
            Email = Email, 
            PhoneNumber = PhoneNumber 
        });
        
        IsDirty = false;  // Reset after save
    }

    private bool CanSave() => IsDirty;
}
```

**Note:** Your ViewModel must have an `IsDirty` property for this feature to work. `ViewModelBase` includes this property by default.

## ⚡ Attributes for `RelayCommand` Source-Generation

The `RelayCommand` attribute generates `IRelayCommand` properties, eliminating manual command setup.

**RelayCommand options:**

- `CommandName` for customization.
- `CanExecute` a property or method that return `bool` to specified to control when the command is executable.
- `ParameterValue` or `ParameterValues` for 1 or many parameter values.

### 🛠 Quick Start Tips for RelayCommands

```csharp
// Generates a RelayCommand named "SaveCommand"
[RelayCommand()]
public void Save();

// Generates a RelayCommand named "MySaveCommand"
[RelayCommand("MySave")]
public void Save();
```

### 🏷️ Commands with CanExecute Logic

```csharp
// Generates a RelayCommand that takes a string parameter
[RelayCommand()]
public void Save(string text);

// Generates a RelayCommand with CanExecute function
[RelayCommand(CanExecute = nameof(CanSave))]
public void Save();
```

**Note:**

- The `RelayCommand` attribute generates an `IRelayCommand` property linked to the annotated method.
- `CanExecute` logic can be specified to control when the command is executable.

### 🔄 Asynchronous Commands

```csharp
// Generates an asynchronous RelayCommand
[RelayCommand()]
public Task Save();

// Generates an asynchronous RelayCommand with async keyword
[RelayCommand()]
public async Task Save();

// Generates an asynchronous RelayCommand named "MySaveCommand"
[RelayCommand("MySave")]
public Task Save();

// Generates an asynchronous RelayCommand named "MySaveCommand" with async keyword
[RelayCommand("MySave")]
public async Task Save();

// Generates an asynchronous RelayCommand that takes a string parameter
[RelayCommand()]
public Task Save(string text);

// Generates an asynchronous RelayCommand with async keyword and string parameter
[RelayCommand()]
public async Task Save(string text);

// Generates an asynchronous RelayCommand with CanExecute function
[RelayCommand(CanExecute = nameof(CanSave))]
public Task Save();

// Generates an asynchronous RelayCommand with async keyword and CanExecute function
[RelayCommand(CanExecute = nameof(CanSave))]
public async Task Save();
```

### 🔁 Commands with Multi-Parameter

```csharp
// Generates multi asynchronous RelayCommand with async keyword with multiple parameters
[RelayCommand("MyTestLeft", ParameterValues = [LeftTopRightBottomType.Left, 1])]
[RelayCommand("MyTestTop", ParameterValues = [LeftTopRightBottomType.Top, 1])]
[RelayCommand("MyTestRight", ParameterValues = [LeftTopRightBottomType.Right, 1])]
[RelayCommand("MyTestBottom", ParameterValues = [LeftTopRightBottomType.Bottom, 1])]
public Task TestDirection(LeftTopRightBottomType leftTopRightBottomType, int steps)

// Generates multi asynchronous RelayCommand with async keyword and CanExecute function with multiple parameters
[RelayCommand("MyTestLeft", CanExecute = nameof(CanTestDirection), ParameterValues = [LeftTopRightBottomType.Left, 1])]
[RelayCommand("MyTestTop", CanExecute = nameof(CanTestDirection), ParameterValues = [LeftTopRightBottomType.Top, 1])]
[RelayCommand("MyTestRight", CanExecute = nameof(CanTestDirection), ParameterValues = [LeftTopRightBottomType.Right, 1])]
[RelayCommand("MyTestBottom", CanExecute = nameof(CanTestDirection), ParameterValues = [LeftTopRightBottomType.Bottom, 1])]
public Task TestDirection(LeftTopRightBottomType leftTopRightBottomType, int steps)
```

### 🔀 Commands executed on a background thread

```csharp
// Generates an RelayCommand with and CanExecute function and will be executed on a background thread
[RelayCommand(CanExecute = nameof(CanSave), ExecuteOnBackgroundThread = true)]
public void Save();

// Generates an asynchronous RelayCommand with async keyword and CanExecute function and will be executed on a background thread
[RelayCommand(CanExecute = nameof(CanSave), ExecuteOnBackgroundThread = true)]
public async Task Save();
```

### ♿ Commands with auto set on IsBusy

```csharp
// Generates an RelayCommand and toggles IsBusy around a synchronous command execution
[RelayCommand(CanExecute = nameof(CanSave), AutoSetIsBusy = true)]
public void Save();

// Generates an RelayCommand and toggles IsBusy around a synchronous, background‑thread command execution
[RelayCommand(CanExecute = nameof(CanSave), ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
public void Save();

// Generates an asynchronous RelayCommand and toggles IsBusy around an asynchronous command execution
[RelayCommand(CanExecute = nameof(CanSave), AutoSetIsBusy = true)]
public async Task Save();

// Generates an asynchronous RelayCommand and toggles IsBusy around an asynchronous, background‑thread command execution
[RelayCommand(CanExecute = nameof(CanSave), ExecuteOnBackgroundThread = true, AutoSetIsBusy = true)]
public async Task Save();
```

---

## 🎁 Wrapping DTOs with `ObservableDtoViewModel`

The `ObservableDtoViewModel` attribute generates ViewModels that wrap Data Transfer Objects (DTOs) or POCOs, automatically creating properties that forward to the underlying DTO while implementing `INotifyPropertyChanged`.

This is particularly useful when you want to:

- Wrap API response objects with change notification support
- Add ViewModel functionality to existing data models
- Create editable views of immutable data structures
- Track changes in your ViewModels with automatic `IsDirty` support

### 🛠 Quick Start: Wrapping a DTO

Given a simple DTO class or record:

```csharp
// Using a class
public class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? Age { get; set; }
}

// Or using a record
public record Person(
    string? FirstName,
    string? LastName,
    int? Age);
```

You can create a ViewModel wrapper with just an attribute:

```csharp
[ObservableDtoViewModel(typeof(Person))]
public partial class PersonViewModel : ViewModelBase
{
}
```

### ✨ Generated Code

The source generator automatically creates:

```csharp
public partial class PersonViewModel
{
    private readonly Person dto;

    public PersonViewModel(Person dto)
    {
        this.dto = dto;
    }

    public Person InnerModel => dto;

    public string? FirstName
    {
        get => dto.FirstName;
        set
        {
            if (dto.FirstName == value)
            {
                return;
            }

            dto.FirstName = value;
            RaisePropertyChanged(nameof(FirstName));
        }
    }

    public string? LastName
    {
        get => dto.LastName;
        set
        {
            if (dto.LastName == value)
            {
                return;
            }

            dto.LastName = value;
            RaisePropertyChanged(nameof(LastName));
        }
    }

    public int? Age
    {
        get => dto.Age;
        set
        {
            if (dto.Age == value)
            {
                return;
            }

            dto.Age = value;
            RaisePropertyChanged(nameof(Age));
        }
    }

    public override string ToString()
        => $"{nameof(FirstName)}: {FirstName}, {nameof(LastName)}: {LastName}, {nameof(Age)}: {Age}";
}
```

### 🔍 How It Works

- All public properties with a setter from the DTO are wrapped
- Each property getter returns the value from the underlying DTO
- Each property setter updates the DTO and raises `PropertyChanged`
- The DTO is injected through the constructor
- An `InnerModel` property provides access to the underlying DTO instance
- Nullable annotations are preserved
- **Supports both classes and records** – works seamlessly with modern C# record types

### 🐆 Record Support with Immutability

When wrapping C# records, the generator intelligently handles immutability using `with` expressions for primary constructor parameters:

```csharp
// Record with primary constructor parameters
public record Person(
    string? FirstName,
    string? LastName)
{
    // Regular property (mutable)
    public int? Age { get; set; }
}

[ObservableDtoViewModel(typeof(Person))]
public partial class PersonViewModel : ViewModelBase
{
}
```

**Generated code for records:**

```csharp
public partial class PersonViewModel
{
    private Person dto;  // Note: NOT readonly for records with primary constructor parameters

    public PersonViewModel(Person dto)
    {
        this.dto = dto;
    }

    // Primary constructor parameter: uses 'with' expression
    public string? FirstName
    {
        get => dto.FirstName;
        set
        {
            if (dto.FirstName == value) return;
            dto = dto with { FirstName = value };  // Immutable update
            RaisePropertyChanged(nameof(FirstName));
        }
    }

    // Primary constructor parameter: uses 'with' expression
    public string? LastName
    {
        get => dto.LastName;
        set
        {
            if (dto.LastName == value) return;
            dto = dto with { LastName = value };  // Immutable update
            RaisePropertyChanged(nameof(LastName));
        }
    }

    // Regular property: uses direct assignment
    public int? Age
    {
        get => dto.Age;
        set
        {
            if (dto.Age == value) return;
            dto.Age = value;  // Direct assignment
            RaisePropertyChanged(nameof(Age));
        }
    }
}
```

**Key differences for records:**

- **Primary constructor parameters** are immutable, so the generator uses `with` expressions to create a new record instance
- **Regular properties** on records can be mutated directly, so they use standard assignment
- The `dto` field is **not readonly** when the record has primary constructor parameters, allowing reassignment with `with` expressions
- The `dto` field **is readonly** for regular classes or records with only regular properties

### 🖨️ ToString Generation

The generator automatically creates a `ToString()` override based on the DTO's implementation:

**When the DTO has a custom ToString:**

```csharp
public class Person
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public override string ToString()
        => $"Person: {FirstName} {LastName}";
}
```

**Generated ViewModel ToString delegates to the DTO:**

```csharp
public override string ToString()
    => dto?.ToString() ?? base.ToString();
```

**When the DTO doesn't have a custom ToString:**

```csharp
public class Person
{
    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int? Age { get; set; }
}
```

**Generated ViewModel ToString lists all properties:**

```csharp
public override string ToString()
    => $"{nameof(FirstName)}: {FirstName}, {nameof(LastName)}: {LastName}, {nameof(Age)}: {Age}";
```

**Note:** Records have a compiler-generated `ToString()`, which is **not** considered a custom implementation. The generator will create a property-listing `ToString()` for records unless you explicitly override it.

### �🎯 Real-World Example: API Response Wrapper

```csharp
// API response DTO
public class UserDto
{
    public string Username { get; set; }

    public string Email { get; set; }

    public bool IsActive { get; set; }
}

// ViewModel wrapper
[ObservableDtoViewModel(typeof(UserDto))]
public partial class UserViewModel : ViewModelBase
{
    // Add custom commands or additional properties here
    [RelayCommand]
    private async Task SaveChanges()
    {
        // Save the modified dto to the API
        await userService.UpdateUserAsync(dto);
    }
}
```

### 📝 Usage in XAML

```xml
<UserControl>
    <StackPanel>
        <TextBox Text="{Binding Username, UpdateSourceTrigger=PropertyChanged}" />
        <TextBox Text="{Binding Email, UpdateSourceTrigger=PropertyChanged}" />
        <CheckBox IsChecked="{Binding IsActive}" Content="Active" />
        <Button Command="{Binding SaveChangesCommand}" Content="Save" />
    </StackPanel>
</UserControl>
```

**Note:** The `ObservableDtoViewModel` attribute requires that your ViewModel inherits from `ViewModelBase`, `ObservableObject`, or implements `INotifyPropertyChanged` with a `RaisePropertyChanged` method.

### 🔒 Readonly Properties Support

The generator automatically handles readonly properties (properties without setters) from your DTO, generating them as pass-through readonly properties in the ViewModel:

**DTO with readonly property:**

```csharp
public record Person(
    string? FirstName,
    string? LastName)
{
    // Readonly property (no setter)
    public int? Age { get; }
}
```

**Generated ViewModel:**

```csharp
public partial class PersonViewModel
{
    private Person dto;

    // Regular properties with setters (writable)
    public string? FirstName
    {
        get => dto.FirstName;
        set
        {
            if (dto.FirstName == value) return;
            dto = dto with { FirstName = value };
            RaisePropertyChanged(nameof(FirstName));
            IsDirty = true;
        }
    }

    public string? LastName
    {
        get => dto.LastName;
        set
        {
            if (dto.LastName == value) return;
            dto = dto with { LastName = value };
            RaisePropertyChanged(nameof(LastName));
            IsDirty = true;
        }
    }

    // Readonly property (no setter generated)
    public int? Age => dto.Age;
}
```

**Key points:**

- ✅ Readonly properties are generated as expression-bodied properties
- ✅ No setter is generated, maintaining immutability
- ✅ The property is still accessible in the ViewModel and can be bound in XAML
- ✅ Useful for calculated properties, timestamps, IDs, or other immutable data
- ✅ Works with both classes and records

**Common use cases for readonly properties:**

- **Identifiers** - Database IDs, GUIDs that shouldn't be changed
- **Timestamps** - Created date, last modified date
- **Calculated values** - Full names, total amounts, computed status
- **Metadata** - Version numbers, entity types

### 🔧 Method Proxies Support

The generator automatically creates proxy methods for public methods in your DTO, allowing you to call DTO methods through the ViewModel:

**DTO with methods:**

```csharp
public class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string GetFullName()
    {
        return $"{FirstName} {LastName}";
    }

    public string FormatAddress(string street, int zipCode)
    {
        return $"{street}, {zipCode}";
    }
}
```

**Generated ViewModel with method proxies:**

```csharp
public partial class PersonViewModel
{
    private readonly Person dto;

    // Properties...

    // Proxy method without parameters
    public string GetFullName()
        => dto.GetFullName();

    // Proxy method with parameters
    public string FormatAddress(string street, int zipCode)
        => dto.FormatAddress(street, zipCode);
}
```

**Using proxy methods:**

```csharp
var viewModel = new PersonViewModel(new Person 
{ 
    FirstName = "John", 
    LastName = "Doe" 
});

// Call methods on the ViewModel, which delegates to the DTO
string fullName = viewModel.GetFullName();
string address = viewModel.FormatAddress("Main St", 12345);
```

**Method proxy features:**

- ✅ All public instance methods are proxied
- ✅ Parameters are preserved with correct types
- ✅ Return types are maintained
- ✅ Methods are generated as expression-bodied members for clarity
- ✅ `ToString()` method is **excluded** (handled separately by the generator)
- ✅ Works with methods that return `void`, values, or objects

**Common use cases for method proxies:**

- **Business logic** - Calculations, validations, formatting
- **Helper methods** - Utility functions on the DTO
- **State checks** - IsValid(), CanProcess(), etc.
- **Data formatting** - GetDisplayName(), FormatCurrency(), etc.

**Note:** Static methods, property accessors, and compiler-generated methods are not proxied. The generator focuses on explicit public instance methods you've defined.

### 🚫 Selective Generation with IgnorePropertyNames and IgnoreMethodNames

Sometimes you don't want to generate ViewModels for all properties or methods in your DTO. The `IgnorePropertyNames` and `IgnoreMethodNames` options let you exclude specific members from generation:

#### Ignoring Properties

Use `IgnorePropertyNames` to exclude specific properties from the generated ViewModel:

**DTO with multiple properties:**

```csharp
public class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public int? Age { get; set; }
    public string? InternalId { get; set; }  // Internal use only
}
```

**Exclude properties using IgnorePropertyNames:**

```csharp
[ObservableDtoViewModel(
    typeof(Person), 
    IgnorePropertyNames = [nameof(Person.Age), nameof(Person.InternalId)])]
public partial class PersonViewModel : ViewModelBase
{
}
```

**Generated code (Age and InternalId excluded):**

```csharp
public partial class PersonViewModel
{
    private readonly Person dto;

    // Only FirstName and LastName are generated
    public string? FirstName
    {
        get => dto.FirstName;
        set
        {
            if (dto.FirstName == value) return;
            dto.FirstName = value;
            RaisePropertyChanged(nameof(FirstName));
            IsDirty = true;
        }
    }

    public string? LastName
    {
        get => dto.LastName;
        set
        {
            if (dto.LastName == value) return;
            dto.LastName = value;
            RaisePropertyChanged(nameof(LastName));
            IsDirty = true;
        }
    }

    // Age and InternalId are NOT generated
    // ToString also excludes ignored properties
    public override string ToString()
        => $"{nameof(FirstName)}: {FirstName}, {nameof(LastName)}: {LastName}";
}
```

#### Ignoring Methods

Use `IgnoreMethodNames` to exclude specific methods from being proxied:

**DTO with multiple methods:**

```csharp
public class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }

    public string GetFullName()
    {
        return $"{FirstName} {LastName}";
    }

    public void InternalCalculation()
    {
        // Complex internal logic that shouldn't be exposed
    }
}
```

**Exclude methods using IgnoreMethodNames:**

```csharp
[ObservableDtoViewModel(
    typeof(Person), 
    IgnoreMethodNames = [nameof(Person.InternalCalculation)])]
public partial class PersonViewModel : ViewModelBase
{
}
```

**Generated code (InternalCalculation excluded):**

```csharp
public partial class PersonViewModel
{
    // All properties are generated...

    // GetFullName IS generated
    public string GetFullName()
        => dto.GetFullName();

    // InternalCalculation is NOT generated
}
```

#### Combining Both Options

You can use both `IgnorePropertyNames` and `IgnoreMethodNames` together:

```csharp
[ObservableDtoViewModel(
    typeof(Person),
    IgnorePropertyNames = [nameof(Person.Age), nameof(Person.InternalId)],
    IgnoreMethodNames = [nameof(Person.InternalCalculation), nameof(Person.Debug)])]
public partial class PersonViewModel : ViewModelBase
{
}
```

**Common use cases for ignoring members:**

- **Internal properties** - Database IDs, internal state that shouldn't be editable
- **Calculated properties** - Properties you'll override with custom logic
- **Sensitive data** - Properties that require special handling or encryption
- **Internal methods** - Debug methods, internal calculations, private business logic
- **Complex methods** - Methods requiring special error handling or orchestration
- **Legacy methods** - Methods kept for backward compatibility but not exposed in UI

**Benefits:**

- ✅ Reduces generated code size
- ✅ Prevents exposing internal implementation details
- ✅ Gives you control over the ViewModel surface area
- ✅ Allows custom implementations for specific properties/methods
- ✅ Automatically updates `ToString()` to exclude ignored properties
- ✅ Works with both classes and records

**Note:** Property and method names are matched using **case-sensitive** comparison. Use `nameof()` expressions to avoid typos.

### � Accessing the Underlying DTO

The generated ViewModel automatically includes an `InnerModel` property that provides direct access to the wrapped DTO instance:

```csharp
[ObservableDtoViewModel(typeof(Person))]
public partial class PersonViewModel : ViewModelBase
{
}

// Usage:
var viewModel = new PersonViewModel(new Person 
{ 
    FirstName = "John", 
    LastName = "Doe" 
});

// Access the underlying DTO
Person dto = viewModel.InnerModel;

// Pass to a service or repository
await _personRepository.SaveAsync(viewModel.InnerModel);

// Send via API
var response = await _httpClient.PostAsJsonAsync("/api/persons", viewModel.InnerModel);
```

**Common use cases:**

- **Persistence** - Save the DTO to a database or file
- **API calls** - Send the DTO to a REST API or gRPC service
- **Serialization** - Convert the DTO to JSON, XML, or other formats
- **Business logic** - Pass the DTO to domain services or validators
- **Unit testing** - Verify the ViewModel correctly updates the underlying data

**Why `InnerModel`?**

The property is named `InnerModel` to:

- ✅ Clearly indicate it's the internal/wrapped model
- ✅ Minimize naming conflicts with DTO properties
- ✅ Follow common wrapper/decorator pattern conventions
- ✅ Maintain consistency across your codebase

**Example with repository pattern:**

```csharp
public class PersonEditViewModel : ViewModelBase
{
    private readonly IPersonRepository repository;

    [ObservableDtoViewModel(typeof(PersonDto))]
    public partial class PersonViewModel : ViewModelBase
    {
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        if (!ValidateModel())
        {
            return;
        }

        // Get the DTO and save it
        await repository.UpdateAsync(PersonViewModel.InnerModel);
        
        MessageBox.Show("Person saved successfully!");
        IsDirty = false;
    }
}
```

### ✅ Validation Support with Data Annotations

The `ObservableDtoViewModel` generator automatically copies validation attributes from your DTO properties to the generated ViewModel properties. This enables seamless validation using Data Annotations without duplicating attribute definitions.

**Key features:**

- ✅ **Automatic attribute copying** - Validation attributes from DTO are applied to ViewModel properties
- ✅ **Validation on property change** - Enable real-time validation as users type
- ✅ **Validation on initialization** - Optionally validate all properties when the ViewModel is created
- ✅ **Full attribute preservation** - All attribute parameters and error messages are maintained
- ✅ **Works with all validation attributes** - `Required`, `Range`, `MinLength`, `MaxLength`, `EmailAddress`, etc.

#### Basic Validation Example

**DTO with validation attributes:**

```csharp
using System.ComponentModel.DataAnnotations;

public class Person
{
    [Required(ErrorMessage = "First name is required")]
    [MinLength(2, ErrorMessage = "First name must be at least 2 characters long")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName { get; set; }

    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName { get; set; }

    [Required(ErrorMessage = "Age is required")]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int? Age { get; set; }

    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string? Email { get; set; }
}
```

**ViewModel with validation enabled:**

```csharp
[ObservableDtoViewModel(
    typeof(Person),
    EnableValidationOnPropertyChanged = true,
    EnableValidationOnInit = false)]
public partial class PersonViewModel : ViewModelBase
{
}
```

**Generated code includes copied attributes:**

```csharp
public partial class PersonViewModel
{
    private readonly Person dto;

    public PersonViewModel(Person dto)
    {
        this.dto = dto;

        InitializeValidation(
            validateOnPropertyChanged: true,
            validateAllPropertiesOnInit: false);
    }

    public Person InnerModel => dto;

    [Required(ErrorMessage = "First name is required")]
    [MinLength(2, ErrorMessage = "First name must be at least 2 characters long")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
    public string? FirstName
    {
        get => dto.FirstName;
        set
        {
            if (dto.FirstName == value)
            {
                return;
            }

            dto.FirstName = value;
            RaisePropertyChanged(nameof(FirstName));
            IsDirty = true;
        }
    }

    [Required(ErrorMessage = "Last name is required")]
    [MinLength(2, ErrorMessage = "Last name must be at least 2 characters long")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
    public string? LastName
    {
        get => dto.LastName;
        set
        {
            if (dto.LastName == value)
            {
                return;
            }

            dto.LastName = value;
            RaisePropertyChanged(nameof(LastName));
            IsDirty = true;
        }
    }

    [Required(ErrorMessage = "Age is required")]
    [Range(18, 120, ErrorMessage = "Age must be between 18 and 120")]
    public int? Age
    {
        get => dto.Age;
        set
        {
            if (dto.Age == value)
            {
                return;
            }

            dto.Age = value;
            RaisePropertyChanged(nameof(Age));
            IsDirty = true;
        }
    }

    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string? Email
    {
        get => dto.Email;
        set
        {
            if (dto.Email == value)
            {
                return;
            }

            dto.Email = value;
            RaisePropertyChanged(nameof(Email));
            IsDirty = true;
        }
    }

    // ... other properties and methods
}
```

#### Validation Configuration Options

**`EnableValidationOnPropertyChanged`**

When `true`, the generator adds an `InitializeValidation` call that validates each property as it changes:

```csharp
[ObservableDtoViewModel(
    typeof(Person),
    EnableValidationOnPropertyChanged = true)]
public partial class PersonViewModel : ViewModelBase
{
}
```

This provides **real-time validation** as users type, with immediate feedback.

**`EnableValidationOnInit`**

When `true`, all properties are validated immediately when the ViewModel is created:

```csharp
[ObservableDtoViewModel(
    typeof(Person),
    EnableValidationOnPropertyChanged = true,
    EnableValidationOnInit = true)]
public partial class PersonViewModel : ViewModelBase
{
}
```

**When to use `EnableValidationOnInit`:**

- ✅ **Edit forms** - When loading existing data that should be pre-validated
- ✅ **Required field indicators** - Show which fields need attention before submission
- ❌ **New/empty forms** - Don't use for empty forms (poor UX to show errors immediately)

#### Complete Form Example with Validation

**DTO:**

```csharp
public class CustomerDto
{
    [Required(ErrorMessage = "Customer name is required")]
    [MinLength(3, ErrorMessage = "Name must be at least 3 characters")]
    [MaxLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Phone number is required")]
    [Phone(ErrorMessage = "Please enter a valid phone number")]
    public string? Phone { get; set; }

    [Range(18, 150, ErrorMessage = "Age must be between 18 and 150")]
    public int? Age { get; set; }
}
```

**ViewModel:**

```csharp
[ObservableDtoViewModel(
    typeof(CustomerDto),
    EnableValidationOnPropertyChanged = true,
    EnableValidationOnInit = false)]
public partial class CustomerEditViewModel : ViewModelBase
{
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        if (!ValidateAllProperties())
        {
            MessageBox.Show("Please fix all validation errors before saving.", "Validation Error");
            return;
        }

        // Save the customer
        await _customerRepository.SaveAsync(InnerModel);
        
        MessageBox.Show("Customer saved successfully!", "Success");
        IsDirty = false;
    }

    private bool CanSave() => !HasErrors && IsDirty;
}
```

**XAML (WPF example):**

```xml
<UserControl>
    <StackPanel Margin="10">
        <TextBlock Text="Customer Name *" />
        <TextBox Text="{Binding Name, 
                               UpdateSourceTrigger=PropertyChanged, 
                               ValidatesOnNotifyDataErrors=True}" />

        <TextBlock Text="Email *" Margin="0,10,0,0" />
        <TextBox Text="{Binding Email, 
                               UpdateSourceTrigger=PropertyChanged, 
                               ValidatesOnNotifyDataErrors=True}" />

        <TextBlock Text="Phone *" Margin="0,10,0,0" />
        <TextBox Text="{Binding Phone, 
                               UpdateSourceTrigger=PropertyChanged, 
                               ValidatesOnNotifyDataErrors=True}" />

        <TextBlock Text="Age" Margin="0,10,0,0" />
        <TextBox Text="{Binding Age, 
                               UpdateSourceTrigger=PropertyChanged, 
                               ValidatesOnNotifyDataErrors=True}" />

        <StackPanel Orientation="Horizontal" Margin="0,20,0,0">
            <Button Content="Save" 
                    Command="{Binding SaveCommand}" 
                    Padding="20,5" />
            <TextBlock Text="{Binding HasErrors}" 
                       Margin="10,0,0,0" 
                       VerticalAlignment="Center" />
        </StackPanel>
    </StackPanel>
</UserControl>
```

#### Benefits of Attribute Copying

**Before (manual duplication):**

```csharp
// DTO
public class Person
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(2)]
    public string? Name { get; set; }
}

// ViewModel - manually duplicating attributes
public class PersonViewModel : ViewModelBase
{
    private Person dto;

    [Required(ErrorMessage = "Name is required")]  // Duplicated!
    [MinLength(2)]                                  // Duplicated!
    public string? Name
    {
        get => dto.Name;
        set
        {
            dto.Name = value;
            RaisePropertyChanged();
        }
    }
}
```

**After (automatic with generator):**

```csharp
// DTO - single source of truth
public class Person
{
    [Required(ErrorMessage = "Name is required")]
    [MinLength(2)]
    public string? Name { get; set; }
}

// ViewModel - attributes automatically copied
[ObservableDtoViewModel(
    typeof(Person),
    EnableValidationOnPropertyChanged = true)]
public partial class PersonViewModel : ViewModelBase
{
    // Attributes automatically copied from DTO!
}
```

**Advantages:**

- ✅ **No duplication** - Validation rules defined once in the DTO
- ✅ **Consistency** - ViewModel and DTO always have matching validation
- ✅ **Maintainability** - Change validation in one place, it updates everywhere
- ✅ **Type safety** - Generator ensures attribute syntax is preserved correctly
- ✅ **Less code** - No manual attribute copying needed

#### Validation Best Practices with ObservableDtoViewModel

**✅ Do's:**

- ✅ Define validation attributes in your DTO (single source of truth)
- ✅ Use `EnableValidationOnPropertyChanged = true` for real-time validation
- ✅ Check `HasErrors` before saving data
- ✅ Use `ValidateAllProperties()` before form submission
- ✅ Disable save buttons with `CanExecute = nameof(CanSave)` and `!HasErrors`

**❌ Don'ts:**

- ❌ Don't set `EnableValidationOnInit = true` for empty/new forms
- ❌ Don't duplicate validation attributes in both DTO and ViewModel
- ❌ Don't ignore validation errors when saving
- ❌ Don't forget to set `UpdateSourceTrigger=PropertyChanged` in XAML bindings

---

### 🔄 Change Tracking with `IsDirty`

The `ObservableDtoViewModel` generator automatically adds `IsDirty = true;` to all property setters when your ViewModel inherits from `ViewModelBase`. This provides automatic change tracking without any additional code.

**Default Behavior (IsDirty enabled):**

```csharp
// DTO
public class Person
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
}

// ViewModel with automatic IsDirty tracking
[ObservableDtoViewModel(typeof(Person))]
public partial class PersonViewModel : ViewModelBase
{
}
```

**Generated code includes IsDirty tracking:**

```csharp
public partial class PersonViewModel
{
    private readonly Person dto;

    public PersonViewModel(Person dto)
    {
        this.dto = dto;
    }

    public string? FirstName
    {
        get => dto.FirstName;
        set
        {
            if (dto.FirstName == value)
            {
                return;
            }

            dto.FirstName = value;
            RaisePropertyChanged(nameof(FirstName));
            IsDirty = true;  // Automatically added!
        }
    }

    public string? LastName
    {
        get => dto.LastName;
        set
        {
            if (dto.LastName == value)
            {
                return;
            }

            dto.LastName = value;
            RaisePropertyChanged(nameof(LastName));
            IsDirty = true;  // Automatically added!
        }
    }
}
```

**Disabling IsDirty tracking:**

If you don't want automatic `IsDirty` tracking, you can disable it:

```csharp
[ObservableDtoViewModel(typeof(Person), UseIsDirty = false)]
public partial class PersonViewModel : ViewModelBase
{
}
```

**When is UseIsDirty available?**

- ✅ **Enabled by default** when your ViewModel inherits from `ViewModelBase`
- ❌ **Not available** when using `ObservableObject` or custom base classes without `IsDirty` property
- 🔧 **Can be disabled** by setting `UseIsDirty = false` in the attribute

**Use cases for IsDirty:**

- **Form tracking** - Know when the user has modified any field
- **Save prompts** - Ask "Do you want to save changes?" before closing
- **Validation triggers** - Only validate when changes have been made
- **Change indicators** - Show visual indicators (e.g., asterisk) when data is modified
- **Undo/Redo** - Track whether there are unsaved changes to enable/disable buttons

**Example with form tracking:**

```csharp
[ObservableDtoViewModel(typeof(CustomerDto))]
public partial class CustomerEditViewModel : ViewModelBase
{
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        // Save changes
        await repository.UpdateAsync(dto);
        IsDirty = false;  // Reset after save
    }

    private bool CanSave() => IsDirty;
}
```

In XAML:

```xml
<TextBlock Text="*" Foreground="Red" 
           Visibility="{Binding IsDirty, 
                               Converter={StaticResource BoolToVisibilityConverter}}" />
<Button Content="Save" Command="{Binding SaveCommand}" />
```

---

## 🎯 Real-World Use Cases

### 📅 Scenario 1: A User Profile Form

```csharp
public partial class UserProfileViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    private string firstName;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    private string lastName;

    public string FullName => $"{FirstName} {LastName}";

    [RelayCommand]
    private void SaveProfile()
    {
        MessageBox.Show($"Profile Saved: {FullName}");
    }
}
```

#### 🔗 XAML Binding where Context is UserProfileViewModel

```xml
<TextBox Text="{Binding FirstName, UpdateSourceTrigger=PropertyChanged}" />
<TextBox Text="{Binding LastName, UpdateSourceTrigger=PropertyChanged}" />
<TextBlock Text="{Binding FullName}" />
<Button Content="Save" Command="{Binding SaveProfileCommand}" />
```

#### 🔥 Result for UserProfileViewModel binding

The FullName property updates automatically when FirstName or LastName changes

### 📑 Scenario 2: Fetching Data from an API

A ViewModel that fetches data asynchronously and enables/disables a button based on loading state.

```csharp
public partial class DataViewModel : ViewModelBase
{
    [ObservableProperty]
    private string? data;

    [ObservableProperty]
    private bool isLoading;

    [RelayCommand(CanExecute = nameof(CanFetchData))]
    private async Task FetchData(CancellationToken cancellationToken)
    {
        IsLoading = true;
        await Task.Delay(2000, cancellationToken).ConfigureAwait(false); // Simulate API call
        Data = "Fetched Data from API";
        IsLoading = false;
    }

    private bool CanFetchData() => !IsLoading;
}
```

#### 🔗 XAML Binding where Context is DataViewModel

```xml
<Button Command="{Binding Path=FetchDataCommand}" Content="Fetch Data" />

<TextBlock Text="{Binding Path=Data}" />
```

#### 🔥 Result for DataViewModel binding

The button is disabled while data is being fetched, preventing multiple API calls.

## 🛠 Troubleshooting

### 🚧 Properties Are Not Updating in UI?

✅ Ensure your ViewModel inherits from ViewModelBase, which includes INotifyPropertyChanged.

```csharp
public partial class MyViewModel : ViewModelBase { }
```

### 🚧 Commands Are Not Executing?

✅ Check if your command has a valid CanExecute method.

```csharp
[RelayCommand(CanExecute = nameof(CanSave))]
private void Save() { /* ... */ }

private bool CanSave() => !string.IsNullOrEmpty(Name);
```

---

## 📌 Summary

- ✔️ **Use** `ObservableProperty` to eliminate manual property creation.
- ✔️ **Use** `NotifyPropertyChangedFor` to notify dependent properties.
- ✔️ **Use** `RelayCommand` for automatic command generation.
- ✔️ **Use async commands** for better UI responsiveness.
- ✔️ **Improve performance** by leveraging `CanExecute` for commands.

### 🚀 Why Use Atc.Wpf Source Generators?

- ✅ **Reduces boilerplate** – Write less code, get more done.
- ✅ **Improves maintainability** – Focus on business logic instead of plumbing.
- ✅ **Enhances MVVM architecture** – Ensures best practices in WPF development.

---

## 🔎 Behind the scenes

### 📝 Human-Written Code - for simple example

```csharp
public partial class TestViewModel : ViewModelBase
{
    [ObservableProperty]
    private string name;
}
```

### ⚙️ Auto-Generated Code - for simple example

```csharp
public partial class TestViewModel
{
    public string Name
    {
        get => name;
        set
        {
            if (name == value)
            {
                return;
            }

            name = value;
            RaisePropertyChanged(nameof(Name));
        }
    }
}
```

### 📝 Human-Written Code - for advanced example

```csharp
public partial class PersonViewModel : ViewModelBase
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName))]
    [Required]
    [MinLength(2)]
    private string firstName = "John";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FullName), nameof(Age))]
    [NotifyPropertyChangedFor(nameof(Email))]
    [NotifyPropertyChangedFor(nameof(TheProperty))]
    private string? lastName = "Doe";

    [ObservableProperty]
    private int age = 27;

    [ObservableProperty]
    private string? email;

    [ObservableProperty(nameof(TheProperty), nameof(FullName), nameof(Age))]
    private string? myTestProperty;

    public string FullName => $"{FirstName} {LastName}";

    [RelayCommand]
    public void ShowData()
    {
        // TODO: Implement ShowData - it could be a dialog box
    }

    [RelayCommand(CanExecute = nameof(CanSaveHandler))]
    public void SaveHandler()
    {
        var dialogBox = new InfoDialogBox(
            Application.Current.MainWindow!,
            new DialogBoxSettings(DialogBoxType.Ok),
            "Hello to SaveHandler method");

        dialogBox.Show();
    }

    public bool CanSaveHandler()
    {
        // TODO: Implement validation
        return true;
    }
}
```

### ⚙️ Auto-Generated Code - for advanced example

```csharp
public partial class PersonViewModel
{
    public IRelayCommand ShowDataCommand => new RelayCommand(ShowData);

    public IRelayCommand SaveHandlerCommand => new RelayCommand(SaveHandler, CanSaveHandler);

    public string FirstName
    {
        get => firstName;
        set
        {
            if (firstName == value)
            {
                return;
            }

            firstName = value;
            RaisePropertyChanged(nameof(FirstName));
            RaisePropertyChanged(nameof(FullName));
        }
    }

    public string? LastName
    {
        get => lastName;
        set
        {
            if (lastName == value)
            {
                return;
            }

            lastName = value;
            RaisePropertyChanged(nameof(LastName));
            RaisePropertyChanged(nameof(FullName));
            RaisePropertyChanged(nameof(Age));
            RaisePropertyChanged(nameof(Email));
            RaisePropertyChanged(nameof(TheProperty));
        }
    }

    public int Age
    {
        get => age;
        set
        {
            if (age == value)
            {
                return;
            }

            age = value;
            RaisePropertyChanged(nameof(Age));
        }
    }

    public string? Email
    {
        get => email;
        set
        {
            if (email == value)
            {
                return;
            }

            email = value;
            RaisePropertyChanged(nameof(Email));
        }
    }

    public string? TheProperty
    {
        get => myTestProperty;
        set
        {
            if (myTestProperty == value)
            {
                return;
            }

            myTestProperty = value;
            RaisePropertyChanged(nameof(TheProperty));
            RaisePropertyChanged(nameof(FullName));
            RaisePropertyChanged(nameof(Age));
        }
    }
}
```
