---
name: mvvm-communitytoolkit
description: MVVM на CommunityToolkit.Mvvm: ObservableObject, [ObservableProperty] на partial-свойствах, [RelayCommand], сгенерированные команды. Используй, когда в шаблоне выбран CommunityToolkit.
---

# MVVM на CommunityToolkit

## База

Все ViewModel наследуют `ViewModelBase`:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace PnvApp.ViewModels;

public abstract class ViewModelBase : ObservableObject;
```

## Свойства

Используй `[ObservableProperty]` на `partial`-свойствах (C# 14).
Не пиши `SetProperty` вручную.

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace PnvApp.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Привет!";

    [ObservableProperty]
    public partial bool IsBusy { get; set; }
}
```

Правила:

- Класс обязан быть `partial`.
- Свойство обязано быть `partial` с `{ get; set; }`.
- Инициализатор допустим и предпочтителен для значений по умолчанию.
- Имя свойства — `PascalCase`; сгенерированное поле — `_camelCase`.
- Не создавай рядом публичное поле с тем же именем.

## Команды

```csharp
using CommunityToolkit.Mvvm.Input;

public sealed partial class MainViewModel : ViewModelBase
{
    [RelayCommand]
    private void SayHello() => Greeting = "Привет, мир!";

    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
    }
}
```

Генератор создаёт:

- `SayHelloCommand` типа `IRelayCommand`;
- `LoadAsyncCommand` типа `IAsyncRelayCommand`;
- `CanSayHello` и `SayHelloCommand.NotifyCanExecuteChanged()` при
  `[RelayCommand(CanExecute = nameof(CanSayHello))]`.

Правила:

- Имя метода — глагол в `PascalCase`, без суффикса `Command`.
- `async`-команда возвращает `Task` и принимает `CancellationToken`.
- Не создавай `RelayCommand` вручную, если хватает атрибута.

## CanExecute

```csharp
[ObservableProperty]
[NotifyCanExecuteChangedFor(nameof(SayHelloCommand))]
public partial string Name { get; set; } = string.Empty;

private bool CanSayHello() => !string.IsNullOrWhiteSpace(Name);

[RelayCommand(CanExecute = nameof(CanSayHello))]
private void SayHello() => Greeting = $"Привет, {Name}!";
```

## Связь с View

- `View` биндится по именам свойств и команд: `{Binding Greeting}`,
  `{Binding SayHelloCommand}`.
- `x:DataType` обязателен, иначе compiled bindings не работают.
- ViewModel не знает о `Window`, `Control`, `Application`.

```xml
<TextBlock Text="{Binding Greeting}" />
<Button Content="Поздороваться" Command="{Binding SayHelloCommand}" />
```

## Зависимости

Только конструкторная инъекция:

```csharp
public sealed partial class MainViewModel(IGreetingService greetingService) : ViewModelBase
{
    [RelayCommand]
    private void SayHello() => Greeting = greetingService.GetGreeting("мир").Text;
}
```

## Чего не делать

```csharp
// Плохо: ручное свойство с SetProperty
private string _greeting = string.Empty;
public string Greeting
{
    get => _greeting;
    set => SetProperty(ref _greeting, value);
}
```

```csharp
// Плохо: ViewModel создаёт окно
[RelayCommand]
private void Show() => new MainWindow().Show();
```

```csharp
// Плохо: бизнес-логика в команде
[RelayCommand]
private void Save() => File.WriteAllText("data.txt", Greeting);
```

## Чек-лист

- [ ] Класс `partial`, свойства `partial`.
- [ ] Нет ручных `SetProperty` и `RelayCommand`.
- [ ] `async`-команды принимают `CancellationToken`.
- [ ] ViewModel не зависит от Avalonia.
- [ ] `x:DataType` указывает на конкретный тип ViewModel.
