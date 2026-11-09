---
name: reactiveui-interactions
description: Связь ViewModel → View в ReactiveUI через Interaction<TInput,TOutput>, RegisterHandler только в code-behind. Используй, когда ViewModel должен показать диалог или запросить действие у View.
---

# ReactiveUI: Interactions

## Правило

ViewModel не знает о конкретных View. Любой запрос к UI идёт через
`Interaction<TInput, TOutput>`.

```text
ViewModel ──Handle(input)──► Interaction ──► handler в View ──► результат
```

## Объявление

```csharp
using System.Reactive;
using ReactiveUI;

namespace Pnv.Desktop.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
    public Interaction<string, Unit> ShowMessage { get; } = new();
}
```

- `Interaction<TInput, TOutput>` — публичное свойство только для чтения.
- `Unit` — когда результат не нужен.
- `bool` — подтверждение, `string?` — ввод, свой тип — результат диалога.

## Использование в ViewModel

```csharp
[ReactiveCommand]
private async Task SayHelloAsync()
{
    Greeting = _greetingService.GetGreeting("мир").Text;
    await ShowMessage.Handle(Greeting);
}
```

- `Handle` асинхронный: возвращает `Task<TOutput>`.
- Если handler не зарегистрирован, `Handle` зависает. Регистрируй его
  при создании View.
- Не вызывай `Handle` из конструктора ViewModel.

## Регистрация в View

Только в code-behind:

```csharp
using System.Reactive;
using Avalonia.Controls;
using Pnv.Desktop.ViewModels;

namespace Pnv.Desktop.View;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (DataContext is not MainViewModel viewModel)
            return;

        viewModel.ShowMessage.RegisterHandler(context =>
        {
            // здесь допустим UI-код: показать диалог, обновить контрол
            context.SetOutput(Unit.Default);
        });
    }
}
```

- `RegisterHandler` — единственное место, где ViewModel «касается» View.
- В handler допустимы диалоги, фокус, прокрутка, анимации.
- Не регистрируй handler из ViewModel и не храни View в ViewModel.

## Диалог с результатом

```csharp
// ViewModel
public Interaction<string, bool> Confirm { get; } = new();

[ReactiveCommand]
private async Task DeleteAsync()
{
    var confirmed = await Confirm.Handle("Удалить запись?");
    if (!confirmed)
        return;

    await _storage.DeleteAsync();
}
```

```csharp
// View
viewModel.Confirm.RegisterHandler(async context =>
{
    var dialog = new ConfirmWindow { DataContext = context.Input };
    var result = await dialog.ShowDialog<bool>(this);
    context.SetOutput(result);
});
```

## Когда Interaction не нужен

- Изменение состояния экрана → биндинг.
- Системное действие без диалога (буфер обмена, открыть URL) → UI-сервис
  с интерфейсом в DI.
- Навигация между экранами → сервис навигации.

## Запрещено

```csharp
// Плохо: ViewModel создаёт окно
public sealed class MainViewModel
{
    public void Show() => new MainWindow().Show();
}
```

```csharp
// Плохо: ViewModel знает про Window
public sealed class MainViewModel(Window window) { }
```

```csharp
// Плохо: handler зарегистрирован в ViewModel
ShowMessage.RegisterHandler(context => context.SetOutput(Unit.Default));
```

## Чек-лист

- [ ] ViewModel не ссылается на Avalonia-типы View.
- [ ] `Interaction` объявлен как свойство только для чтения.
- [ ] `RegisterHandler` только в code-behind.
- [ ] Для каждого `Handle` есть зарегистрированный handler.
- [ ] Результат диалога возвращается через `SetOutput`.
