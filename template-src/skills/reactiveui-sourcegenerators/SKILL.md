---
name: reactiveui-sourcegenerators
description: Использование ReactiveUI.SourceGenerators: [Reactive] на приватных полях, [ReactiveCommand], partial-классы, CanExecute. Используй при написании ViewModel на ReactiveUI.
---

# ReactiveUI Source Generators

## Подключение

```xml
<PackageReference Include="ReactiveUI.Avalonia" Version="12.0.3" />
<PackageReference Include="ReactiveUI.SourceGenerators" Version="3.2.0" />
```

Генераторы работают на этапе компиляции: ручные `ReactiveCommand.Create`
и `RaiseAndSetIfChanged` не нужны.

## Свойства

```csharp
using ReactiveUI.SourceGenerators;

namespace Pnv.Desktop.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    [Reactive]
    private string _greeting = "Привет!";
}
```

- Класс обязан быть `partial`.
- Атрибут ставится на приватное поле `_camelCase`.
- Генератор создаёт публичное свойство `Greeting`.
- Инициализатор поля задаёт значение по умолчанию.

Эквивалент без генератора (не писать вручную):

```csharp
private string _greeting = "Привет!";
public string Greeting
{
    get => _greeting;
    set => this.RaiseAndSetIfChanged(ref _greeting, value);
}
```

## Команды

```csharp
using ReactiveUI.SourceGenerators;

public sealed partial class MainViewModel : ViewModelBase
{
    [ReactiveCommand]
    private void SayHello() => Greeting = "Привет, мир!";

    [ReactiveCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
    }
}
```

Генератор создаёт:

- `SayHelloCommand` — `ReactiveCommand<Unit, Unit>`;
- `LoadAsyncCommand` — `ReactiveCommand<Unit, Unit>` с асинхронным телом;
- `CanSayHello` и обновление доступности при `CanExecute = nameof(CanSayHello)`.

## CanExecute

```csharp
[Reactive]
private string _name = string.Empty;

private bool CanSayHello() => !string.IsNullOrWhiteSpace(_name);

[ReactiveCommand(CanExecute = nameof(CanSayHello))]
private void SayHello() => Greeting = $"Привет, {_name}!";
```

- Метод `CanExecute` — приватный, без параметров, возвращает `bool`.
- Не вызывай `NotifyCanExecuteChanged` вручную: генератор подписывается
  на изменения `[Reactive]`-свойств, использованных в `CanExecute`.

## Именование

| Элемент | Имя |
|---|---|
| Поле | `_greeting` |
| Свойство | `Greeting` |
| Асинхронная команда | `LoadAsyncCommand` |
| Синхронная команда | `SayHelloCommand` |

Не создавай свойство и команду с одинаковым именем.

## Связь с View

```xml
<TextBlock Text="{Binding Greeting}" />
<Button Content="Поздороваться" Command="{Binding SayHelloCommand}" />
```

## Чего не делать

```csharp
// Плохо: ручное свойство при подключённых генераторах
private string _greeting = string.Empty;
public string Greeting
{
    get => _greeting;
    set => this.RaiseAndSetIfChanged(ref _greeting, value);
}
```

```csharp
// Плохо: ручная команда
public ReactiveCommand<Unit, Unit> SayHello { get; }

public MainViewModel()
{
    SayHello = ReactiveCommand.Create(() => Greeting = "Привет");
}
```

```csharp
// Плохо: атрибут на публичном поле
[Reactive]
public string Greeting = string.Empty;
```

## Чек-лист

- [ ] Класс `partial`.
- [ ] `[Reactive]` только на приватных полях `_camelCase`.
- [ ] `[ReactiveCommand]` на приватных методах.
- [ ] Нет ручных `ReactiveCommand.Create` и `RaiseAndSetIfChanged`.
- [ ] `CanExecute` синхронизирован с изменениями свойств.
