---
name: avalonia-theming
description: Темы Avalonia: FluentTheme, RequestedThemeVariant, ResourceDictionary, DynamicResource и хранение стилей. Используй при настройке внешнего вида приложения и добавлении тем.
---

# Темы Avalonia

## Базовая настройка

`App.axaml` подключает Fluent-тему и вариант по умолчанию:

```xml
<Application
    x:Class="Pnv.Desktop.App"
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
    RequestedThemeVariant="Default">
    <Application.Styles>
        <FluentTheme />
    </Application.Styles>
</Application>
```

- `RequestedThemeVariant="Default"` — следовать системной теме.
- `Light` / `Dark` — принудительный вариант.
- Не подключай одновременно `FluentTheme` и стороннюю тему.

## Ресурсы и словари

Ресурсы приложения — в отдельных `ResourceDictionary`:

```text
View/
└── Styles/
    ├── Colors.axaml
    ├── Controls.axaml
    └── Theme.axaml
```

Подключение:

```xml
<Application.Resources>
    <ResourceDictionary>
        <ResourceDictionary.MergedDictionaries>
            <ResourceInclude Source="avares://Pnv.Desktop/View/Styles/Colors.axaml" />
            <ResourceInclude Source="avares://Pnv.Desktop/View/Styles/Controls.axaml" />
        </ResourceDictionary.MergedDictionaries>
    </ResourceDictionary>
</Application.Resources>
```

- `Colors.axaml` — цвета и кисти (см. скилл `avalonia-colors`).
- `Controls.axaml` — стили контролов.
- `Theme.axaml` — общие размеры, отступы, типографика.

## ThemeVariant

В словаре можно задать разные значения для светлой и тёмной темы:

```xml
<ResourceDictionary
    xmlns="https://github.com/avaloniaui"
    xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml">
    <ResourceDictionary.ThemeDictionaries>
        <ResourceDictionary x:Key="Light">
            <SolidColorBrush x:Key="AppBackgroundBrush" Color="#FFFFFF" />
        </ResourceDictionary>
        <ResourceDictionary x:Key="Dark">
            <SolidColorBrush x:Key="AppBackgroundBrush" Color="#1E1E1E" />
        </ResourceDictionary>
    </ResourceDictionary.ThemeDictionaries>
</ResourceDictionary>
```

Использование — через `DynamicResource`, чтобы тема менялась без перезапуска:

```xml
<Border Background="{DynamicResource AppBackgroundBrush}" />
```

## Стили

Стили — селекторы, а не `Style` внутри каждого контрола:

```xml
<Styles xmlns="https://github.com/avaloniaui">
    <Style Selector="Button.primary">
        <Setter Property="Background" Value="{DynamicResource AccentBrush}" />
        <Setter Property="Foreground" Value="{DynamicResource AccentForegroundBrush}" />
        <Setter Property="Padding" Value="16,8" />
    </Style>
</Styles>
```

- Один файл стилей на группу контролов.
- Не дублируй одинаковые сеттеры в окнах.
- Не хардкодь цвета — только ресурсы.

## Переключение темы

Простой и поддерживаемый способ — свойство приложения:

```csharp
public void SetTheme(ThemeVariant variant)
    => Application.Current!.RequestedThemeVariant = variant;
```

- `ThemeVariant.Light`, `ThemeVariant.Dark`, `ThemeVariant.Default`.
- Не перезагружай `App.axaml` ради смены темы.
- Не храни тему в статическом поле View — используй сервис или опции.

## Чего не делать

```xml
<!-- Плохо: цвет захардкожен и не меняется вместе с темой -->
<Button Background="#FF0078D4" />
```

```csharp
// Плохо: ручная замена ресурсов и перезагрузка словарей
Application.Current.Resources.MergedDictionaries.Clear();
```

## Чек-лист

- [ ] `FluentTheme` подключён один раз.
- [ ] `RequestedThemeVariant` задан осознанно.
- [ ] Цвета и стили — в отдельных словарях.
- [ ] В контролах используется `DynamicResource`.
- [ ] Смена темы не требует перезапуска приложения.
