---
name: ef-core-interceptors
description: Логирование SQL и других операций EF Core через DbCommandInterceptor и ILogger<T>, подключение через AddInterceptors. Используй при добавлении интерцепторов и диагностики запросов.
---

# EF Core: интерцепторы

## Назначение

Интерцептор перехватывает операции EF Core до и после выполнения.
В шаблоне он используется для структурированного логирования SQL.

## SqlLoggingInterceptor

```csharp
using System.Data.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace PnvApp.Infrastructure.Interceptors;

public sealed class SqlLoggingInterceptor(ILogger<SqlLoggingInterceptor> logger)
    : DbCommandInterceptor
{
    public override InterceptionResult<DbDataReader> ReaderExecuting(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result)
    {
        logger.LogDebug("SQL: {CommandText}", command.CommandText);
        return result;
    }

    public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(
        DbCommand command,
        CommandEventData eventData,
        InterceptionResult<DbDataReader> result,
        CancellationToken cancellationToken = default)
    {
        logger.LogDebug("SQL: {CommandText}", command.CommandText);
        return ValueTask.FromResult(result);
    }
}
```

Правила:

- Класс `sealed`, primary constructor с `ILogger<T>`.
- Переопределяй и синхронный, и асинхронный вариант.
- Не меняй результат и не выполняй команду повторно.
- Текст SQL передавай параметром шаблона, а не склейкой строк.

## Полезные точки перехвата

| Метод | Когда вызывается |
|---|---|
| `ReaderExecuting` | перед запросом, возвращающим строки |
| `NonQueryExecuting` | перед `INSERT`/`UPDATE`/`DELETE` |
| `ScalarExecuting` | перед запросом одного значения |
| `CommandFailed` | при ошибке выполнения |
| `SaveChangesStarting` | перед сохранением изменений |

Для замера длительности используй `CommandExecuted`/`CommandExecutedAsync`:
`eventData.StartTime` и текущее время.

```csharp
public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    => logger.LogError(eventData.Exception, "SQL failed: {CommandText}", command.CommandText);
```

## Подключение

Интерцептор регистрируется вместе с `DbContext`:

```csharp
services.AddDbContext<AppDbContext>((provider, options) =>
{
    var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    var connectionString = GetConnectionString(databaseOptions);

    options
        .UseSqlite(connectionString)
        .AddInterceptors(new SqlLoggingInterceptor(
            provider.GetRequiredService<ILogger<SqlLoggingInterceptor>>()));
});
```

- Создавай интерцептор через `provider`, чтобы получить `ILogger`.
- Порядок интерцепторов = порядок выполнения.
- Не регистрируй интерцептор как Singleton и не передавай его в `AddDbContext`
  дважды.

## Уровни логирования

- `Debug` — каждый SQL-запрос. В обычной работе это шум.
- `Information` — только значимые операции (миграции, инициализация).
- `Warning`/`Error` — ошибки и медленные запросы.

```csharp
// Хорошо: параметры запроса не логируются
logger.LogDebug("SQL: {CommandText}", command.CommandText);

// Плохо: в лог попадут значения параметров, включая персональные данные
logger.LogDebug("SQL: {CommandText} {@Parameters}", command.CommandText, command.Parameters);
```

## Безопасность

- Не логируй параметры команд: там могут быть пароли и персональные данные.
- Не логируй connection string целиком.
- Не включай подробное логирование SQL в релизной конфигурации по умолчанию.

## Чего не делать

```csharp
// Плохо: интерцептор меняет результат
public override InterceptionResult<DbDataReader> ReaderExecuting(...)
{
    return InterceptionResult<DbDataReader>.SuppressWithResult(...);
}
```

```csharp
// Плохо: логирование через Console
public override void CommandFailed(DbCommand command, CommandErrorEventData eventData)
    => Console.WriteLine(eventData.Exception.Message);
```

## Чек-лист

- [ ] Интерцептор — `sealed`, с `ILogger<T>`.
- [ ] Переопределены sync и async варианты.
- [ ] Интерцептор подключён через `AddInterceptors`.
- [ ] SQL логируется структурированно, без параметров.
- [ ] Уровень логирования не `Information` для каждого запроса.
