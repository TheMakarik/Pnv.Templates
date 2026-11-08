---
name: avalonia-colors
description: Организация цветов и кистей Avalonia: Colors.axaml, пары Color + SolidColorBrush, именование, DynamicResource, светлая и тёмная темы. Используй при добавлении или изменении цветов интерфейса.
---

# Цвета Avalonia

## Где хранить

Все цвета — в `View/Styles/Colors.axaml`. В контролах — только ссылки
через `DynamicResource`.

```text
View/Styles/Colors.axaml
```

## Пары Color + Brush

Для каждого цвета заводи и `Color`, и `SolidColorBrush`:

```xml
<ResourceDictionary
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <Color x:Key="AccentColor">#FF0078D4</Color>
    <SolidColorBrush x:Key="AccentBrush" Color="{StaticResource AccentColor}" />

    <Color x:Key="SurfaceColor">#FFFFFFFF</Color>
    <SolidColorBrush x:Key="SurfaceBrush" Color="{StaticResource SurfaceColor}" />

    <Color x:Key="TextPrimaryColor">#FF1A1A1A</Color>
    <SolidColorBrush x:Key="TextPrimaryBrush" Color="{StaticResource TextPrimaryColor}" />
</ResourceDictionary>
```

- `Color` нужен для градиентов, анимаций и `ColorAnimation`.
- `SolidColorBrush` — для `Background`, `Foreground`, `BorderBrush`.
- Кисть ссылается на цвет через `StaticResource`, а не дублирует hex.

## Именование

| Назначение | Суффикс | Пример |
|---|---|---|
| Цвет | `Color` | `AccentColor` |
| Кисть | `Brush` | `AccentBrush` |
| Поверхность | `Surface` | `SurfaceBrush` |
| Текст | `Text...` | `TextPrimaryBrush` |
| Граница | `Border...` | `BorderSubtleBrush` |
| Ошибка | `Error...` | `ErrorBrush` |

- Имя описывает роль, а не сам цвет: `ErrorBrush`, а не `RedBrush`.
- Один и тот же оттенок не заводят под разными именами.

## Светлая и тёмная темы

```xml
<ResourceDictionary
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <ResourceDictionary.ThemeDictionaries>
        <ResourceDictionary x:Key="Light">
            <SolidColorBrush x:Key="AppBackgroundBrush" Color="#FFFFFFFF" />
            <SolidColorBrush x:Key="TextPrimaryBrush" Color="#FF1A1A1A" />
        </ResourceDictionary>
        <ResourceDictionary x:Key="Dark">
            <SolidColorBrush x:Key="AppBackgroundBrush" Color="#FF1E1E1E" />
            <SolidColorBrush x:Key="TextPrimaryBrush" Color="#FFF5F5F5" />
        </ResourceDictionary>
    </ResourceDictionary.ThemeDictionaries>
</ResourceDictionary>
```

- Одинаковый набор ключей в обеих темах.
- Не оставляй ключ только в одной теме: в другой он будет пустым.

## Использование

```xml
<!-- Хорошо -->
<Border Background="{DynamicResource SurfaceBrush}">
    <TextBlock Foreground="{DynamicResource TextPrimaryBrush}" Text="Привет" />
</Border>

<!-- Плохо: цвет захардкожен -->
<Border Background="White">
    <TextBlock Foreground="#1A1A1A" Text="Привет" />
</Border>
```

- `DynamicResource` — для всего, что должно меняться вместе с темой.
- `StaticResource` — только внутри самого словаря (кисть ссылается на цвет).
- Не используй `DynamicResource` для `Color` в `SolidColorBrush`: там нужен
  `StaticResource` или литерал.

## Прозрачность и производные цвета

- Прозрачность задавай через alpha-канал: `#80FF0000`.
- Для затемнения/осветления заводи отдельную кисть, а не считай в конвертере.
- Не накладывай полупрозрачные слои «на глаз» без необходимости.

## Чего не делать

```xml
<!-- Плохо: палитра размазана по окнам -->
<Window>
    <Button Background="#0078D4" />
    <TextBlock Foreground="#333333" />
</Window>
```

```xml
<!-- Плохо: имя не отражает роль -->
<SolidColorBrush x:Key="Blue2Brush" Color="#FF0078D4" />
```

## Чек-лист

- [ ] Все цвета собраны в `Colors.axaml`.
- [ ] Для каждого цвета есть `Color` и `SolidColorBrush`.
- [ ] Имена отражают роль, а не оттенок.
- [ ] Светлая и тёмная темы содержат одинаковые ключи.
- [ ] В контролах нет литеральных цветов.
