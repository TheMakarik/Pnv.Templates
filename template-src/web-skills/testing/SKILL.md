---
name: testing
description: Тесты xUnit + FluentAssertions + AutoFixture + FakeItEasy в Tests/Pnv.Web.Tests: сервисы и handler'ы MediatR, DbContext на EF InMemory, naming MethodName_ExpectedResult, Arrange/Act/Assert. Используй при написании тестов.
---

# Тестирование

## Стек

- xUnit — тест-раннер.
- FluentAssertions — проверки.
- AutoFixture — генерация тестовых данных.
- FakeItEasy — подделки зависимостей.

Тесты лежат в `Tests/Pnv.Web.Tests`.

## Что тестировать

- Сервисы (`GreetingService`).
- Handler'ы MediatR (команды и запросы).

DbContext в тестах — EF InMemory (пакет `Microsoft.EntityFrameworkCore.InMemory`
уже подключён) с уникальным именем базы:

```csharp
var options = new DbContextOptionsBuilder<AppDbContext>()
    .UseInMemoryDatabase(Guid.NewGuid().ToString())
    .Options;
```

## Naming и структура

Имя теста — `MethodName_ExpectedResult`. Тело — Arrange/Act/Assert с комментариями.

```csharp
public sealed class CreateGreetingHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsGreetingWithText()
    {
        // Arrange
        var store = new InMemoryGreetingStore();
        var handler = new CreateGreetingHandler(store);
        var command = new CreateGreetingCommand("Привет");

        // Act
        var greeting = await handler.Handle(command, CancellationToken.None);

        // Assert
        greeting.Text.Should().Be("Привет");
    }
}
```

## Запреты

- Не поднимать `WebApplication` (`WebApplicationFactory`) — это unit-тесты.
- Не ходить в реальный PostgreSQL.

## Чек-лист

- [ ] Имя `MethodName_ExpectedResult`, тело AAA с комментариями.
- [ ] Зависимости подменяются FakeItEasy, данные — AutoFixture.
- [ ] DbContext — InMemory с уникальным именем базы.
- [ ] Нет запуска приложения, сети и реальной БД.
