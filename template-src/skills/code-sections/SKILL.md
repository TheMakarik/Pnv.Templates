---
name: code-sections
description: Правила раскладки типов по секциям Services/Models/View/ViewModels/Infrastructure. Используй, когда решаешь, в какую папку положить новый тип и как назвать файл.
---

# Секции кода

## Главное правило

Корень проекта — не свалка. В корне UI-проекта допустимы только
точки входа и композиция: `Program.cs`, `App.axaml(.cs)`, `Bootstrapper.cs`.
Всё остальное лежит в тематической секции.

## Секции

| Секция | Что хранит | Пример |
|---|---|---|
| `Models` | доменные модели, DTO, EF-сущности | `Greeting`, `GreetingRecord` |
| `Services` | контракты и реализации бизнес-операций | `IGreetingService`, `GreetingService` |
| `Options` | классы настроек для `IOptions<T>` | `AppOptions`, `DatabaseOptions` |
| `Infrastructure` | БД, файлы, внешние интеграции | `AppDbContext`, `SqlLoggingInterceptor` |
| `ViewModels` | состояние экрана и команды | `MainViewModel`, `ViewModelBase` |
| `View` | XAML и code-behind | `MainWindow.axaml`, `MainWindow.axaml.cs` |

## Один публичный тип — один файл

- Имя файла строго совпадает с именем публичного типа.
- Вложенные классы запрещены.
- Вспомогательный `enum`/`record`/`struct` можно положить рядом только
  с основным типом, если он используется исключительно им.

```csharp
// Хорошо: Models/Greeting.cs
namespace Pnv.Desktop.Models;

public sealed record Greeting(string Text);
```

```csharp
// Плохо: в одном файле два публичных класса
public sealed class GreetingService { }
public sealed class GreetingValidator { }
```

## Именование папок

- Папка = роль, а не «фича на глаз»: `Models`, `Services`, `View`.
- Не создавай `Helpers`, `Utils`, `Common`, `Misc` — это признак
  неопределённой ответственности. Если тип некуда положить, сначала
  определи его роль.
- Вложенные подпапки допустимы по смыслу: `Infrastructure/Interceptors`,
  `View/Controls`, `View/Converters`.

## Связи между секциями

- `Models` не знают о `Services`, `ViewModels` и `View`.
- `Services` не знают о `ViewModels` и `View`.
- `ViewModels` знают о `Services` и `Models`, но не о `View`.
- `View` знает о `ViewModels` только через `x:DataType` и биндинги.
- `Infrastructure` знает о `Models`, но не о `View`.

## Неймспейсы

Неймспейс повторяет секцию, а не физический проект:

```csharp
namespace Pnv.Desktop.Models;
namespace Pnv.Desktop.Services;
namespace Pnv.Desktop.Infrastructure;
namespace Pnv.Desktop.ViewModels;
namespace Pnv.Desktop.View;
```

В split-режиме проекты `<Name>.Core`, `<Name>.Modules` и
`<Name>.Infrastructure` всё равно используют эти же неймспейсы.
