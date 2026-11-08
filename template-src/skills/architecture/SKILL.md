---
name: architecture
description: Слои и структура решения в обоих режимах шаблона (один проект и split на Core/Modules/Infrastructure). Используй при проектировании структуры, размещении типов и проверке направления зависимостей.
---

# Архитектура решения

## Два режима шаблона

Шаблон умеет создавать решение в двух видах. Логические слои одинаковые,
меняется только физическая упаковка.

### Один проект (`SplitProjects=false`)

```text
Source/<Name>/
├── View/            # XAML и code-behind
├── ViewModels/
├── Models/
├── Services/
├── Options/         # при включённом DI
├── Infrastructure/  # при включённом EF Core
├── Bootstrapper.cs  # при включённом DI
├── App.axaml(.cs)
└── Program.cs
```

### Несколько проектов (`SplitProjects=true`)

```text
Source/<Name>/                  # UI: App, Program, View, ViewModels, Bootstrapper
Source/<Name>.Core/             # модели, контракты сервисов, опции
Source/<Name>.Modules/          # реализации сервисов
Source/<Name>.Infrastructure/   # EF Core: DbContext, интерцепторы
```

## Направление зависимостей

```text
<Name> (UI) ──► <Name>.Modules ──► <Name>.Core
     │                                  ▲
     └────────► <Name>.Infrastructure ──┘
```

- `Core` не зависит ни от чего, кроме BCL.
- `Modules` зависит только от `Core`.
- `Infrastructure` зависит только от `Core` (и EF Core).
- UI зависит от `Modules` и `Infrastructure`, но не наоборот.

Запрещено: `Core` → `Modules`, `Core` → UI, `Modules` → UI.
UI-типы (`Window`, `Control`, `Application`) не должны появляться в `Core`,
`Modules` и `Infrastructure`.

## Пространства имён

Пространства имён **не зависят от режима** и всегда начинаются с имени решения:

- `<Name>.Models`
- `<Name>.Services`
- `<Name>.Infrastructure`
- `<Name>.ViewModels`
- `<Name>.View`
- `<Name>.Options`

В split-проектах выставлен `<RootNamespace><Name></RootNamespace>`,
поэтому папка `Models` в проекте `<Name>.Core` всё равно даёт
`namespace <Name>.Models;`.

## Композиция

Единственная точка сборки зависимостей — `Bootstrapper` в UI-проекте
(создаётся только при включённом DI). Там же настраиваются опции,
логирование и EF Core. В остальном коде контейнер не используется:
зависимости приходят через конструктор.

Без DI композиция выполняется в `App.axaml.cs` вручную:
`new MainViewModel(new GreetingService())`.

## Проверка перед добавлением типа

1. Это модель/контракт? → `Core` (split) или `Models`/`Services` (single).
2. Это реализация бизнес-операции? → `Modules` (split) или `Services` (single).
3. Это работа с БД/файлами/внешним миром? → `Infrastructure`.
4. Это состояние экрана/команда? → `ViewModels`.
5. Это разметка/визуальное поведение? → `View`.
