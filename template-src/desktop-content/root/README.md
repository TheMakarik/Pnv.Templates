# PnvApp

Десктопное приложение на AvaloniaUI (.NET 10, C# 14).

## Запуск

```bash
dotnet run --project Source/PnvApp
```

## Тесты

```bash
dotnet test
```

## Структура

```text
Source/PnvApp/            # приложение: Program, App, View, ViewModels
Source/PnvApp.Core/       # модели, контракты сервисов, опции (split-режим)
Source/PnvApp.Modules/    # реализации сервисов (split-режим)
Source/PnvApp.Infrastructure/  # EF Core и SQLite (при включённом EF)
Tests/PnvApp.Tests/       # тесты
Skills/                   # скиллы для ИИ-агентов
```

Если решение создано в режиме одного проекта, всё лежит в
`Source/PnvApp`: `View`, `ViewModels`, `Models`, `Services`, `Options`,
`Infrastructure`.

## Возможности

- **DI**: без внедрения зависимостей, `Microsoft.Extensions.Hosting` или
  `Microsoft.Extensions.Hosting + Scrutor`.
- **MVVM**: CommunityToolkit.Mvvm или ReactiveUI.
- **EF Core + SQLite**: `AppDbContext`, инициализация базы и логирование SQL
  через интерцептор. EF Core доступен только при включённом DI.
- **Тесты**: xUnit, FakeItEasy, FluentAssertions, AutoFixture.
- **Скиллы**: `Skills/` для OpenCode и `.claude/skills/` для Claude Code.

## Скиллы

Перед задачей открой релевантный файл `Skills/<id>/SKILL.md` и прочитай его
целиком. Список скиллов зависит от выбранных при создании опций.

`AGENTS.md` и `CLAUDE.md` содержат краткие правила работы с проектом.
