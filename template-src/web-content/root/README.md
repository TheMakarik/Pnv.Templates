# Pnv.Web

Веб-приложение на ASP.NET Core Minimal API (.NET 10, C# 14).

## Запуск

```bash
dotnet run --project Backend/Pnv.Web
```

API: http://localhost:5080

- `GET /api/greetings` — список приветствий
- `POST /api/greetings` — создать (`{"text": "..."}`)
- `GET /` — информация о приложении

## PostgreSQL

Строка подключения — `ConnectionStrings:Default` (appsettings.json). Пароль
лучше хранить в user secrets:

```bash
cd Backend/Pnv.Web
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=pnvweb;Username=postgres;Password=<пароль>"
```

## Фронтенд (если выбран)

- Razor Pages — страницы в проекте бэкенда.
- Razor Pages + Vue / React — `ClientApp/` в проекте бэкенда:
  `npm install && npm run build` (результат в `wwwroot/dist`).
- Blazor WebAssembly / Blazor Server — `dotnet run --project Frontend/Pnv.Web.Frontend`.
- React / Vue — самостоятельное SPA в `Frontend/`: `npm install && npm run dev`.

Везде используется TypeScript — никогда JavaScript.

## Docker (если выбран)

```bash
docker compose up --build
```

Поднимает API (http://localhost:8080) и PostgreSQL 17.

## Тесты

```bash
dotnet test
```

## Структура

```text
Backend/Pnv.Web/                 # API и бэкенд-слои (одиночная архитектура)
Backend/Pnv.Web.Api/             # стартовый проект (split-архитектура)
Backend/Pnv.Web.Core/            # Domain + Application (split-архитектура)
Backend/Pnv.Web.Infrastructure/  # EF Core + PostgreSQL (split-архитектура)
Frontend/                        # фронтенд-проекты (если выбраны)
Tests/Pnv.Web.Tests/             # тесты
Skills/                          # скиллы для ИИ-агентов
```

Минимальный API — только в папке `Endpoints`. Перед изменением архитектуры
прочитай `Skills/architecture/SKILL.md`.
