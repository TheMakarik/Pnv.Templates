---
name: options
description: IOptions-паттерн: классы опций в Options/ (AppOptions с Title), регистрация AddOptions<T>().Bind(...), использование IOptions<T> в параметрах handler'ов endpoint'ов. Используй при добавлении настроек приложения.
---

# Опции (IOptions)

## Классы опций

Классы опций лежат в `Options/`. Базовый пример — `AppOptions` со свойством `Title`:

```csharp
namespace Pnv.Web.Options;

public sealed class AppOptions
{
    public string Title { get; init; } = string.Empty;
}
```

## Регистрация

В `Program.cs`:

```csharp
builder.Services.AddOptions<AppOptions>()
    .Bind(builder.Configuration.GetSection(nameof(AppOptions)));
```

Имя секции в `appsettings.json` совпадает с именем класса:

```json
"AppOptions": {
  "Title": "Pnv.Web"
}
```

## Использование

Через `IOptions<T>` в параметрах handler'ов endpoint'ов:

```csharp
group.MapGet("/", (IOptions<AppOptions> options) =>
    TypedResults.Ok(new { Title = options.Value.Title }));
```

## Как добавить новую опцию

1. Новый класс в `Options/` (например, `MailOptions`).
2. Секция в `appsettings.json` с тем же именем, что и класс.
3. Регистрация `AddOptions<T>().Bind(...)` в `Program.cs`.
4. Секретные значения опций — в user secrets (см. скилл `secrets`).

## Чек-лист

- [ ] Класс опций — в `Options/`, `sealed`, свойства — `init`.
- [ ] Имя секции конфигурации = имени класса.
- [ ] Чтение — через `IOptions<T>`, без обращений к конфигурации в логике.
