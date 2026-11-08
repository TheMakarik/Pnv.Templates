---
name: infrastructure-organization
description: Организация слоя Infrastructure: DbContext, конфигурации сущностей, интерцепторы, инициализаторы, доступ к данным. Используй при добавлении работы с БД, файлами и внешними интеграциями.
---

# Слой Infrastructure

## Где лежит

- Один проект: `Source/<Name>/Infrastructure`.
- Split: `Source/<Name>.Infrastructure`.
- Неймспейс — `<Name>.Infrastructure`.

```text
Infrastructure/
├── AppDbContext.cs
├── DatabaseInitializer.cs
├── Interceptors/
│   └── SqlLoggingInterceptor.cs
├── Configurations/        # IEntityTypeConfiguration<T>
└── Repositories/          # доступ к данным, если нужен
```

## Что здесь живёт

| Тип | Пример |
|---|---|
| `DbContext` | `AppDbContext` |
| Конфигурации сущностей | `GreetingRecordConfiguration` |
| Интерцепторы | `SqlLoggingInterceptor` |
| Инициализаторы | `DatabaseInitializer` |
| Репозитории и шлюзы | `GreetingStorage` |
| Работа с файлами | `FileSettingsStorage` |

Инфраструктура — единственное место, где допустимы `DbContext`,
`DbConnection`, `HttpClient` и прямые обращения к файловой системе.

## Направление зависимостей

```text
Infrastructure ──► Core (Models, контракты)
UI ──► Infrastructure
Infrastructure ──► UI  — запрещено
```

- Infrastructure знает о моделях и контрактах, но не о View и ViewModel.
- Infrastructure не создаёт окна, не показывает диалоги и не логирует
  через `Console`.
- UI не работает с `DbContext` напрямую — только через сервисы.

## Регистрация

Всё подключается в `Bootstrapper`:

```csharp
private static void RegisterInfrastructure(IServiceCollection services)
{
    services.AddDbContext<AppDbContext>((provider, options) =>
    {
        var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        options
            .UseSqlite(GetConnectionString(databaseOptions))
            .AddInterceptors(new SqlLoggingInterceptor(
                provider.GetRequiredService<ILogger<SqlLoggingInterceptor>>()));
    });

    services.AddSingleton<DatabaseInitializer>();
    services.AddSingleton<IGreetingStorage, GreetingStorage>();
}
```

- `DbContext` регистрируется через `AddDbContext`, не как Singleton.
- `DatabaseInitializer` вызывает `Initialize()` один раз после построения хоста.
- Контракты хранилищ лежат в `Core`/`Services`, реализации — здесь.

## Конфигурация сущностей

Для нескольких сущностей выноси конфигурации в отдельные классы:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PnvApp.Models;

namespace PnvApp.Infrastructure.Configurations;

public sealed class GreetingRecordConfiguration : IEntityTypeConfiguration<GreetingRecord>
{
    public void Configure(EntityTypeBuilder<GreetingRecord> builder)
    {
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Text).IsRequired().HasMaxLength(256);
    }
}
```

Подключение:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
    => modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
```

## Работа с файлами

- Базовый каталог — `AppContext.BaseDirectory` или путь из опций.
- Пути — через `Path.Join`.
- Каталоги создавай до записи.
- Асинхронные операции — с `CancellationToken`.

```csharp
public sealed class FileSettingsStorage(IOptions<AppOptions> options)
{
    private readonly string _path = Path.Join(options.Value.DataDirectory, "settings.json");

    public async Task SaveAsync(string content, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
        await File.WriteAllTextAsync(_path, content, cancellationToken);
    }
}
```

## Чего не делать

```csharp
// Плохо: инфраструктура знает про UI
public sealed class GreetingStorage
{
    public void ShowError() => new ErrorWindow().Show();
}
```

```csharp
// Плохо: инфраструктура в Singleton и держит DbContext
services.AddSingleton<GreetingStorage>();
public sealed class GreetingStorage(AppDbContext dbContext) { }
```

```csharp
// Плохо: логирование через Console
Console.WriteLine($"SQL: {command.CommandText}");
```

## Чек-лист

- [ ] Все типы в `Infrastructure` или `Infrastructure/<подпапка>`.
- [ ] Нет ссылок на View, ViewModel, Avalonia.
- [ ] `DbContext` не захвачен Singleton-сервисом.
- [ ] Регистрация — только в `Bootstrapper`.
- [ ] Пути через `Path.Join`, логи через `ILogger<T>`.
