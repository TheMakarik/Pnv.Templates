# Pnv.Desktop

Десктопное приложение на AvaloniaUI (.NET 10, C# 14).

## Запуск

```bash
dotnet run --project Source/Pnv.Desktop
```

## Тесты

```bash
dotnet test
```

## Структура

```text
Source/Pnv.Desktop/            # приложение: Program, App, View, ViewModels
Source/Pnv.Desktop.Core/       # модели, контракты сервисов, опции (split-режим)
Source/Pnv.Desktop.Modules/    # реализации сервисов (split-режим)
Source/Pnv.Desktop.Infrastructure/  # EF Core и SQLite (при включённом EF)
Tests/Pnv.Desktop.Tests/       # тесты
Skills/                   # скиллы для ИИ-агентов
```

Если решение создано в режиме одного проекта, всё лежит в
`Source/Pnv.Desktop`: `View`, `ViewModels`, `Models`, `Services`, `Options`,
`Infrastructure`.

## Возможности

- **DI**: без внедрения зависимостей, `Microsoft.Extensions.Hosting` или
  `Microsoft.Extensions.Hosting + Scrutor`.
- **Логирование**: Serilog с настройкой из `appsettings.json`. Включается и
  выключается опцией `--logging`; работает только при включённом DI.
- **MVVM**: CommunityToolkit.Mvvm или ReactiveUI.
- **EF Core + SQLite**: `AppDbContext`, инициализация базы и логирование SQL
  через интерцептор. EF Core доступен только при включённом DI.
- **Тесты**: xUnit, FakeItEasy, FluentAssertions, AutoFixture.
- **Скиллы**: `Skills/` для OpenCode и `.claude/skills/` для Claude Code.

## Скиллы

Перед задачей открой релевантный файл `Skills/<id>/SKILL.md` и прочитай его
целиком. Список скиллов зависит от выбранных при создании опций.

`AGENTS.md` и `CLAUDE.md` содержат краткие правила работы с проектом.
