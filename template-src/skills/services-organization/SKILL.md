---
name: services-organization
description: Как проектировать сервисы: контракт, реализация, конструкторная инъекция, async, размещение в single/split. Используй при добавлении бизнес-операций.
---

# Сервисы

## Где лежат

| Режим | Контракт | Реализация |
|---|---|---|
| Один проект | `Source/<Name>/Services` | `Source/<Name>/Services` |
| Split | `Source/<Name>.Core/Services` | `Source/<Name>.Modules/Services` |

Неймспейс в обоих случаях — `<Name>.Services`.

## Контракт и реализация

- Контракт: `I<Имя>Service`.
- Реализация: `<Имя>Service`, `sealed`.
- Один контракт — один файл.
- Реализация не наследует другие реализации и не содержит UI-кода.

```csharp
namespace PnvApp.Services;

public interface IGreetingService
{
    Greeting GetGreeting(string name);
}
```

```csharp
namespace PnvApp.Services;

public sealed class GreetingService : IGreetingService
{
    public Greeting GetGreeting(string name) => new($"Привет, {name}!");
}
```

## Зависимости

Зависимости приходят только через конструктор:

```csharp
public sealed class ReportService(IReportStorage storage, ILogger<ReportService> logger)
    : IReportService
{
    public async Task<Report> BuildAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Building report");
        return await storage.LoadAsync(cancellationToken);
    }
}
```

- Не запрашивай сервисы из `IServiceProvider` внутри бизнес-кода.
- Не создавай зависимости через `new`, если они уже зарегистрированы.
- Не передавай в сервис `Window`, `Control` или `Application`.

## Async

- Асинхронные методы заканчиваются на `Async`.
- Всегда принимай `CancellationToken` в публичных async-операциях.
- Не используй `.Result`, `.Wait()` и `async void`.
- Для параллельной работы — `Task.WhenAll`.

## Lifetime

- `Singleton` — stateless-сервисы и общее состояние приложения.
- `Transient` — дешёвые объекты без разделяемого состояния.
- `Scoped` в десктопе обычно не нужен.

Регистрация — в `Bootstrapper` (см. скиллы `dependency-injection`
и `dependency-injection-scrutor`).

## Чего не делать

```csharp
// Плохо: сервис знает про View и создаёт окно
public sealed class GreetingService
{
    public void Show() => new MainWindow().Show();
}
```

```csharp
// Плохо: сервис сам достаёт зависимости из контейнера
public sealed class GreetingService(IServiceProvider provider) { }
```
