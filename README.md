# ATC.Net Avalonia and WPF

This is a base libraries for building Avalonia or WPF application with the MVVM design pattern.

## Requirements

[.NET >= 9.0.202 - SDK](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

[.NET 9 - Runtime for Avalonia](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

[.NET 9 - Desktop Runtime for WPF](https://dotnet.microsoft.com/en-us/download/dotnet/9.0)

## NuGet Packages Provided in this Repository

| Nuget package               | Description                                                                                                                                                              | Dependencies                           |
|-----------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------|
| [![NuGet Version](https://img.shields.io/nuget/v/Atc.XamlToolkit.svg?label=Atc.XamlToolkit&logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/Atc.XamlToolkit)                            | Base package with ViewModelBase, ObservableObject   | Atc & Atc.XamlToolkit.SourceGenerators |
| [![NuGet Version](https://img.shields.io/nuget/v/Atc.XamlToolkit.Avalonia.svg?label=Atc.XamlToolkit.Avalonia&logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/Atc.XamlToolkit.Avalonia) | RelayCommand, MainWindowViewModelBase for Avalonia  | Atc.XamlToolkit                        |
| [![NuGet Version](https://img.shields.io/nuget/v/Atc.XamlToolkit.Wpf.svg?label=Atc.XamlToolkit.Wpf&logo=nuget&style=for-the-badge)](https://www.nuget.org/packages/Atc.XamlToolkit.Wpf)                | RelayCommand, MainWindowViewModelBase for WPF       | Atc.XamlToolkit.SourceGenerators       |

## MVVM Easily Separate UI and Business Logic

With the `Atc.XamlToolkit.Avalonia` or `Atc.Wpf` package, it is very easy to get startet with the nice `MVVM pattern`

Please read more here:

- [MVVM framework](docs/Mvvm/@Readme.md)

# ⚙️ Source Generators

Our source generators streamline development by automatically generating boilerplate code for 
both `Avalonia` and `WPF` projects.

Using custom attributes, they simplify the creation of common 
patterns such as attached properties, dependency properties, and viewmodels — reducing manual 
coding and potential errors.

Learn more about each generator:

- [SourceGenerators for AttachedProperties](docs/SourceGenerators/AttachedProperty.md)
- [SourceGenerators for DependencyProperties](docs/SourceGenerators/DependencyProperty.md)
- [SourceGenerators for ViewModel](docs/SourceGenerators/ViewModel.md)

Example for ViewModel classes

![MVVM Source Generation](docs/images/mvvm-source-generated.png)

For more details, see the [MVVM](docs/Mvvm/@Readme.md) section.

# How to contribute

[Contribution Guidelines](https://atc-net.github.io/introduction/about-atc#how-to-contribute)

[Coding Guidelines](https://atc-net.github.io/introduction/about-atc#coding-guidelines)
