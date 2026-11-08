---
name: testing
description: Написание тестов на xUnit с FakeItEasy, FluentAssertions и AutoFixture: структура класса, Dummy vs Fake, Arrange/Act/Assert, именование. Используй при создании и правке тестов.
---

# Тестирование

## Где лежат

`Tests/<Name>.Tests`, неймспейс `<Name>.Tests.<Секция>`.
Имя файла — `<ТестируемыйТип>Tests.cs`.

```text
Tests/PnvApp.Tests/
├── PnvApp.Tests.csproj
├── Services/
│   └── GreetingServiceTests.cs
└── ViewModels/
    └── MainViewModelTests.cs
```

## Инструменты

- **xUnit** — фреймворк.
- **FakeItEasy** — фейки зависимостей.
- **FluentAssertions** — утверждения.
- **AutoFixture** — генерация данных.

## Структура класса

- Тестируемый объект — поле `systemUnderTests`.
- Общие фейки — в полях и конструкторе.
- Один тестовый класс на один тип.
- `IDisposable` — только если нужно освобождать ресурсы.

```csharp
using AutoFixture;
using FluentAssertions;
using PnvApp.Services;
using Xunit;

namespace PnvApp.Tests.Services;

public sealed class GreetingServiceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GreetingService systemUnderTests = new();

    [Fact]
    public void GetGreeting_ReturnsGreetingWithName()
    {
        // Arrange
        var name = _fixture.Create<string>();

        // Act
        var greeting = systemUnderTests.GetGreeting(name);

        // Assert
        greeting.Text.Should().Contain(name);
    }
}
```

## Dummy vs Fake

- `A.Dummy<T>()` — зависимость нужна конструктору, поведение не проверяется.
- `A.Fake<T>()` — поведение настраивается или проверяется.

```csharp
var logger = A.Dummy<ILogger<GreetingService>>();

var greetingService = A.Fake<IGreetingService>();
A.CallTo(() => greetingService.GetGreeting(A<string>._))
    .Returns(new Greeting("тест"));
```

Правила:

- Не настраивай `A.Dummy` — для этого есть `A.Fake`.
- Не проверяй вызовы, которые не важны для теста.
- Фейк интерфейса, а не конкретного класса.

## Тест ViewModel

```csharp
using FakeItEasy;
using FluentAssertions;
using PnvApp.Models;
using PnvApp.Services;
using PnvApp.ViewModels;
using Xunit;

namespace PnvApp.Tests.ViewModels;

public sealed class MainViewModelTests
{
    [Fact]
    public void SayHello_SetsGreetingFromService()
    {
        // Arrange
        var greetingService = A.Fake<IGreetingService>();
        A.CallTo(() => greetingService.GetGreeting(A<string>._))
            .Returns(new Greeting("тест"));
        var systemUnderTests = new MainViewModel(greetingService);

        // Act
        systemUnderTests.SayHelloCommand.Execute(null);

        // Assert
        systemUnderTests.Greeting.Should().Be("тест");
    }
}
```

- ViewModel тестируется без Avalonia: команды и свойства доступны напрямую.
- `[RelayCommand]` и `[ReactiveCommand]` создают `SayHelloCommand` в обоих
  вариантах MVVM.
- Не создавай `Window` в тестах ViewModel.

## Именование

`Действие_Условие_ОжидаемыйРезультат`:

```csharp
[Fact]
public void GetGreeting_EmptyName_ReturnsGreetingWithEmptyName() { }

[Fact]
public void SayHello_ServiceThrows_PropagatesException() { }
```

## Утверждения

- FluentAssertions: `.Should().Be(...)`, `.Should().Contain(...)`,
  `.Should().BeEmpty()`, `.Should().ThrowAsync<T>()`.
- Один логический ассерт на тест; несколько проверок — только над одним
  результатом.
- Не сравнивай строки без указания причины: `.Should().Be("тест")`.

```csharp
// Хорошо
greeting.Text.Should().Be("Привет, мир!");

// Плохо: проверка «хоть что-то»
greeting.Should().NotBeNull();
```

## Файлы и пути

- Временные каталоги создавай в `Path.GetTempPath()`.
- Пути — через `Path.Join`.
- Удаляй временные файлы в `Dispose`.
- Не пиши тесты, зависящие от текущего каталога процесса.

## Чего не делать

```csharp
// Плохо: тест зависит от реальной БД
var dbContext = new AppDbContext();
```

```csharp
// Плохо: несколько действий в Act
systemUnderTests.SayHelloCommand.Execute(null);
systemUnderTests.ResetCommand.Execute(null);
```

```csharp
// Плохо: Thread.Sleep вместо ожидания результата
Thread.Sleep(1000);
```

## Чек-лист

- [ ] Тест лежит в `Tests/<Name>.Tests` и повторяет секцию тестируемого типа.
- [ ] Есть Arrange/Act/Assert.
- [ ] Зависимости — фейки, а не реальные сервисы.
- [ ] Имя теста описывает поведение.
- [ ] Тест не зависит от сети, БД и текущего каталога.
