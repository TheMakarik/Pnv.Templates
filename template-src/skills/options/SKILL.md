---
name: options
description: Настройка IOptions<T> в C#: биндинг секции appsettings.json через AddOptions<T>().Bind, валидация, значения по умолчанию, где регистрировать. Используй при добавлении или правке классов Options.
---

# IOptions

## Где лежат опции

- Один проект: `Source/<Name>/Options`.
- Split: `Source/<Name>.Core/Options`.
- Неймспейс — `<Name>.Options`.

Класс опций — `sealed`, с инициализаторами и без логики:

```csharp
namespace PnvApp.Options;

public sealed class AppOptions
{
    public string Title { get; set; } = "PnvApp";
    public string DataPath { get; set; } = string.Empty;
}
```

## Регистрация

Опции регистрируются только в `Bootstrapper`, до построения хоста:

```csharp
private static void RegisterOptions(HostApplicationBuilder builder)
{
    builder.Services
        .AddOptions<AppOptions>()
        .Bind(builder.Configuration.GetSection(nameof(AppOptions)))
        .ValidateDataAnnotations()
        .ValidateOnStart();
}
```

- Секция в `appsettings.json` называется так же, как класс: `AppOptions`.
- `AddOptions<T>()` регистрирует `IOptions<T>`, `IOptionsSnapshot<T>` и `IOptionsMonitor<T>`.
- Отдельно регистрировать `IOptions<>` не нужно.

## Потребление

```csharp
public sealed class GreetingService(IOptions<AppOptions> options)
{
    private readonly AppOptions _options = options.Value;

    public string GetTitle() => _options.Title;
}
```

Правила:

- `IOptions<T>` — для настроек, которые не меняются во время работы.
- `IOptionsMonitor<T>` — если нужна реакция на изменение файла.
- Не инжектировать сам класс `AppOptions` — только `IOptions<AppOptions>`.
- Не читать `IConfiguration` в бизнес-коде: всё уже связано в опции.

## Валидация

```csharp
using System.ComponentModel.DataAnnotations;

public sealed class AppOptions
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Range(1, 4096)]
    public int Width { get; set; } = 800;
}
```

Для сложных правил — `.Validate(...)`:

```csharp
builder.Services
    .AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection(nameof(AppOptions)))
    .Validate(options => !string.IsNullOrWhiteSpace(options.Title), "Title обязателен")
    .ValidateOnStart();
```

`ValidateOnStart()` важен: приложение падает на старте, а не при первом обращении
к опциям.

## Значения по умолчанию

Всегда задавай разумные значения в самом классе. Отсутствующая секция в
`appsettings.json` не должна приводить к `null` и падению.

```csharp
// Хорошо
public string Title { get; set; } = "PnvApp";

// Плохо — null при отсутствии ключа
public string Title { get; set; } = null!;
```

## Чего не делать

```csharp
// Плохо: сервис сам лезет в конфигурацию
public sealed class GreetingService(IConfiguration configuration)
{
    public string GetTitle() => configuration["AppOptions:Title"] ?? "PnvApp";
}
```

```csharp
// Плохо: опции как сервис-локатор
public sealed class GreetingService(IServiceProvider provider)
{
    public string GetTitle()
        => provider.GetRequiredService<IOptions<AppOptions>>().Value.Title;
}
```
