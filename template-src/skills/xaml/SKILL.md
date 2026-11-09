---
name: xaml
description: Разметка Avalonia XAML: структура View, x:Class и x:DataType, compiled bindings, Grid, стили, ресурсы. Используй при верстке и правке окон и контролов.
---

# XAML в Avalonia

## Структура

```text
Source/<Name>/View/
├── MainWindow.axaml
├── MainWindow.axaml.cs
├── Controls/       # кастомные контролы
├── Converters/     # конвертеры (см. avalonia-converters)
├── Dialogs/        # диалоговые окна
└── Styles/         # стили и ресурсы
```

- Неймспейс — `<Name>.View`.
- Имя файла совпадает с именем класса.
- Каждый экран — отдельная пара `.axaml` + `.axaml.cs`.

## Заголовок документа

```xml
<Window
    x:Class="Pnv.Desktop.View.MainWindow"
    x:DataType="vm:MainViewModel"
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    xmlns:vm="using:Pnv.Desktop.ViewModels"
    Title="Pnv.Desktop"
    Width="800"
    Height="450"
    WindowStartupLocation="CenterScreen">
```

- `x:Class` — первым атрибутом.
- `x:DataType` обязателен для compiled bindings.
- Пространства имён — через `using:`, а не `clr-namespace:`.
- Один атрибут на строку.

## Компоновка

`Grid` с явными определениями предпочтительнее вложенных `StackPanel`:

```xml
<Grid ColumnDefinitions="Auto,*,Auto" RowDefinitions="Auto,*" Margin="16">
    <TextBlock
        Grid.Row="0"
        Grid.Column="0"
        VerticalAlignment="Center"
        Text="{Binding Greeting}" />
    <Button
        Grid.Row="0"
        Grid.Column="2"
        Command="{Binding SayHelloCommand}"
        Content="Поздороваться" />
</Grid>
```

- `Grid.Column`/`Grid.Row` — первыми атрибутами элемента.
- Для отступов — `Margin`/`Padding`, а не пустые `Border`.
- `Spacing` используется в `StackPanel`, а не `Margin` у каждого ребёнка.

## Биндинги

- Только compiled bindings: проект включает
  `AvaloniaUseCompiledBindingsByDefault=true`.
- Биндинг идёт к свойству ViewModel, а не к контролу.
- Для команд — `Command`, для значений — `Text`/`Content`/`IsVisible`.

```xml
<!-- Хорошо -->
<TextBlock Text="{Binding Greeting}" />
<Button Command="{Binding SayHelloCommand}" Content="Поздороваться" />

<!-- Плохо: биндинг к элементу управления -->
<TextBlock Text="{Binding #other.Text}" />
```

## Стили

Повторяющиеся настройки — в стилях, а не в каждом элементе:

```xml
<Style Selector="Button.primary">
    <Setter Property="Padding" Value="16,8" />
    <Setter Property="Background" Value="{DynamicResource AccentBrush}" />
</Style>
```

```xml
<Button Classes="primary" Content="Сохранить" />
```

- Селектор — по классу, а не по `x:Name`.
- Цвета — только через `DynamicResource` (см. `avalonia-colors`).
- Стили контролов — в `View/Styles/*.axaml`.

## Иконки и изображения

- Иконки подключай ресурсом (`avares://`) или векторным `Path`.
- Не вставляй base64-картинки в XAML.
- Для кнопок-иконок задавай `Width`/`Height` и `Padding="0"`.
- Текстовые кнопки должны оставаться текстовыми: иконка не обязательна,
  если в проекте нет библиотеки иконок.

## Code-behind

В `.axaml.cs` допустимо только:

- `InitializeComponent()`;
- регистрация `Interaction`-обработчиков (ReactiveUI);
- визуальные действия: фокус, прокрутка, анимация;
- подписки, которые нельзя выразить биндингом.

```csharp
namespace Pnv.Desktop.View;

public sealed partial class MainWindow : Window
{
    public MainWindow() => InitializeComponent();
}
```

## Чего не делать

```xml
<!-- Плохо: нет x:DataType, биндинги не проверяются -->
<Window x:Class="Pnv.Desktop.View.MainWindow">
    <TextBlock Text="{Binding Greeting}" />
</Window>
```

```xml
<!-- Плохо: цвета и размеры захардкожены -->
<Button Background="#0078D4" Width="120" Content="ОК" />
```

```csharp
// Плохо: бизнес-логика в code-behind
private void Button_Click(object? sender, RoutedEventArgs e)
    => GreetingText.Text = new GreetingService().GetGreeting("мир").Text;
```

## Чек-лист

- [ ] `x:Class` и `x:DataType` указаны.
- [ ] Все биндинги compiled.
- [ ] Цвета и стили — в ресурсах.
- [ ] В code-behind нет бизнес-логики.
- [ ] Имя файла совпадает с классом.
