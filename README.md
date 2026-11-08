# ПНВ: шаблоны .NET

Набор шаблонов `dotnet new` для проектов ПНВ.

Сейчас в разработке только десктопный шаблон:

- **ПНВ: ДесктопПриложение** (`pnv-desktop`) — AvaloniaUI, .NET 10, C# 14,
  с выбором DI, MVVM, EF Core (SQLite), тестов, скиллов и структуры решения.

## Требования

- .NET SDK 10.0.x
- Python 3.10+

## Сборка и установка

```bash
python3 build/pack.py --install
```

Скрипт собирает `artifacts/Pnv.Templates.1.0.1.nupkg`, удаляет старую версию
пакета и устанавливает новую.

Проверка, что шаблон виден:

```bash
dotnet new list pnv
```

Ожидаемый результат — `ПНВ: ДесктопПриложение` с коротким именем
`pnv-desktop`.

## Создание проекта

```bash
dotnet new pnv-desktop -n MyApp -o MyApp
```

Примеры с опциями:

```bash
# ReactiveUI + Scrutor + EF Core + split-проекты
dotnet new pnv-desktop -n MyApp \
  --di HostingScrutor --mvvm ReactiveUI --ef true --split true

# Минимальный вариант без DI, тестов и скиллов
dotnet new pnv-desktop -n MyApp --di None --tests false --skills false

# Конкретная версия Avalonia
dotnet new pnv-desktop -n MyApp --avalonia-version 12.0.4
```

Полный список опций:

```bash
dotnet new pnv-desktop --help
```

## Опции

| Опция | Значения | По умолчанию |
|---|---|---|
| `--di` | `None`, `Hosting`, `HostingScrutor` | `Hosting` |
| `--mvvm` | `CommunityToolkit`, `ReactiveUI` | `CommunityToolkit` |
| `--tests` | `true` / `false` | `true` |
| `--skills` | `true` / `false` | `true` |
| `--logging` | `true` / `false` | `true` |
| `--ef` | `true` / `false` | `false` |
| `--split` | `true` / `false` | `false` |
| `--avalonia-version` | `12.0.4` … `12.1.5` | `12.1.5` |
| `--no-restore` | `true` / `false` | `false` |

`--ef` и `--logging` работают только при включённом DI. При `--di None`
EF Core и Serilog не добавляются, даже если указаны `--ef true` или
`--logging true`.

## Что создаётся

- `Source/<Name>` — приложение Avalonia (в split-режиме — UI-проект).
- `Source/<Name>.Core`, `.Modules`, `.Infrastructure` — split-проекты;
  `Infrastructure` только при включённом EF Core.
- `Tests/<Name>.Tests` — тесты (если `--tests true`).
- `Skills/` — скиллы для OpenCode и ИИ-агентов.
- `.claude/skills/` — зеркало скиллов для Claude Code.
- `AGENTS.md`, `CLAUDE.md`, `opencode.json` — при `--skills true`.

При `--logging false` из проекта убираются пакеты Serilog, настройка
логирования в `Bootstrapper` и секция `Serilog` в `appsettings.json`.
Остаётся стандартное логирование `Microsoft.Extensions.Hosting`.

Набор скиллов зависит от опций: скиллы по DI, MVVM, EF Core и тестам
добавляются только вместе с соответствующими технологиями.

## Проверка

```bash
python3 build/verify.py
```

Скрипт устанавливает пакет и проверяет:

- все комбинации `Di × Mvvm × SplitProjects` (с EF Core там, где он допустим);
- включение и выключение логирования;
- игнорирование `--ef` при `--di None`;
- сборку на каждой поддерживаемой версии Avalonia;
- отсутствие шаблонных директив в сгенерированных файлах;
- соответствие набора скиллов выбранным опциям;
- `dotnet test` для default- и split+EF-вариантов.

## Разработка

- `SPEC.md` — полная спецификация шаблона.
- `template-src/desktop/.template.config/` — конфигурация шаблона.
- `template-src/desktop-content/` — контент, подключаемый через `sources`.
- `template-src/skills/` — канонические скиллы.
- `build/pack.py` — стейджинг и упаковка.
- `build/verify.py` — матрица генераций, сборок и тестов.

### Условные директивы

- В C#: `#if (SYMBOL)` / `#endif`.
- В XML (`.csproj`, `.slnx`): `<!--#if (SYMBOL) -->` / `<!--#endif -->`.
- В JSON директивы не используются: варианты файлов подключаются через
  `sources` с `condition`.
