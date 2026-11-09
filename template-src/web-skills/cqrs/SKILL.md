---
name: cqrs
description: MediatR как замена сервисов: команды и запросы — records с IRequest<T>, handler'ы IRequestHandler в Features/<Сущность>, отправка через ISender, авторегистрация handler'ов, без behaviors и notifications. Используй при добавлении бизнес-операций.
---

# CQRS (MediatR)

## Роль MediatR

MediatR здесь — только замена сервисов: каждая бизнес-операция — команда
или запрос с одним handler'ом.

НЕ использовать:

- pipeline behaviors;
- валидационные пайплайны;
- notifications (`INotification`).

## Команды и запросы

`record` с `IRequest<T>`; namespace — `...Features.<Сущность>`:

```csharp
namespace Pnv.Web.Features.Greetings.Commands;

public sealed record CreateGreetingCommand(string Text) : IRequest<Greeting>;
```

Команды — в `Features/<Сущность>/Commands`, запросы — в `Features/<Сущность>/Queries`.

## Handler'ы

`IRequestHandler<TRequest, TResponse>` рядом с командой/запросом:

```csharp
public sealed class CreateGreetingHandler(InMemoryGreetingStore store)
    : IRequestHandler<CreateGreetingCommand, Greeting>
{
    public Task<Greeting> Handle(CreateGreetingCommand command, CancellationToken ct)
    {
        var greeting = store.Create(command.Text);
        return Task.FromResult(greeting);
    }
}
```

Без EF Core handler получает `InMemoryGreetingStore`, а не DbContext.

## Отправка

Endpoints отправляют операции через `ISender`:

```csharp
var greeting = await sender.Send(new CreateGreetingCommand(request.Text), ct);
```

Прямые вызовы handler'ов запрещены.

## Регистрация

Handler'ы регистрируются автоматически (`RegisterServicesFromAssembly`
в композиционном корне). Вручную регистрировать handler'ы не надо.

## Чек-лист

- [ ] Команда/запрос — `record` с `IRequest<T>` в `Features/<Сущность>`.
- [ ] Handler — `IRequestHandler` рядом с операцией.
- [ ] Отправка — `ISender.Send`, без прямых вызовов handler'ов.
- [ ] Никаких behaviors, пайплайнов и notifications.
- [ ] Handler'ы не регистрируются вручную.
