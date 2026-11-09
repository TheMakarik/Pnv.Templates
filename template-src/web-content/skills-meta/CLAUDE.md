# Правила работы с проектом

## Скиллы

Проектные скиллы лежат в `Skills/<id>/SKILL.md`. Для Claude Code создано
зеркало в `.claude/skills/<id>/SKILL.md` с тем же содержимым. Перед задачей
найди релевантный скилл, открой файл и прочитай его целиком. Не перечисляй
скиллы по памяти: их набор зависит от опций, выбранных при создании проекта.

## Стиль кода

- File-scoped namespaces.
- Primary constructors, `sealed` по умолчанию.
- `var`, когда тип очевиден; `is null` / `is not null`.
- Один публичный тип на файл, без вложенных классов.
- Без XML-документации и закомментированного кода.
- Минимальный API — только в папке `Endpoints`.
- Фронтенд — только TypeScript, никогда JavaScript.

## Структура

- `Backend/<Name>` — API и бэкенд-слои, `Tests/<Name>.Tests` — тесты.
- `Endpoints` — только минимальный API (MapXxxEndpoints + DTO).
- `Domain` / `Models` — сущности и модели без логики приложения.
- `Application` / `Services` — бизнес-операции (или handler'ы MediatR).
- `Infrastructure` — EF Core, PostgreSQL, внешние интеграции.
- `Frontend` — фронтенд-проекты (Blazor / React / Vue), когда выбраны.

Перед изменением архитектуры прочитай `Skills/architecture/SKILL.md`.
