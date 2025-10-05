# 🔔 Messaging System

The Atc.XamlToolkit provides a powerful **Messenger** pattern implementation for decoupled communication between ViewModels and other components.

## Why Use Messaging?

- ✅ **Loose Coupling** - Components don't need direct references to each other
- ✅ **Scalability** - Easy to add new message handlers without modifying existing code
- ✅ **Testability** - Mock message sending/receiving in unit tests
- ✅ **Clean Architecture** - Follows SOLID principles and separation of concerns

## Getting Started

### Basic Message Sending

```csharp
// Send a simple notification
Messenger.Default.Send(new NotificationMessage("RefreshData"));

// Send typed data
Messenger.Default.Send(new GenericMessage<User>(currentUser));
```

### Registering for Messages

```csharp
public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        // Register for messages
        Messenger.Default.Register<NotificationMessage>(this, HandleNotification);
        Messenger.Default.Register<GenericMessage<User>>(this, HandleUserMessage);
    }

    private void HandleNotification(NotificationMessage msg)
    {
        if (msg.Notification == "RefreshData")
        {
            // Refresh your data...
        }
    }

    private void HandleUserMessage(GenericMessage<User> msg)
    {
        var user = msg.Content;
        // Handle user data...
    }

    protected override void Cleanup()
    {
        // Don't forget to unregister!
        Messenger.Default.Unregister(this);
        base.Cleanup();
    }
}
```

## Message Types

### GenericMessage\<T\>

Send typed data between components:

```csharp
// Send a string message
Messenger.Default.Send(new GenericMessage<string>("Hello World"));

// Send an integer
Messenger.Default.Send(new GenericMessage<int>(42));

// Send a complex object
Messenger.Default.Send(
    new GenericMessage<User>(
        new User
        {
            Id = 1,
            Name = "John Doe"
        }));
```

### NotificationMessage

Simple string-based notifications for triggering actions:

```csharp
// Send a notification
Messenger.Default.Send(new NotificationMessage("UserLoggedOut"));

// Receive and handle
Messenger.Default.Register<NotificationMessage>(this, msg =>
{
    switch (msg.Notification)
    {
        case "UserLoggedOut":
            // Handle logout...
            break;
        case "RefreshData":
            // Refresh data...
            break;
    }
});
```

### NotificationMessageAction

Messages with callback actions that receivers can execute:

```csharp
// Sender side
var message = new NotificationMessageAction(
    "ConfirmDelete",
    () => Console.WriteLine("Delete confirmed!"));

Messenger.Default.Send(message);

// Receiver side
Messenger.Default.Register<NotificationMessageAction>(this, msg =>
{
    if (msg.Notification == "ConfirmDelete")
    {
        // Show confirmation dialog...
        if (userConfirmed)
        {
            msg.Execute(); // Execute the callback
        }
    }
});
```

### NotificationMessageAction\<TCallbackParameter\>

Messages with parameterized callbacks:

```csharp
// Sender side
var message = new NotificationMessageAction<bool>(
    "ConfirmDelete",
    confirmed =>
    {
        if (confirmed)
        {
            DeleteItem();
        }
    });

Messenger.Default.Send(message);

// Receiver side
Messenger.Default.Register<NotificationMessageAction<bool>>(this, msg =>
{
    if (msg.Notification == "ConfirmDelete")
    {
        var result = ShowConfirmationDialog();
        msg.Execute(result); // Pass result to callback
    }
});
```

### PropertyChangedMessage\<T\>

Broadcast property value changes across the application:

```csharp
// Send property change notification
Messenger.Default.Send(new PropertyChangedMessage<string>(
    sender: this,
    oldValue: "John",
    newValue: "Jane",
    propertyName: "UserName"));

// Receive and react to property changes
Messenger.Default.Register<PropertyChangedMessage<string>>(this, msg =>
{
    if (msg.PropertyName == "UserName")
    {
        Console.WriteLine($"UserName changed from {msg.OldValue} to {msg.NewValue}");
    }
});
```

### NotificationMessageWithCallback

Generic notification with callback support:

```csharp
// Sender side
var message = new NotificationMessageWithCallback(
    "RequestData",
    () =>
    {
        // Callback when data is ready
        ProcessData();
    });

Messenger.Default.Send(message);

// Receiver side
Messenger.Default.Register<NotificationMessageWithCallback>(this, msg =>
{
    if (msg.Notification == "RequestData")
    {
        // Prepare data...
        msg.Execute(); // Notify sender that data is ready
    }
});
```

## Best Practices

### 1. Always Unregister

Memory leaks can occur if you don't unregister message handlers:

```csharp
public class MyViewModel : ViewModelBase
{
    public MyViewModel()
    {
        Messenger.Default.Register<NotificationMessage>(this, HandleMessage);
    }

    protected override void Cleanup()
    {
        // Critical: Unregister to prevent memory leaks
        Messenger.Default.Unregister(this);
        base.Cleanup();
    }

    private void HandleMessage(NotificationMessage msg)
    {
        // Handle message...
    }
}
```

### 2. Use Typed Messages

Prefer `GenericMessage<T>` over string-based messages for type safety:

```csharp
// ❌ Avoid: Stringly-typed
Messenger.Default.Send(new NotificationMessage("User:John:Age:30"));

// ✅ Better: Strongly-typed
Messenger.Default.Send(
    new GenericMessage<User>(
        new User
        {
            Name = "John",
            Age = 30
        }));
```

### 3. Define Message Constants

Create a central location for all message identifiers:

```csharp
public static class MessageConstants
{
    public const string RefreshData = nameof(RefreshData);
    public const string UserLoggedIn = nameof(UserLoggedIn);
    public const string UserLoggedOut = nameof(UserLoggedOut);
    public const string NavigateToSettings = nameof(NavigateToSettings);
}

// Usage
Messenger.Default.Send(new NotificationMessage(MessageConstants.RefreshData));
```

### 4. Use Message Tokens for Scoping

Target messages to specific recipients using tokens:

```csharp
// Send with token
Messenger.Default.Send(
    new GenericMessage<string>("Data"),
    token: "ViewGroup1");

// Only recipients registered with "ViewGroup1" token will receive it
Messenger.Default.Register<GenericMessage<string>>(
    this,
    "ViewGroup1", // Token
    HandleMessage);
```

### 5. Keep Messages Simple

Messages should be simple Data Transfer Objects (DTOs):

```csharp
// ✅ Good: Simple DTO
public class UserLoggedInMessage
{
    public int UserId { get; set; }

    public string UserName { get; set; }

    public DateTime LoginTime { get; set; }
}

// ❌ Avoid: Complex objects with behavior
public class UserLoggedInMessage
{
    public User User { get; set; }

    public IUserRepository Repository { get; set; } // Don't pass services

    public void SaveToDatabase() { } // Don't add behavior
}
```

### 6. Document Your Message Contracts

Create documentation for all messages used in your application:

```csharp
/// <summary>
/// Messages used throughout the application.
/// </summary>
public static class AppMessages
{
    /// <summary>
    /// Sent when user authentication is successful.
    /// Payload: GenericMessage&lt;User&gt;
    /// </summary>
    public const string UserAuthenticated = nameof(UserAuthenticated);

    /// <summary>
    /// Sent to request navigation to a specific view.
    /// Payload: GenericMessage&lt;string&gt; (view name)
    /// </summary>
    public const string NavigateToView = nameof(NavigateToView);
}
```

## Advanced Scenarios

### Cross-Module Communication

Ideal for plugin architectures or modular applications:

```csharp
// Module A (Order Module)
public class OrderViewModel : ViewModelBase
{
    private void OnOrderCompleted()
    {
        Messenger.Default.Send(new GenericMessage<Order>(currentOrder));
    }
}

// Module B (Inventory Module)
public class InventoryViewModel : ViewModelBase
{
    public InventoryViewModel()
    {
        Messenger.Default.Register<GenericMessage<Order>>(this, OnOrderReceived);
    }

    private void OnOrderReceived(GenericMessage<Order> msg)
    {
        // Update inventory based on order
        UpdateInventory(msg.Content);
    }
}
```

### Broadcast Events

Notify multiple ViewModels of system-wide events:

```csharp
// Central service
public class ApplicationService
{
    public void NotifyThemeChanged(string newTheme)
    {
        Messenger.Default.Send(new GenericMessage<string>(newTheme));
    }
}

// Multiple ViewModels can listen
public class MainViewModel : ViewModelBase
{
    public MainViewModel()
    {
        Messenger.Default.Register<GenericMessage<string>>(this, OnThemeChanged);
    }

    private void OnThemeChanged(GenericMessage<string> msg)
    {
        ApplyTheme(msg.Content);
    }
}
```

### Request-Response Pattern

Use callbacks to implement request-response:

```csharp
// Requester
var message = new NotificationMessageAction<List<Customer>>(
    "GetCustomers",
    customers =>
    {
        // Handle the response
        DisplayCustomers(customers);
    });

Messenger.Default.Send(message);

// Provider
Messenger.Default.Register<NotificationMessageAction<List<Customer>>>(this, msg =>
{
    if (msg.Notification == "GetCustomers")
    {
        var customers = _repository.GetAll();
        msg.Execute(customers);
    }
});
```

## Testing with Messenger

### Unit Testing Message Sending

```csharp
[Fact]
public void Should_Send_Message_When_Action_Executed()
{
    // Arrange
    var viewModel = new MyViewModel();
    var messageReceived = false;

    Messenger.Default.Register<NotificationMessage>(this, msg =>
    {
        if (msg.Notification == "ActionCompleted")
        {
            messageReceived = true;
        }
    });

    // Act
    viewModel.ExecuteAction();

    // Assert
    Assert.True(messageReceived);

    // Cleanup
    Messenger.Default.Unregister(this);
}
```

### Mocking Message Reception

```csharp
[Fact]
public void Should_Handle_Message_Correctly()
{
    // Arrange
    var viewModel = new MyViewModel();
    var testUser = new User { Name = "Test" };

    // Act
    Messenger.Default.Send(new GenericMessage<User>(testUser));

    // Assert
    Assert.Equal("Test", viewModel.CurrentUser.Name);
}
```

## Common Patterns

### Navigation Message Pattern

```csharp
public class NavigationMessage
{
    public string ViewName { get; set; }
    public object Parameter { get; set; }
}

// Send
Messenger.Default.Send(
    new GenericMessage<NavigationMessage>(
        new NavigationMessage
        {
            ViewName = "DetailView",
            Parameter = userId
        }));
```

### Status Update Pattern

```csharp
public class StatusMessage
{
    public string Message { get; set; }
    public StatusLevel Level { get; set; }
}

// Send status updates
Messenger.Default.Send(
    new GenericMessage<StatusMessage>(
        new StatusMessage
        {
            Message = "Processing...",
            Level = StatusLevel.Info
        }));
```

### Dialog Request Pattern

```csharp
var dialogMessage = new NotificationMessageAction<bool>(
    "ShowConfirmDialog",
    confirmed =>
    {
        if (confirmed)
        {
            // Proceed with action
        }
    });

Messenger.Default.Send(dialogMessage);
```

## Summary

The Messenger system in Atc.XamlToolkit provides:

- ✅ Loose coupling between components
- ✅ Type-safe message passing
- ✅ Support for callbacks and request-response patterns
- ✅ Token-based message scoping
- ✅ Easy to test and maintain

Use it to create clean, maintainable MVVM applications with minimal dependencies between ViewModels.
