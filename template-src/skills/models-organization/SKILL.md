---
name: models-organization
description: Как проектировать и раскладывать модели в секции Models. Используй при добавлении DTO, доменных моделей и EF-сущностей.
---

# Модели

## Где лежат

- Один проект: `Source/<Name>/Models`.
- Split: `Source/<Name>.Core/Models`.
- Неймспейс всегда `<Name>.Models`.

## Что считать моделью

- DTO для передачи данных между слоями.
- Доменная сущность без поведения UI.
- EF-сущность (только при включённом EF Core).

Модель не содержит команд, ссылок на `Window`, `Control`, `DbContext`
или сервисы. Поведение — в сервисах, состояние экрана — во ViewModel.

## Record или class

Иммутабельный DTO — `record`:

```csharp
namespace Pnv.Desktop.Models;

public sealed record Greeting(string Text);
```

Изменяемая EF-сущность — `sealed class` с инициализаторами:

```csharp
namespace Pnv.Desktop.Models;

public sealed class GreetingRecord
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}
```

## Правила

- `sealed` по умолчанию.
- `required` для обязательных полей, если нет конструктора.
- Коллекции — только чтение (`IReadOnlyCollection<T>`), если модель
  не предназначена для изменения.
- Никаких публичных полей — только свойства.
- Никакой логики, кроме вычисляемых свойств без побочных эффектов.
- Не добавляй атрибуты сериализации «на всякий случай»: для JSON
  используй source-generated контекст (см. скилл `json-sourcegen`).

## Нейминг

- Имя отражает сущность: `Greeting`, `GreetingRecord`, `ProjectSettings`.
- Не используй суффиксы `Dto`, `Model`, `Data`, если они не несут смысла.
- Для EF-сущности допустим суффикс `Record`/`Entity`, если рядом есть DTO.

## Чего не делать

```csharp
// Плохо: модель знает про UI и БД
public sealed class Greeting
{
    public Window Window { get; set; } = null!;
    public AppDbContext DbContext { get; set; } = null!;
}
```

```csharp
// Хорошо: чистая модель
public sealed record Greeting(string Text);
```
