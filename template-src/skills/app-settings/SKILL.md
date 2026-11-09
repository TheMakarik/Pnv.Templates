---
name: app-settings
description: Хранение настроек приложения: appsettings.json, секции, переменные окружения, пути через Path.Join, поведение при отсутствии DI. Используй при добавлении новых настроек или путей к файлам.
---

# Настройки приложения

## Два режима

| Режим | Где настройки | Как читаются |
|---|---|---|
| DI включён | `appsettings.json` рядом с приложением | `IOptions<T>` |
| DI выключен | константы/параметры конструктора | напрямую |

При включённом DI единственный источник настроек — `appsettings.json`.
Не дублируй значения константами в коде.

## appsettings.json

Файл лежит в корне проекта приложения и копируется в выходной каталог:

```xml
<ItemGroup>
  <None Update="appsettings.json">
    <CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>
  </None>
</ItemGroup>
```

Структура: одна секция на класс опций.

```json
{
  "AppOptions": {
    "Title": "Pnv.Desktop",
    "Width": 800,
    "Height": 450,
    "DataDirectory": "%APPDATA%/Pnv.Desktop"
  },
  "DatabaseOptions": {
    "ConnectionString": ""
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    }
  }
}
```

- Ключи совпадают с именами свойств.
- Никаких комментариев — JSON их не поддерживает.
- Не используй `#if`: для разных вариантов держи разные файлы и подключай
  их условиями шаблона.

## Переменные окружения

Пути можно задавать через переменные окружения. Раскрытие — в сеттере опций,
чтобы остальной код всегда видел готовый путь:

```csharp
public sealed class AppOptions
{
    private string _dataDirectory = string.Empty;

    public string DataDirectory
    {
        get => _dataDirectory;
        set => _dataDirectory = Environment.ExpandEnvironmentVariables(value);
    }
}
```

Пустая строка — сигнал «использовать значение по умолчанию»:

```csharp
public string ResolveDataDirectory()
{
    if (!string.IsNullOrWhiteSpace(_dataDirectory))
        return _dataDirectory;

    return Path.Join(AppContext.BaseDirectory, "data");
}
```

## Пути

Только `Path.Join`, не `Path.Combine`.

```csharp
// Хорошо
var databasePath = Path.Join(AppContext.BaseDirectory, "app.db");
var configPath = Path.Join(dataDirectory, "settings.json");

// Плохо
var databasePath = Path.Combine(AppContext.BaseDirectory, "app.db");
```

- Относительные пути в `appsettings.json` считай от `AppContext.BaseDirectory`,
  а не от текущего каталога процесса.
- Каталоги создавай до записи: `Directory.CreateDirectory(directory)`.
- Не собирай пути конкатенацией строк.

## Секреты

- Пароли, токены и ключи не храни в `appsettings.json` в репозитории.
- Для локальной разработки используй переменные окружения или user secrets.
- Не логируй значения секретных настроек, даже частично.

## Без DI

Если DI выключен, настройки передаются явно:

```csharp
var options = new AppOptions { Title = "Pnv.Desktop" };
var viewModel = new MainViewModel(new GreetingService(options));
```

Не создавай глобальный статический конфиг и не читай файл из ViewModel.

## Чек-лист

- [ ] Новая настройка добавлена в класс опций и в `appsettings.json`.
- [ ] Секция совпадает с именем класса.
- [ ] Пути собираются через `Path.Join`.
- [ ] Переменные окружения раскрыты до использования.
- [ ] Секреты не попали в файл и в логи.
