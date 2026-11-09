---
name: architecture
description: Структура комбо-решения (Desktop + Backend), варианты архитектуры десктопа и бэкенда, правила зависимостей, композиционные корни. Используй при добавлении слоёв, проектов, зависимостей и проверке направления связей.
---

# Архитектура комбо-решения

## Структура каталогов

```text
Desktop/<Name>/            # Avalonia-приложение (UI)
Backend/<Name>.Web/        # API на минимальном ASP.NET Core
Frontend/                  # веб-фронтенд (Blazor/React/Vue), если выбран
Tests/<Name>.Desktop.Tests/
Tests/<Name>.Web.Tests/
Skills/                    # скиллы для ИИ-агентов
```

## Десктоп: два режима упаковки

Логические слои одинаковы, меняется только физическая упаковка (`SplitProjects`).

### Один проект (`SplitProjects=false`)

Всё в `Desktop/<Name>`: `View/`, `ViewModels/`, `Models/`, `Services/`, `Api/`,
`Options/` (при DI), `Infrastructure/` (при EF Core), `Bootstrapper.cs`.

### Несколько проектов (`SplitProjects=true`)

- `Desktop/<Name>` (UI) — App, Program, View, ViewModels, Bootstrapper.
- `Desktop/<Name>.Core` — модели, контракты (`Services/`, `Api/`), опции.
- `Desktop/<Name>.Modules` — реализации сервисов и API-клиента.
- `Desktop/<Name>.Infrastructure` — EF Core (только при включённом EF Core).

Зависимости: `UI → Modules → Core`, `Infrastructure → Core`. `Core` не зависит
ни от кого. Пространства имён не зависят от режима: `<Name>.Services`,
`<Name>.Api`, `<Name>.Models`.

## Бэкенд: варианты архитектуры

- `LayeredSingle` — один проект `Backend/<Name>.Web`: `Domain/`, `Application/`,
  `Endpoints/`, `Infrastructure/` (при EF Core).
- `LayeredSplit` — `Backend/<Name>.Web.Api` (`Program.cs`, `Endpoints/`),
  `Backend/<Name>.Web.Core` (`Domain/`, `Application/`),
  `Backend/<Name>.Web.Infrastructure` (при EF Core). Зависимости:
  `Api → Core + Infrastructure`, `Infrastructure → Core`.
- `CleanArchitecture` — зависимости направлены внутрь к домену:
  `Endpoints → Application`, `Infrastructure → Application`.
- `CleanArchitectureCqrs` — как CleanArchitecture, но MediatR: команды,
  запросы и handler'ы в `Application`.

## Композиционные корни

- Десктоп: `Bootstrapper.cs` — все регистрации: сервисы, опции, DbContext,
  HttpClient-клиенты.
- Бэкенд: `Program.cs` — конфигурация, DI, подключение endpoint'ов.

## Железные правила

- Минимальный API — только в папке `Endpoints` (`*Endpoints.cs`), не в
  `Program.cs` и не в других слоях.
- Фронтенд — всегда TypeScript, никогда JavaScript.
- Десктоп ходит в API только через `IGreetingsApiClient` из папки `Api/`
  (при включённой связке HttpClient/Refit); при «Нет связки» (`ApiClient=None`)
  десктоп работает с локальным `IGreetingService` — никаких `HttpClient`,
  URL и JSON вручную во View и ViewModel.
- `Domain`/`Models` — без логики; бизнес-операции — в `Application`/`Services`
  (или handler'ах MediatR).
- UI-типы (`Window`, `Control`, `Application`) не появляются вне десктопного
  UI-проекта.

Перед изменением архитектуры перечитай этот скилл целиком.
