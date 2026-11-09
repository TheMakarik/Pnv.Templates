---
name: avalonia-converters
description: Конвертеры значений Avalonia (IValueConverter, IMultiValueConverter) как MarkupExtension. Используй при добавлении конвертеров в View/Converters, для bool/enum-преобразований и MultiBinding.
---

# Конвертеры Avalonia

## Где лежат

`Source/<Name>/View/Converters`, неймспейс `<Name>.View.Converters`.
Имя файла и класса заканчивается на `Converter`.

## Базовая структура

Конвертер — `sealed`, наследует `MarkupExtension` и реализует `IValueConverter`.
Так его можно использовать в XAML без объявления в ресурсах.

```csharp
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace Pnv.Desktop.View.Converters;

public sealed class BooleanToOpacityConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value is true ? 1d : 0.5d;

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
```

```xml
<TextBlock Opacity="{Binding IsEnabled, Converter={converters:BooleanToOpacityConverter}}" />
```

## Convert

- Проверяй тип входа: `value is not bool flag` → вернуть
  `AvaloniaProperty.UnsetValue` или значение по умолчанию.
- Не бросай исключения на «неожиданный» тип: UI не должен падать из-за биндинга.
- `parameter` — строка из XAML; приводи её явно.
- Не выполняй в конвертере запросы к БД, файлам и сервисам.

```csharp
public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
{
    if (value is not bool isVisible)
        return AvaloniaProperty.UnsetValue;

    return isVisible ? Visibility.Visible : Visibility.Collapsed;
}
```

## ConvertBack

Если обратное преобразование не поддерживается — `NotSupportedException`.
Не возвращай `null` молча: ошибка станет неочевидной.

```csharp
public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    => throw new NotSupportedException();
```

Если поддерживается — реализуй симметрично и покрой тестом.

## Enum

```csharp
public sealed class StatusToStringConverter : MarkupExtension, IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => value switch
        {
            Status.Idle => "Ожидание",
            Status.Running => "Выполняется",
            Status.Failed => "Ошибка",
            _ => "Неизвестно",
        };

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotSupportedException();

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
```

- Всегда добавляй ветку `_`.
- Тексты — по-русски, если интерфейс русский.
- Не смешивай форматирование и бизнес-правила.

## MultiBinding

```csharp
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace Pnv.Desktop.View.Converters;

public sealed class AllTrueConverter : MarkupExtension, IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        => values.Count > 0 && values.All(value => value is true);

    public override object ProvideValue(IServiceProvider serviceProvider) => this;
}
```

```xml
<Button IsEnabled="{MultiBinding Converter={converters:AllTrueConverter}}">
    <MultiBinding.Bindings>
        <Binding Path="IsLoaded" />
        <Binding Path="HasChanges" />
    </MultiBinding.Bindings>
</Button>
```

## Чего не делать

```csharp
// Плохо: конвертер без MarkupExtension — нужен экземпляр в ресурсах
public sealed class MyConverter : IValueConverter { }
```

```csharp
// Плохо: конвертер ходит в сервис
public sealed class GreetingConverter(IGreetingService service) : IValueConverter { }
```

```csharp
// Плохо: имя не отражает поведение
public sealed class Converter1 : MarkupExtension, IValueConverter { }
```

## Чек-лист

- [ ] Конвертер `sealed` и наследует `MarkupExtension`.
- [ ] `ProvideValue` возвращает `this`.
- [ ] Некорректный тип входа не приводит к падению.
- [ ] `ConvertBack` реализован или бросает `NotSupportedException`.
- [ ] Конвертер лежит в `View/Converters` и назван по смыслу.
