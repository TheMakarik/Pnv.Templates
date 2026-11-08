---
name: dependency-injection-scrutor
description: Регистрация сервисов и ViewModel через Scrutor: сканирование сборок, AssignableTo, имена *Service, WithSingletonLifetime. Используй, когда в шаблоне выбран вариант DI с Scrutor.
---

# DI со Scrutor

## Когда применяется

Только при `Di=HostingScrutor`. При обычном `Hosting` используется ручная
регистрация (скилл `dependency-injection`).

Scrutor не заменяет DI-контейнер, а добавляет сканирование сборок поверх
`Microsoft.Extensions.DependencyInjection`.

## Где регистрировать

Всё — в `Bootstrapper`, в методе `RegisterServices`:

```csharp
private static void RegisterServices(IServiceCollection services)
{
    services.Scan(scan => scan
        .FromAssembliesOf(typeof(Bootstrapper), typeof(GreetingService))
        .AddClasses(classes => classes.AssignableTo<ViewModelBase>())
        .AsSelf()
        .WithSingletonLifetime()
        .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal)))
        .AsImplementedInterfaces()
        .WithSingletonLifetime());
}
```

## Что попадает в сканирование

- ViewModel-ы: всё, что наследует `ViewModelBase`.
- Сервисы: классы, имя которых заканчивается на `Service`.
- Только сборки, переданные в `FromAssembliesOf`. Не сканируй всё приложение
  и не подключай `FromApplicationDependencies()` без необходимости.

## AsSelf и AsImplementedInterfaces

| Тип | Регистрация | Почему |
|---|---|---|
| ViewModel | `AsSelf()` | зависит от конкретного класса, интерфейса нет |
| Сервис | `AsImplementedInterfaces()` | потребители зависят от `I<Name>Service` |

```csharp
// Хорошо: сервис доступен как IGreetingService
.AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service", StringComparison.Ordinal)))
.AsImplementedInterfaces()

// Хорошо: ViewModel доступна как MainViewModel
.AddClasses(classes => classes.AssignableTo<ViewModelBase>())
.AsSelf()
```

Не регистрируй один и тот же тип дважды: `AsSelf().AsImplementedInterfaces()`
нужен только если потребители используют и класс, и интерфейс.

## Lifetime

- `WithSingletonLifetime()` — сервисы без состояния и ViewModel главного окна.
- `WithTransientLifetime()` — объекты с коротким жизненным циклом.
- `WithScopedLifetime()` — в десктопе не используется.

Не смешивай: Singleton, зависящий от Transient, захватывает Transient навсегда.

## Подводные камни

- Сканирование по имени `*Service` захватит и `DatabaseInitializerService`,
  если такой появится. Имя должно отражать роль, а не подгоняться под фильтр.
- Абстракция без реализации не регистрируется — приложение упадёт при
  попытке получить её из контейнера.
- `AsImplementedInterfaces()` без интерфейса не зарегистрирует тип.
- Порядок вызовов `AddClasses` важен: каждый фильтр — отдельная группа.
- Сканирование не должно захватывать `Program`, `App`, `Bootstrapper`.

## Когда лучше ручная регистрация

- Мало типов (3–5): явные `AddSingleton` читаются лучше.
- Нужны разные lifetime для разных реализаций.
- Нужны фабрики и условная регистрация.

## Чек-лист

- [ ] `FromAssembliesOf` перечисляет только нужные сборки.
- [ ] ViewModel-ы регистрируются `AsSelf()`.
- [ ] Сервисы регистрируются `AsImplementedInterfaces()`.
- [ ] Lifetime осознан: Singleton по умолчанию.
- [ ] Сканирование не захватывает точки входа и инфраструктуру.
