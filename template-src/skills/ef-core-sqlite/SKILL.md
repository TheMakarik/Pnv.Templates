---
name: ef-core-sqlite
description: Работа с EF Core и SQLite в десктопном приложении: DbContext, DbSet, OnModelCreating, connection string из IOptions, EnsureCreated и миграции. Используй при добавлении или изменении базы данных.
---

# EF Core + SQLite

## Где лежит

- Один проект: `Source/<Name>/Infrastructure` и `Models`.
- Split: `Source/<Name>.Infrastructure` и `Source/<Name>.Core/Models`.
- Неймспейсы — `<Name>.Infrastructure` и `<Name>.Models`.

## Сущность

```csharp
namespace PnvApp.Models;

public sealed class GreetingRecord
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}
```

- Сущность — `sealed class` с инициализаторами.
- Навигационные свойства — `virtual` только если нужен lazy loading
  (в шаблоне он не включён).
- Не добавляй UI-атрибуты и логику в сущность.

## DbContext

```csharp
using Microsoft.EntityFrameworkCore;
using PnvApp.Models;

namespace PnvApp.Infrastructure;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<GreetingRecord> Greetings => Set<GreetingRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GreetingRecord>(entity =>
        {
            entity.HasKey(record => record.Id);
            entity.Property(record => record.Text).IsRequired().HasMaxLength(256);
        });
    }
}
```

- `sealed`, primary constructor, `DbContextOptions<T>` в конструкторе.
- `DbSet` — выражение-свойство, без сеттера.
- Конфигурация — в `OnModelCreating`, а не атрибутами на сущности.
- Для большого числа сущностей выноси конфигурации в
  `IEntityTypeConfiguration<T>` и подключай через `ApplyConfigurationsFromAssembly`.

## Connection string

Опции базы:

```csharp
namespace PnvApp.Options;

public sealed class DatabaseOptions
{
    public string ConnectionString { get; set; } = string.Empty;
}
```

Регистрация:

```csharp
services.AddDbContext<AppDbContext>((provider, options) =>
{
    var databaseOptions = provider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    var connectionString = string.IsNullOrWhiteSpace(databaseOptions.ConnectionString)
        ? $"Data Source={Path.Join(AppContext.BaseDirectory, "app.db")}"
        : databaseOptions.ConnectionString;

    options.UseSqlite(connectionString);
});
```

- Пустая строка в настройках — значение по умолчанию.
- Путь только через `Path.Join`.
- Не храни абсолютный путь в репозитории.

## Инициализация

```csharp
namespace PnvApp.Infrastructure;

public sealed class DatabaseInitializer(AppDbContext dbContext)
{
    public void Initialize() => dbContext.Database.EnsureCreated();
}
```

`EnsureCreated()` подходит для шаблона и простых приложений: создаёт схему,
если базы нет. Он не обновляет существующую схему.

Для развития схемы переходи на миграции:

```bash
dotnet ef migrations add InitialCreate \
    --project Source/PnvApp.Infrastructure \
    --startup-project Source/PnvApp
```

```csharp
public void Initialize() => dbContext.Database.Migrate();
```

- Не смешивай `EnsureCreated` и `Migrate`.
- Не вызывай инициализацию из конструктора `DbContext`.
- Инициализация выполняется один раз при старте, в `Bootstrapper`.

## Доступ к данным

- Запросы — в сервисах или репозиториях, не в ViewModel и не в View.
- `DbContext` не живёт дольше операции: не храни его в поле Singleton-сервиса.
- Асинхронные методы принимают `CancellationToken`.
- Не возвращай `IQueryable<T>` наружу: материализуй в список.

```csharp
public sealed class GreetingStorage(AppDbContext dbContext)
{
    public async Task<IReadOnlyList<GreetingRecord>> LoadAsync(CancellationToken cancellationToken)
        => await dbContext.Greetings
            .AsNoTracking()
            .OrderBy(record => record.Id)
            .ToListAsync(cancellationToken);
}
```

## Чего не делать

```csharp
// Плохо: DbContext в Singleton-сервисе
public sealed class GreetingService(AppDbContext dbContext) { }
services.AddSingleton<GreetingService>();
```

```csharp
// Плохо: логика БД во ViewModel
[RelayCommand]
private async Task LoadAsync()
{
    using var dbContext = new AppDbContext();
    Greetings = await dbContext.Greetings.ToListAsync();
}
```

## Чек-лист

- [ ] `DbContext` — `sealed`, с `DbContextOptions<T>`.
- [ ] Конфигурация в `OnModelCreating`.
- [ ] Connection string из `IOptions<DatabaseOptions>`.
- [ ] Путь к файлу через `Path.Join`.
- [ ] `DbContext` не захвачен Singleton-сервисом.
- [ ] EF используется только при включённом DI.
