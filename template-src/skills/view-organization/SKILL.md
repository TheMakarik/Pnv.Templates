---
name: view-organization
description: Как организовывать View в Avalonia: папка View, x:Class, x:DataType, code-behind, стили и ресурсы. Используй при создании и правке окон и контролов.
---

# View

## Где лежат

- Все XAML-экраны — в `Source/<Name>/View`.
- Неймспейс — `<Name>.View`.
- Имя файла совпадает с именем класса: `MainWindow.axaml` + `MainWindow.axaml.cs`.

```text
View/
├── MainWindow.axaml
├── MainWindow.axaml.cs
├── Controls/       # кастомные контролы
├── Converters/     # конвертеры
├── Dialogs/        # диалоговые окна
└── Styles/         # стили и ресурсы
```

## XAML

- Всегда указывай `x:Class` и `x:DataType`.
- Включай compiled bindings (`AvaloniaUseCompiledBindingsByDefault=true`).
- Не используй `{Binding}` без типа — это ломает compile-time проверку.
- Атрибуты — каждый на новой строке, `x:Class` первым.

```xml
<Window
    x:Class="Pnv.Desktop.View.MainWindow"
    x:DataType="vm:MainViewModel"
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="using:Pnv.Desktop.ViewModels"
    Title="Pnv.Desktop"
    Width="800"
    Height="450">
    <TextBlock Text="{Binding Greeting}" />
</Window>
```

## Code-behind

В code-behind допустимо только:

- `InitializeComponent()`;
- регистрация обработчиков `Interaction` (ReactiveUI);
- чисто визуальные действия: фокус, прокрутка, анимация;
- подписки, которые нельзя выразить биндингом.

Запрещено: бизнес-логика, запросы к БД, вычисления, создание сервисов.

```csharp
namespace Pnv.Desktop.View;

public sealed partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();
}
```

## DataContext

- DataContext задаётся извне: в `App.axaml.cs` или в родительском View.
- View не создаёт ViewModel через `new`, если используется DI.
- Для превью в дизайнере можно указать `Design.DataContext`,
  но только если у ViewModel есть безпараметрический конструктор.

## Стили и ресурсы

- Цвета — в `View/Styles/Colors.axaml` (см. `avalonia-colors`).
- Стили контролов — в `View/Styles/*.axaml`, не в каждом окне.
- Тема — `FluentTheme` + `RequestedThemeVariant` (см. `avalonia-theming`).
- Не хардкодь цвета и размеры в контролах — используй `DynamicResource`.

## Чего не делать

```xml
<!-- Плохо: нет x:DataType, биндинг не проверяется компилятором -->
<Window x:Class="Pnv.Desktop.View.MainWindow">
    <TextBlock Text="{Binding Greeting}" />
</Window>
```

```csharp
// Плохо: бизнес-логика в code-behind
private void Button_Click(object? sender, RoutedEventArgs e)
{
    var greeting = new GreetingService().GetGreeting("мир");
    GreetingText.Text = greeting.Text;
}
```
