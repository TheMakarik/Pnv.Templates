---
name: architecture
description: Структура решения и варианты архитектуры бэкенда (LayeredSingle, LayeredSplit, CleanArchitecture, CleanArchitectureCqrs), правила зависимостей, композиционный корень. Используй при добавлении слоёв, проектов и зависимостей.
---

# Архитектура решения

## Структура каталогов

```
Backend/    # бэкенд: API и слои
Frontend/   # фронтенд-проекты (Blazor/React/Vue), если выбраны
Tests/      # тесты Pnv.Web.Tests
Skills/     # скиллы для ИИ-агентов
```

## Варианты архитектуры

### LayeredSingle — многослойная в одном проекте

Один проект `Backend/Pnv.Web`, слои — папки:

- `Domain/` — сущности и модели.
- `Application/` — бизнес-операции (сервисы).
- `Infrastructure/` — EF Core, реализации, внешние интеграции.
  Папка добавляется только при включённом EF Core.
- `Endpoints/` — слой презентации: минимальный API.

### LayeredSplit — многослойная в разных проектах

- `Backend/Pnv.Web.Api` — `Program.cs`, `Endpoints/`, `Options/`.
- `Backend/Pnv.Web.Core` — `Domain/`, `Application/`: модели и контракты.
- `Backend/Pnv.Web.Infrastructure` — EF Core, реализации контрактов.
  Проект добавляется только при включённом EF Core.

Зависимости: `Infrastructure → Core`, `Api → Core + Infrastructure`.
`Core` не ссылается ни на кого.

### CleanArchitecture

- `Domain/` — сущности.
- `Application/` — интерфейсы сервисов.
- `Infrastructure/` — реализации интерфейсов + EF Core.
- `Endpoints/` — минимальный API.

Зависимости направлены внутрь, к домену: `Endpoints → Application`,
`Infrastructure → Application`.

### CleanArchitectureCqrs

Как CleanArchitecture, но вместо сервисов — MediatR (см. скилл `cqrs`).

## Композиционный корень

- `Program.cs` — все регистрации верхнего уровня и маппинг endpoint'ов.
- `Infrastructure/DependencyInjection.cs` — метод `AddInfrastructure`:
  DbContext, DatabaseInitializer, интеграции.

## Главное правило

Минимальный API пишется ТОЛЬКО в папке `Endpoints` (см. скилл `endpoints`).
В `Program.cs` — только строка вида `app.MapGreetingEndpoints();`.

## Чек-лист

- [ ] Зависимости направлены к домену; Core/Domain ни на кого не ссылается.
- [ ] Модели — в `Domain`/`Models`, бизнес-логика — в `Application`/`Services` (или handler'ах).
- [ ] EF Core и внешние интеграции — только в `Infrastructure`.
- [ ] Минимальный API — только в `Endpoints`, регистрации — в композиционном корне.
