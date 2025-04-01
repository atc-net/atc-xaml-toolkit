# Atc.XamlToolkit - Schemas for ValueConverters

This directory documents the collection of reusable and cross-platform-friendly `ValueConverters` available in **Atc.XamlToolkit**.

These converters are organized by their **input/output types**, making it easy to locate the correct one based on the data transformation needed in your XAML bindings.

---

## 📜 XML Namespace Mapping

For WPF:

```xml
xmlns:atcValueConverters="https://github.com/atc-net/atc-xaml-toolkit/tree/main/schemas/value-converters"
```

For Avalonia:

```xml
xmlns:atcValueConverters="clr-namespace:Atc.XamlToolkit.ValueConverters;assembly=Atc.XamlToolkit.Avalonia"
```

---

## 🧹 Usage

To use a converter in WPF or Avalonia Window control:

```xml
<Window.Resources>
    <atcValueConverters:BoolToVisibilityVisibleValueConverter x:Key="BoolToVisibilityVisibleValueConverter" />
</Window.Resources>

<StackPanel Visibility="{Binding IsVisible, Converter={StaticResource BoolToVisibilityVisibleValueConverter}}" />
```

---

## #️⃣ ValueConverters - Bool to X

| Category            | Type                                         | WPF From → To              | WPF Convert Example               | Avalonia From → To       | Avalonia Convert Example    |
|---------------------|----------------------------------------------|----------------------------|-----------------------------------|--------------------------|-----------------------------|
| Bool → Bool         | `BoolToInverseBoolValueConverter`            | bool → bool                | True → False, False → True        | bool → bool              | True → False, False → True  |
| Bool → Visibility   | `BoolToVisibilityCollapsedValueConverter`    | bool → Visibility          | True → Collapsed, False → Visible | ❌                      | ❌                          |
| Bool → Visibility   | `BoolToVisibilityVisibleValueConverter`      | bool → Visibility          | True → Visible, False → Collapsed | bool → bool (IsVisible) | true → true, False → false  |
| Bool → Width        | `BoolToWidthValueConverter`                  | bool + param → double/Auto | true, 10 → 10, true, Auto → Auto  | ❌                      | ❌                         |
| Bool[] → Bool       | `MultiBoolToBoolValueConverter`              | bool[] → bool              | All True → True                   | ❌                      | ❌                         |
| Bool[] → Visibility | `MultiBoolToVisibilityVisibleValueConverter` | bool[] → Visibility        | All True → Visible                | ❌                      | ❌                         |

---