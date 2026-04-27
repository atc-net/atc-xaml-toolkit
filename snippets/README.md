# Atc.XamlToolkit IDE snippets

Code snippets for the toolkit's source-generator attributes — `[ObservableProperty]`, `[ComputedProperty]`, `[RelayCommand]`, `[DependencyProperty]`, `[AttachedProperty]`, `[StyledProperty]`, `[INotifyPropertyChanged]`, `[ObservableDtoViewModel]` — so the boilerplate types itself.

## Snippet shortcuts

| Shortcut          | Expands to                                            |
|-------------------|-------------------------------------------------------|
| `obsprop`         | `[ObservableProperty]` field                          |
| `obspropcb`       | `[ObservableProperty]` with before/after callbacks    |
| `obspropcmd`      | `[ObservableProperty]` + `[NotifyCanExecuteChangedFor]` |
| `compprop`        | `[ComputedProperty]` expression-bodied                |
| `relaycmd`        | Synchronous `[RelayCommand]` method                   |
| `relaycmdasync`   | Async `[RelayCommand]` method                         |
| `relaycmdcancel`  | Async `[RelayCommand]` with cancellation + IsBusy     |
| `relaycmdcanexec` | `[RelayCommand]` + matching `Can…` predicate          |
| `depprop`         | Field-level `[DependencyProperty]` (WPF / WinUI)      |
| `deppropclass`    | Class-level `[DependencyProperty<T>]`                 |
| `attprop`         | Field-level `[AttachedProperty]`                      |
| `attpropclass`    | Class-level `[AttachedProperty<T>]`                   |
| `styledprop`      | Avalonia `[StyledProperty]` field                     |
| `routedevt`       | WPF `[RoutedEvent]` (WPF only)                        |
| `inotpc`          | `[INotifyPropertyChanged]` partial class              |
| `dtoview`         | `[ObservableDtoViewModel]` partial class              |

The VS Code file (`vscode/atc-xaml-toolkit.code-snippets`) is the canonical, complete set. The Visual Studio XML files cover the most commonly used subset.

## Visual Studio Code

Drop `vscode/atc-xaml-toolkit.code-snippets` into one of these locations:

- **Per-project**: `<repo>/.vscode/atc-xaml-toolkit.code-snippets` — checked-in, applies to anyone who clones the repo.
- **Per-user**: `%APPDATA%\Code\User\snippets\atc-xaml-toolkit.code-snippets` (Windows) or `~/.config/Code/User/snippets/` (Linux/macOS) — applies to all your VS Code projects.

Reload the window (`Ctrl+Shift+P` → "Developer: Reload Window"). Type a shortcut from the table above in any C# file and press `Tab` to expand.

These snippets also work in **Cursor**, **VS Code Insiders**, and any **VS Code-compatible** IDE that reads `.code-snippets` files.

## Visual Studio (Windows)

Open *Tools → Code Snippets Manager…* (`Ctrl+K, Ctrl+B`), choose `Language: Visual C#`, click *Import…*, and select the `.snippet` files in `visual-studio/`. They'll land under *My Code Snippets*.

To deploy them for your whole team, drop the files into:

```
%USERPROFILE%\Documents\Visual Studio 2022\Code Snippets\Visual C#\My Code Snippets\
```

Type the shortcut in a C# file and press `Tab Tab` to expand.

## JetBrains Rider

Rider doesn't read VS Code's `.code-snippets` format directly, but you can re-create the same set as **Live Templates** under *File → Settings → Editor → Live Templates → C#*. Use the table above as the abbreviation list and the corresponding snippet body from `vscode/atc-xaml-toolkit.code-snippets` as the template text.

A pre-built Rider settings layer (`AtcXamlToolkit.DotSettings`) is not currently shipped — file an issue if you'd like one.