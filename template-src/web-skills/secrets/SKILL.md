---
name: secrets
description: .NET user secrets для локальных секретов: UserSecretsId в csproj, команды set/list/clear, автоподхват в Development, передача через переменные окружения в Docker. Используй при работе со строками подключения и ключами API.
---

# Секреты (.NET user secrets)

## Подготовка

`UserSecretsId` уже прописан в csproj — дополнительная настройка не нужна.

Команды выполняются в каталоге проекта (например, `Backend/Pnv.Web`):

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=pnvweb;Username=postgres;Password=<пароль>"
dotnet user-secrets list
dotnet user-secrets clear
```

## Как работают

`WebApplication.CreateBuilder` подхватывает user secrets автоматически
в среде Development. Код читает их как обычную конфигурацию:

```csharp
var connectionString = builder.Configuration.GetConnectionString("Default");
```

Включать что-то дополнительно не надо.

## Что хранить в секретах

- Пароли БД (строку подключения целиком).
- Ключи API и токены.

## Что запрещено

- Коммитить секреты в git.
- Класть секреты в `appsettings.json` — даже «временные».

## Docker

User secrets в контейнере недоступны. Секреты передаются переменными
окружения (двойное подчёркивание вместо двоеточия):

```yaml
environment:
  ConnectionStrings__Default: Host=db;Port=5432;Database=pnvweb;Username=postgres;Password=postgres
```

## Чек-лист

- [ ] Локальные секреты — в user secrets, не в `appsettings.json`.
- [ ] Секреты не попадают в git и в логи.
- [ ] В Docker секреты — через переменные окружения.
