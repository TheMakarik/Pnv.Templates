---
name: logging
description: Логирование через Serilog в ASP.NET Core: инициализация в Program.cs из appsettings.json, ILogger<T> в коде, структурированные шаблоны, SQL EF Core через SqlLoggingInterceptor, запрет логирования секретов. Используй при добавлении логов.
---

# Логирование (Serilog)

## Инициализация

Serilog настраивается один раз в `Program.cs` из конфигурации:

```csharp
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders().AddSerilog(Log.Logger, dispose: true);
```

## Конфигурация

Секция `"Serilog"` в `appsettings.json`: `MinimumLevel`, `Override`,
`WriteTo` → Console с `outputTemplate`:

```json
"Serilog": {
  "MinimumLevel": {
    "Default": "Information",
    "Override": { "Microsoft.AspNetCore": "Warning" }
  },
  "WriteTo": [
    {
      "Name": "Console",
      "Args": {
        "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
      }
    }
  ]
}
```

## Использование

В коде — только `ILogger<T>` через DI. Статический `Log.Logger` в классах не использовать.

```csharp
public sealed class GreetingService(ILogger<GreetingService> logger)
{
    public Greeting Create(string text)
    {
        logger.LogInformation("Создано приветствие {Text}", text);
        // ...
    }
}
```

## Структурированные логи

Данные передавай параметрами шаблона, а не склейкой строк.

```csharp
// Хорошо — значение станет полем события
logger.LogInformation("Создано приветствие {Text}", text);

// Плохо — данные теряются в строке
logger.LogInformation($"Создано приветствие {text}");
```

## SQL EF Core

SQL-запросы логируются через `Interceptors/SqlLoggingInterceptor` в
`Infrastructure` — перехватчик уже подключён к DbContext. Свои выводы SQL добавлять не надо.

## Запреты

- Не логировать секреты, ключи, пароли и персональные данные.
- Не использовать `Console.WriteLine`/`Debug.WriteLine` для логов приложения.

## Чек-лист

- [ ] Serilog инициализирован в `Program.cs` из секции `Serilog`.
- [ ] В классах — `ILogger<T>`, не статический `Log.Logger`.
- [ ] Логи структурированные: данные — параметрами шаблона.
- [ ] SQL логируется через `SqlLoggingInterceptor`.
- [ ] В логи не попадают секреты и персональные данные.
