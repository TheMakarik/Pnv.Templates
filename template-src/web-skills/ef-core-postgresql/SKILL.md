---
name: ef-core-postgresql
description: EF Core с PostgreSQL через Npgsql: строка подключения в ConnectionStrings:Default, сущности в Domain, AppDbContext/DatabaseInitializer/SqlLoggingInterceptor в Infrastructure, регистрация через AddInfrastructure, миграции dotnet ef. Используй при работе с БД.
---

# EF Core + PostgreSQL

## Пакет и подключение

Провайдер — `Npgsql.EntityFrameworkCore.PostgreSQL`.

Строка подключения — ключ `ConnectionStrings:Default`:

```json
"ConnectionStrings": {
  "Default": "Host=localhost;Port=5432;Database=pnvweb;Username=postgres;Password=postgres"
}
```

Пароль здесь — заглушка: переопределяй строку в user secrets (см. скилл `secrets`).
Не храни строку подключения с реальным паролем в `appsettings.json`.

## Размещение

- Сущности — в `Domain` (проект `Pnv.Web.Core` при split-архитектуре).
- `AppDbContext` — в `Infrastructure`.
- `DatabaseInitializer` — в `Infrastructure`: вызывает `EnsureCreated` при старте.
- `Interceptors/SqlLoggingInterceptor` — в `Infrastructure`: логирует SQL (см. скилл `logging`).

## Регистрация

Только через `AddInfrastructure` в `Infrastructure/DependencyInjection.cs`:

```csharp
services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(connectionString)
        .AddInterceptors(interceptor));
```

Не регистрируй DbContext в `Program.cs` напрямую.

## Миграции

```bash
# одиночная архитектура
dotnet ef migrations add <Name> --project Backend/Pnv.Web --startup-project Backend/Pnv.Web

# split-архитектура
dotnet ef migrations add <Name> --project Backend/Pnv.Web.Infrastructure --startup-project Backend/Pnv.Web.Api
```

Шаблон по умолчанию использует `EnsureCreated`. Для production замени
`EnsureCreated` на `Database.Migrate()` в `DatabaseInitializer`.

## Чек-лист

- [ ] Сущности — в `Domain`, контекст и инициализатор — в `Infrastructure`.
- [ ] Строка подключения — `ConnectionStrings:Default`, пароль — в user secrets.
- [ ] Регистрация — через `AddInfrastructure`, не в `Program.cs`.
- [ ] SQL в логах — через `SqlLoggingInterceptor`.
- [ ] Для production — `Migrate()` вместо `EnsureCreated`.
