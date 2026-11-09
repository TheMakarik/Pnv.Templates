---
name: endpoints
description: Минимальный API размещается только в папке Endpoints: статический класс с MapXxxEndpoints, MapGroup + WithTags, TypedResults, record-DTO рядом с endpoint'ами. Используй при добавлении и изменении HTTP-endpoint'ов.
---

# Минимальный API (Endpoints)

## Железное правило

Минимальный API размещается ТОЛЬКО в папке `Endpoints` проекта, в файлах `*Endpoints.cs`.

Запрещено:

- `MapGet`/`MapPost` прямо в `Program.cs`;
- `MapGet`/`MapPost` в других слоях (Application, Infrastructure, Services).

В `Program.cs` — только строка подключения группы:

```csharp
app.MapGreetingEndpoints();
```

## Формат файла

Статический класс с extension-методом `MapXxxEndpoints(this IEndpointRouteBuilder app)`.
Внутри — `MapGroup("/api/xxx").WithTags("Xxx")` и методы `MapGet`/`MapPost`.
Возврат — через `TypedResults` (`Ok`, `Created`, `BadRequest`, `ValidationProblem`, ...).
DTO (`record`) — в том же файле, рядом с endpoint'ами.

```csharp
namespace Pnv.Web.Endpoints;

public static class GreetingEndpoints
{
    public static IEndpointRouteBuilder MapGreetingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/greetings").WithTags("Greetings");

        group.MapGet("/", async (ISender sender, CancellationToken cancellationToken)
            => TypedResults.Ok(await sender.Send(new GetGreetingsQuery(), cancellationToken)));

        group.MapPost("/", async (CreateGreetingRequest request, ISender sender, CancellationToken cancellationToken)
            => TypedResults.Created("/api/greetings", await sender.Send(new CreateGreetingCommand(request.Text), cancellationToken)));

        return app;
    }
}

public sealed record CreateGreetingRequest(string Text);
```

Без CQRS вместо `ISender` принимай параметром сервис из `Application`/`Services`.

## Как добавить endpoint

- Новый роут существующего ресурса — новый метод в существующей группе.
- Новый ресурс — новый файл `XxxEndpoints.cs` в `Endpoints` и строка
  `app.MapXxxEndpoints();` в `Program.cs`.

## Чек-лист

- [ ] Endpoint'ы лежат в `Endpoints/*Endpoints.cs`, не в `Program.cs` и не в других слоях.
- [ ] Группа: `MapGroup("/api/xxx").WithTags("Xxx")`.
- [ ] Возврат через `TypedResults`, а не `Results`.
- [ ] DTO — `record` в том же файле рядом с endpoint'ами.
