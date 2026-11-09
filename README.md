# Pnv.Templates — NuGet-шаблоны проектов

Пакет шаблонов `dotnet new` для быстрых стартов под `.NET 10` / `C# 14` с русскими
именами в IDE (JB Rider, Visual Studio). Все шаблоны генерируют рабочее решение
сборкой с нуля и сопровождаются скиллами для ИИ-агентов (OpenCode / Claude Code).

| Шаблон | shortName | Что генерирует |
|---|---|---|
| ПНВ: ДесктопПриложение | `pnv-desktop` | AvaloniaUI-приложение |
| ПНВ: ВебПриложение | `pnv-web` | ASP.NET Core Minimal API (+ веб-фронтенд по выбору) |
| ПНВ: Комбо | `pnv-combo` | Десктоп + API + веб-фронтенд в одном решении |

## Установка

Публикации на nuget.org нет — пакет ставится из локального файла:

```bash
pwsh build/pack.ps1 -Install              # собрать и установить
# или только установить уже собранный:
dotnet new install artifacts/Pnv.Templates.2026.0.1.nupkg
```

После установки шаблоны видны в `dotnet new list` и в New Solution в IDE.
Для Rider при обновлении пакета сбросьте кеш: `rm -rf ~/.templateengine/Rider`.

## Опции шаблонов

Регистр CLI-опций важен: `--ApiClient`, `--Architecture`, `--Frontend`, `--Docker`,
`--SkipRestore` (у десктопного шаблона restore-флаг — `--no-restore`).

### pnv-desktop — ПНВ: ДесктопПриложение

| Опция | CLI | Значения | По умолчанию |
|---|---|---|---|
| Внедрение зависимостей | `--Di` | `None` / `Hosting` / `HostingScrutor` | `Hosting` |
| MVVM Framework | `--Mvvm` | `CommunityToolkit` / `ReactiveUI` | `CommunityToolkit` |
| Версия AvaloniaUI | `--AvaloniaVersion` | 12.0.4 … 12.1.5 | `12.1.5` |
| Логирование Serilog | `--Logging` | true/false | `true` |
| EF Core (SQLite) | `--EfCore` | true/false | `false` |
| Разделить на проекты | `--SplitProjects` | true/false | `false` |
| Тесты / Скиллы | `--Tests` / `--Skills` | true/false | `true` |

Решение: `Source/<Name>` (+ `.Core` / `.Modules` / `.Infrastructure` при split), `Tests/`.

### pnv-web — ПНВ: ВебПриложение

| Опция | CLI | Значения | По умолчанию |
|---|---|---|---|
| Архитектура | `--Architecture` | `LayeredSingle` / `LayeredSplit` / `CleanArchitecture` / `CleanArchitectureCqrs` | `CleanArchitectureCqrs` |
| Фронтенд | `--Frontend` | `None` / `RazorPages` (+Vue/React) / `BlazorWasm` / `BlazorServer` / `React` / `Vue` | `RazorPages` |
| EF Core (PostgreSQL) | `--EfCore` | true/false | `true` |
| Логирование Serilog | `--Logging` | true/false | `true` |
| Scrutor | `--Scrutor` | true/false | `false` |
| Секреты / Docker | `--Secrets` / `--Docker` | true/false | true / false |
| Тесты / Скиллы | `--Tests` / `--Skills` | true/false | `true` |

Галочка «Использовать Scrutor» — регистрация сервисов сканированием сборки;
при архитектуре CQRS регистрацию выполняет MediatR, галочка ни на что не влияет.

### pnv-combo — ПНВ: Комбо

Комбо = десктоп + API + веб-фронтенд. Структура: `Desktop/`, `Backend/`,
`Frontend/`, `Tests/` (два проекта тестов), `Skills/`.

Опции десктопа и бэкенда настраиваются **раздельно**, что отражено в подписях
в IDE:

| Опция | CLI | Значения | По умолчанию |
|---|---|---|---|
| Связка десктопа с API | `--ApiClient` | `None` (нет связки) / `HttpClient` / `Refit` | `HttpClient` |
| Архитектура бэкенда | `--Architecture` | как у pnv-web | `CleanArchitectureCqrs` |
| Фронтенд | `--Frontend` | как у pnv-web | `RazorPages` |
| EF Core **в десктопе** (SQLite) | `--DesktopEfCore` | true/false | `false` |
| EF Core **в бэкенде** (PostgreSQL) | `--WebEfCore` | true/false | `true` |
| Serilog **в десктопе** / **в бэкенде** | `--DesktopLogging` / `--WebLogging` | true/false | `true` |
| DI **в десктопе** | `--Di` | `None` / `Hosting` / `HostingScrutor` | `Hosting` |
| MVVM **в десктопе** | `--Mvvm` | `CommunityToolkit` / `ReactiveUI` | `CommunityToolkit` |
| Десктоп: разделить на проекты | `--SplitProjects` | true/false | `false` |
| Scrutor **в бэкенде** | `--Scrutor` | true/false | `false` |
| Версия AvaloniaUI | `--AvaloniaVersion` | 12.0.4 … 12.1.5 | `12.1.5` |
| Секреты / Docker / Тесты / Скиллы | `--Secrets` / `--Docker` / `--Tests` / `--Skills` | true/false | см. `dotnet new` |

При «Нет связки» десктоп работает с локальным `IGreetingService`, не обращаясь
к API; при HttpClient/Refit — через сгенерированный `IGreetingsApiClient`.

## Разработка

```
template-src/            — источники шаблонов
  desktop/ web/ combo/   — контракты (.template.config/template.json)
  desktop-content/       — контент десктопного шаблона
  web-content/           — контент веб-шаблона
  combo-content/         — контент комбо (копии с переименованием имён/символов)
  combo-skills/          — merged-скиллы комбо
  skills/ web-skills/    — скиллы десктопа и веба
build/pack.ps1           — staging + сборка .nupkg (-Install ставит пакет)
build/verify.ps1         — матрица сборок/тестов десктопного шаблона
version.txt              — версия пакета (для сборки и релизов)
artifacts/               — собранный пакет
```

Проверенная матрица: все опции комбо (связка None/HttpClient/Refit, split-десктоп,
EF Core и Serilog раздельно, Scrutor, фронтенды) — сборка 0 ошибок, тесты зелёные.

## Релизы

Версия пакета живёт в `version.txt` в корне. Изменение этого файла в `main`
(или ручной запуск **Actions → Release**) запускает сборку: пакет проходит
смок-проверку трёх шаблонов, затем создаётся GitHub Release `v<версия>` с
`Pnv.Templates.<версия>.nupkg`.

В workflow не используется ни одного секрета и ключа — только встроенный
`GITHUB_TOKEN` (права `contents: write`); SSH-ключи остаются на локальной
машине и в CI не нужны.
