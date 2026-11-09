# Правила работы с проектом

## Скиллы

Проектные скиллы лежат в `Skills/<id>/SKILL.md`. Перед задачей найди
релевантный скилл, открой файл и прочитай его целиком. Не перечисляй скиллы
по памяти: их набор зависит от опций, выбранных при создании проекта.

## Стиль кода

- File-scoped namespaces.
- Primary constructors, `sealed` по умолчанию.
- `var`, когда тип очевиден; `is null` / `is not null`.
- Один публичный тип на файл, без вложенных классов.
- Без XML-документации и закомментированного кода.
- Минимальный API — только в папке `Endpoints`.
- Фронтенд — только TypeScript, никогда JavaScript.

## Структура

- `Desktop/<Name>` — Avalonia-приложение; при `SplitProjects` — также
  `Desktop/<Name>.Core`, `Desktop/<Name>.Modules` и (при EF Core)
  `Desktop/<Name>.Infrastructure`.
- `Backend/<Name>.Web` — API и бэкенд-слои.
- `Frontend/` — веб-фронтенд (Blazor / React / Vue), когда выбраны.
- `Tests/<Name>.Desktop.Tests` и `Tests/<Name>.Web.Tests` — тесты десктопа и API.
- `Api` (в десктопе) — клиент API: HTTP-запросы только через `IGreetingsApiClient`,
  не во View и ViewModel.
- `Endpoints` — только минимальный API (MapXxxEndpoints + DTO).
- `Domain` / `Models` — сущности и модели без логики приложения.
- `Application` / `Services` — бизнес-операции (или handler'ы MediatR).
- `Infrastructure` — EF Core, PostgreSQL, внешние интеграции.

Перед изменением архитектуры прочитай `Skills/architecture/SKILL.md`.
