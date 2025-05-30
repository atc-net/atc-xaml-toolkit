# 🧱 MVVM in Avalonia or WPF

## 🪟 For Avaloina

Avaloina fully supports the **Model-View-ViewModel (MVVM)** pattern, which promotes a clear separation of concerns between the UI and business logic.

The **Atc.XamlToolkit.Avaloina** library provides a robust foundation for implementing MVVM effectively, reducing boilerplate code and simplifying development.

## 🖼️ For WPF

Windows Presentation Foundation (WPF) fully supports the **Model-View-ViewModel (MVVM)** pattern, which promotes a clear separation of concerns between the UI and business logic.

The **Atc.XamlToolkit.Wpf** library provides a robust foundation for implementing MVVM effectively, reducing boilerplate code and simplifying development.

## ⚙️ Features

The `Atc.XamlToolkit.Avaloina` or `Atc.XamlToolkit.Wpf` library offers a variety of base classes and utilities to streamline MVVM implementation:

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
