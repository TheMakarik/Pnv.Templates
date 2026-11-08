# ПНВ: шаблон десктопного приложения — спецификация

> Статус: активная разработка. Скоуп — **только десктопный шаблон** (`pnv-desktop`).
> Веб-сервис и комбинированный шаблон в текущий скоуп не входят.

## 1. Цель

Сделать NuGet-пакет шаблонов `.NET` (`Pnv.Templates`), который после
`dotnet new install` доступен в `dotnet new list` и в IDE (Rider/Visual Studio)
под русским именем **«ПНВ: ДесктопПриложение»**.

Шаблон создаёт полностью рабочее Avalonia-приложение под `.NET 10` / `C# 14`
с набором опций, перечисленных ниже. Сгенерированный код и скиллы должны
соответствовать стилю репозиториев `TheMakarik/zaggy-code` и
`TheMakarik/ai-agent-1c-helper`.

## 2. Жёсткие требования

- `.NET` всегда `net10.0`, C# 14.
- UI всегда AvaloniaUI, версия выбирается из списка 12.x.
- Имя шаблона в IDE/CLI: `ПНВ: ДесктопПриложение`.
- `shortName`: `pnv-desktop`.
- Исходные проекты — в папке `Source`, тесты — в `Tests`.
- В solution должны быть solution-папки `Source` и `Tests`.
- Папка скиллов в сгенерированном проекте называется строго `Skills` (заглавная).
- Скиллы должны работать и в OpenCode, и в Claude Code.
- Код пишется в стиле zaggy-code: file-scoped namespaces, primary constructors,
  `var` где очевидно, `is null` / `is not null`, `sealed` по умолчанию,
  один публичный тип на файл, без вложенных классов, без XML-документации,
  структурированное логирование, `Path.Join` вместо `Path.Combine`.

## 3. Опции шаблона

Порядок опций в IDE — как в таблице.

| Символ | CLI | Тип | Значения | По умолчанию |
|---|---|---|---|---|
| `Di` | `--di` | choice | `None`, `Hosting`, `HostingScrutor` | `Hosting` |
| `Mvvm` | `--mvvm` | choice | `CommunityToolkit`, `ReactiveUI` | `CommunityToolkit` |
| `Tests` | `--tests` | bool | true/false | `true` |
| `Skills` | `--skills` | bool | true/false | `true` |
| `Logging` | `--logging` | bool | true/false | `true` |
| `EfCore` | `--ef` | bool | true/false | `false` |
| `SplitProjects` | `--split` | bool | true/false | `false` |
| `AvaloniaVersion` | `--avalonia-version` | choice | см. ниже | `12.1.5` |
| `SkipRestore` | `--no-restore` | bool | true/false | `false` |

Версии Avalonia (только те, для которых существуют все пакеты
`Avalonia`, `Avalonia.Desktop`, `Avalonia.Themes.Fluent`, `Avalonia.Fonts.Inter`):

`12.0.4`, `12.0.5`, `12.1.0`, `12.1.1`, `12.1.2`, `12.1.3`, `12.1.4`, `12.1.5`.

Вычисляемые символы (использовать в условиях `#if`):

- `UseDi` = `(Di != "None")`
- `UseScrutor` = `(Di == "HostingScrutor")`
- `UseCommunityToolkit` = `(Mvvm == "CommunityToolkit")`
- `UseReactiveUI` = `(Mvvm == "ReactiveUI")`
- `UseLogging` = `(Logging && Di != "None")`
- `UseEfCore` = `(EfCore && Di != "None")`

> EF Core и логирование требуют DI: при `Di=None` инфраструктура EF и Serilog
> не создаются, даже если пользователь поставил галочки. Это поведение нужно
> описать в README шаблона.

## 4. Генерируемая структура

### 4.1. Один проект (`SplitProjects=false`)

```text
<Name>.slnx
Directory.Build.props
.gitignore
README.md
AGENTS.md                     # только если Skills=true
CLAUDE.md                     # только если Skills=true
opencode.json                 # только если Skills=true
Source/
  <Name>/
    <Name>.csproj
    Program.cs
    App.axaml
    App.axaml.cs
    Bootstrapper.cs           # только если UseDi
    appsettings.json          # только если UseDi
    app.manifest
    Assets/app.ico
    View/
      MainWindow.axaml
      MainWindow.axaml.cs
    ViewModels/
      ViewModelBase.cs
      MainViewModel.cs
    Models/
      Greeting.cs
      GreetingRecord.cs       # только если UseEfCore
    Services/
      IGreetingService.cs
      GreetingService.cs
    Options/                  # только если UseDi
      AppOptions.cs
      DatabaseOptions.cs      # только если UseEfCore
    Infrastructure/           # только если UseEfCore
      AppDbContext.cs
      DatabaseInitializer.cs
      Interceptors/SqlLoggingInterceptor.cs
Tests/                        # только если Tests=true
  <Name>.Tests/
    <Name>.Tests.csproj
    Services/GreetingServiceTests.cs
Skills/                       # только если Skills=true
.claude/skills/               # только если Skills=true (зеркало Skills)
```

### 4.2. Несколько проектов (`SplitProjects=true`)

3 проекта без EF, 4 — с EF:

```text
Source/
  <Name>/                     # UI: App, Program, View, ViewModels, Bootstrapper
  <Name>.Core/                # Models, Services-контракты, Options
  <Name>.Modules/             # реализации сервисов
  <Name>.Infrastructure/      # только если UseEfCore
Tests/
  <Name>.Tests/
```

Пространства имён **не меняются** между режимами: везде `PnvApp.*`
(после замены `sourceName` — `<Name>.*`). В проектах `.Core`, `.Modules`,
`.Infrastructure` выставить `<RootNamespace>PnvApp</RootNamespace>`,
чтобы папки/проекты не влияли на namespace.

### 4.3. Solution

Формат `.slnx`, solution-папки `Source` и `Tests`:

```xml
<Solution>
  <Folder Name="/Source/">
    <Project Path="Source/PnvApp/PnvApp.csproj" />
    <!-- при SplitProjects -->
    <Project Path="Source/PnvApp.Core/PnvApp.Core.csproj" />
    <Project Path="Source/PnvApp.Modules/PnvApp.Modules.csproj" />
    <!-- при UseEfCore -->
    <Project Path="Source/PnvApp.Infrastructure/PnvApp.Infrastructure.csproj" />
  </Folder>
  <!-- при Tests -->
  <Folder Name="/Tests/">
    <Project Path="Tests/PnvApp.Tests/PnvApp.Tests.csproj" />
  </Folder>
</Solution>
```

## 5. Техническая архитектура шаблона

### 5.1. Раскладка репозитория

```text
pnv-templates/
  SPEC.md
  README.md
  template-src/
    desktop/
      .template.config/
        template.json
        ide.host.json
        dotnetcli.host.json
        icon.png
    desktop-content/          # сюда смотрят sources из template.json
      root/                   # .slnx, Directory.Build.props, README, .gitignore
      app/                    # Program, App.axaml(.cs), app.manifest, Assets
      views/                  # MainWindow.axaml(.cs)
      viewmodels-communitytoolkit/
      viewmodels-reactiveui/
      single-project/         # <Name>.csproj для одного проекта
      split-ui/               # <Name>.csproj для UI в split-режиме
      split-core/             # <Name>.Core.csproj
      split-modules/          # <Name>.Modules.csproj
      split-infrastructure/   # <Name>.Infrastructure.csproj
      service-contracts/      # IGreetingService (single: Source/<Name>/Services, split: Source/<Name>.Core/Services)
      service-implementations/# GreetingService (single: Source/<Name>/Services, split: Source/<Name>.Modules/Services)
      models/                 # Greeting (single: Source/<Name>/Models, split: Source/<Name>.Core/Models)
      models-ef/              # GreetingRecord (только UseEfCore)
      infrastructure/         # AppDbContext, DatabaseInitializer, Interceptors
      options/                # AppOptions, DatabaseOptions
      appsettings-basic/      # appsettings.json без EF
      appsettings-ef/         # appsettings.json с секцией DatabaseOptions
      di-hosting/             # Bootstrapper.cs
      tests/                  # <Name>.Tests.csproj + тесты
      skills-meta/            # AGENTS.md, CLAUDE.md, opencode.json
      skills/                 # сгенерированные скиллы (создаёт build/pack.py)
      claude-skills/          # зеркало скиллов для Claude (создаёт build/pack.py)
  template-src/skills/        # канонические скиллы, пишет отдельный агент
  build/pack.py               # стейджинг + упаковка nupkg
  build/verify.py             # матрица генераций + сборка
  artifacts/                  # nupkg и сгенерированные проекты
```

**Важно:** в корне `template-src/desktop` НЕ должно быть файлов контента —
только `.template.config`. Контент лежит рядом, в `template-src/desktop-content`,
и подключается через `sources` с путями `../desktop-content/...`. Это проверено:
при таком подходе движок не дублирует файлы.

### 5.2. `sources` в `template.json`

Каждый каталог контента подключается отдельной записью с `target`:

```json
{ "source": "../desktop-content/app/", "target": "Source/PnvApp/", "include": ["**/*"] }
```

Замена `sourceName` (`PnvApp` → имя проекта) работает и в `target`:
`target: "Source/PnvApp.Core/"` превращается в `Source/Demo.Core/`.

Для бинарных файлов (`Assets/**`) обязательно `"copyOnly": ["**/*"]`,
чтобы движок не портил `.ico`.

Для скиллов обязательно `"copyOnly": ["**/*"]`, чтобы не обрабатывать
`#if` и примеры кода внутри `SKILL.md`.

Условные записи:

```json
{ "source": "...", "target": "...", "condition": "(UseEfCore)", "include": ["**/*"] }
```

Один и тот же физический файл можно копировать в разные target разными
записями с взаимоисключающими условиями (`(!SplitProjects)` / `(SplitProjects)`).

### 5.3. Условная обработка внутри файлов

- В C# использовать `#if (SYMBOL)` / `#else` / `#endif`
  (обязательно со скобками — это синтаксис шаблонизатора).
- В XML (`.csproj`, `.slnx`, `.props`) использовать
  `<!--#if (SYMBOL) -->` / `<!--#endif -->`: обычный `#if` делает XML
  невалидным и не обрабатывается движком.
- В JSON (`appsettings.json`) не использовать `#if`: вместо этого держать
  отдельные варианты файла и подключать их условиями.
- В сгенерированных файлах не должно оставаться шаблонных директив.
- Набор скиллов фильтруется по опциям: скиллы по DI, MVVM, EF Core и тестам
  подключаются только при выборе соответствующей технологии.

### 5.4. Упаковка

`build/pack.py`:

1. Очищает `build/stage`.
2. Копирует `template-src/desktop` → `build/stage/content/desktop`.
3. Копирует `template-src/desktop-content` → `build/stage/content/desktop-content`.
4. Копирует канонические скиллы `template-src/skills` в
   `build/stage/content/desktop-content/skills` и
   `build/stage/content/desktop-content/claude-skills`.
5. Создаёт `build/stage/Pnv.Templates.nuspec` с `<packageType name="Template" />`.
6. Пакует zip в `artifacts/Pnv.Templates.1.0.0.nupkg`:
   `.nuspec` в корне + `content/**`.
7. Опционально ставит пакет: `dotnet new uninstall Pnv.Templates`;
   `dotnet new install artifacts/Pnv.Templates.1.0.0.nupkg`.

Минимальный nupkg без `[Content_Types].xml` уже проверен — `dotnet new install`
его принимает.

## 6. Содержимое шаблона

### 6.1. Пакеты

Версии пиновать точно.

| Пакет | Версия | Когда |
|---|---|---|
| `Avalonia` | `AvaloniaVersionToken` | всегда |
| `Avalonia.Desktop` | `AvaloniaVersionToken` | всегда |
| `Avalonia.Themes.Fluent` | `AvaloniaVersionToken` | всегда |
| `Avalonia.Fonts.Inter` | `AvaloniaVersionToken` | всегда |
| `AvaloniaUI.DiagnosticsSupport` | `2.2.3` | всегда |
| `CommunityToolkit.Mvvm` | `8.4.2` | `UseCommunityToolkit` |
| `ReactiveUI.Avalonia` | `12.0.3` | `UseReactiveUI` |
| `ReactiveUI.SourceGenerators` | `3.2.0` | `UseReactiveUI` |
| `Microsoft.Extensions.Hosting` | `10.0.0` | `UseDi` |
| `Serilog` | `4.3.1` | `UseLogging` |
| `Serilog.Extensions.Logging` | `10.0.0` | `UseLogging` |
| `Serilog.Settings.Configuration` | `10.0.1` | `UseLogging` |
| `Serilog.Sinks.Console` | `6.1.1` | `UseLogging` |
| `Scrutor` | `7.0.0` | `UseScrutor` |
| `Microsoft.EntityFrameworkCore.Sqlite` | `10.0.0` | `UseEfCore` |
| `Microsoft.Extensions.Logging.Abstractions` | `10.0.0` | split + `UseEfCore` (Infrastructure) |
| `AutoFixture` | `4.18.1` | `Tests` |
| `coverlet.collector` | `6.0.4` | `Tests` |
| `FakeItEasy` | `9.0.1` | `Tests` |
| `FluentAssertions` | `8.11.0` | `Tests` |
| `Microsoft.NET.Test.Sdk` | `17.14.1` | `Tests` |
| `xunit` | `2.9.3` | `Tests` |
| `xunit.runner.visualstudio` | `3.1.4` | `Tests` |

`AvaloniaVersionToken` заменяется символом `AvaloniaVersion`
(`"replaces": "AvaloniaVersionToken"`).

### 6.2. `Directory.Build.props` (в корне сгенерированного решения)

```xml
<Project>
  <PropertyGroup>
    <LangVersion>14.0</LangVersion>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <NuGetAudit>false</NuGetAudit>
    <NoWarn>$(NoWarn);NU1900</NoWarn>
  </PropertyGroup>
</Project>
```

### 6.3. Точка входа

`Program.cs`:

```csharp
using Avalonia;
#if (UseReactiveUI)
using ReactiveUI.Avalonia;
#endif

namespace PnvApp;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
        => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if (UseReactiveUI)
            .UseReactiveUI()
#endif
            .WithInterFont()
            .LogToTrace();
}
```

`App.axaml` — стандартный `Application` с `FluentTheme`,
`RequestedThemeVariant="Default"`, `x:Class="PnvApp.App"`.

`App.axaml.cs`:

- `Initialize()` → `AvaloniaXamlLoader.Load(this)`.
- `OnFrameworkInitializationCompleted()`:
  - проверка `IClassicDesktopStyleApplicationLifetime`;
  - при `UseDi`: `var host = new Bootstrapper().BuildHost();`
    `DataContext = host.Services.GetRequiredService<MainViewModel>()`;
  - без DI: `DataContext = new MainViewModel(new GreetingService())`.
- `#if` для using-ов и тела.

### 6.4. DI

`Bootstrapper.cs` (только `UseDi`):

- `Host.CreateApplicationBuilder(new HostApplicationBuilderSettings { Args = Environment.GetCommandLineArgs(), ContentRootPath = AppContext.BaseDirectory })`.
- `builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)`.
- Serilog: `new LoggerConfiguration().ReadFrom.Configuration(builder.Configuration).CreateLogger()`,
  `builder.Logging.ClearProviders().AddSerilog(Log.Logger, dispose: true)`.
- `RegisterOptions`:
  - `AddOptions<AppOptions>().Bind(...)`.
  - при `UseEfCore` — `AddOptions<DatabaseOptions>().Bind(...)`.
- `RegisterServices`:
  - `UseScrutor`: `services.Scan(...)` по `FromAssembliesOf(typeof(Bootstrapper), typeof(GreetingService))`:
    ViewModel-ы (`AssignableTo<ViewModelBase>`) → `AsSelf().WithSingletonLifetime()`;
    классы с именем на `Service` → `AsImplementedInterfaces().WithSingletonLifetime()`.
  - без Scrutor: `AddSingleton<IGreetingService, GreetingService>()`,
    `AddSingleton<MainViewModel>()`.
  - при `UseEfCore`: `AddDbContext<AppDbContext>((provider, options) => ...)`
    с `UseSqlite(connectionString)` и
    `AddInterceptors(new SqlLoggingInterceptor(provider.GetRequiredService<ILogger<SqlLoggingInterceptor>>()))`;
    `AddSingleton<DatabaseInitializer>()`.
- `BuildHost()` после `builder.Build()` при `UseEfCore` вызывает
  `host.Services.GetRequiredService<DatabaseInitializer>().Initialize()`.

`appsettings.json` (только `UseDi`): секции `AppOptions`, `Serilog`;
при `UseEfCore` — ещё `DatabaseOptions` с пустым `ConnectionString`.
Провайдер `Serilog.Sinks.Console`, минимальный уровень `Information`.

### 6.5. MVVM

**CommunityToolkit** (`viewmodels-communitytoolkit/`):

```csharp
using CommunityToolkit.Mvvm.ComponentModel;

namespace PnvApp.ViewModels;

public abstract class ViewModelBase : ObservableObject;
```

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PnvApp.Services;

namespace PnvApp.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly IGreetingService _greetingService;

    [ObservableProperty]
    public partial string Greeting { get; set; } = "Привет!";

    public MainViewModel(IGreetingService greetingService)
    {
        _greetingService = greetingService;
    }

    [RelayCommand]
    private void SayHello()
        => Greeting = _greetingService.GetGreeting("мир").Text;
}
```

**ReactiveUI** (`viewmodels-reactiveui/`):

```csharp
using System.Reactive;
using ReactiveUI;

namespace PnvApp.ViewModels;

public abstract class ViewModelBase : ReactiveObject
{
    public Interaction<string, Unit> ShowMessage { get; } = new();
}
```

```csharp
using ReactiveUI.SourceGenerators;
using PnvApp.Services;

namespace PnvApp.ViewModels;

public sealed partial class MainViewModel : ViewModelBase
{
    private readonly IGreetingService _greetingService;

    [Reactive]
    private string _greeting = "Привет!";

    public MainViewModel(IGreetingService greetingService)
    {
        _greetingService = greetingService;
    }

    [ReactiveCommand]
    private void SayHello()
        => Greeting = _greetingService.GetGreeting("мир").Text;
}
```

`MainViewModel` не зависит от View. Для RxUI скиллы описывают
`Interaction` и `[Reactive]`/`[ReactiveCommand]`.

### 6.6. View

`View/MainWindow.axaml`:

- `x:Class="PnvApp.View.MainWindow"`,
  `x:DataType="vm:MainViewModel"`, `xmlns:vm="using:PnvApp.ViewModels"`.
- Заголовок `<Name>`, размеры 800×450, `WindowStartupLocation="CenterScreen"`.
- `TextBlock Text="{Binding Greeting}"`,
  `Button Content="Поздороваться" Command="{Binding SayHelloCommand}"`.

`MainWindow.axaml.cs` — `sealed partial class MainWindow : Window`,
конструктор `InitializeComponent()`.

### 6.7. Сервисы и модели

```csharp
namespace PnvApp.Models;

public sealed record Greeting(string Text);
```

```csharp
using PnvApp.Models;

namespace PnvApp.Services;

public interface IGreetingService
{
    Greeting GetGreeting(string name);
}
```

```csharp
using PnvApp.Models;

namespace PnvApp.Services;

public sealed class GreetingService : IGreetingService
{
    public Greeting GetGreeting(string name) => new($"Привет, {name}!");
}
```

### 6.8. EF Core

`Models/GreetingRecord.cs`:

```csharp
namespace PnvApp.Models;

public sealed class GreetingRecord
{
    public int Id { get; set; }
    public string Text { get; set; } = string.Empty;
}
```

`Infrastructure/AppDbContext.cs`:

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

`Infrastructure/Interceptors/SqlLoggingInterceptor.cs` — `DbCommandInterceptor`
с `ILogger<SqlLoggingInterceptor>`, логирует `command.CommandText`
в `ReaderExecuting` и `ReaderExecutingAsync` (structured logging,
шаблон `"SQL: {CommandText}"`).

`Infrastructure/DatabaseInitializer.cs`:

```csharp
namespace PnvApp.Infrastructure;

public sealed class DatabaseInitializer(AppDbContext dbContext)
{
    public void Initialize() => dbContext.Database.EnsureCreated();
}
```

`Options/DatabaseOptions.cs` — `ConnectionString` (string, пустая строка).
При пустой строке использовать `Data Source={Path.Join(AppContext.BaseDirectory, "app.db")}`.

### 6.9. Тесты

`<Name>.Tests.csproj` ссылается на `../../Source/PnvApp/PnvApp.csproj`
(в split-режиме этого достаточно: ProjectReference транзитивны).

`Services/GreetingServiceTests.cs`:

```csharp
using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using PnvApp.Models;
using PnvApp.Services;
using PnvApp.ViewModels;
using Xunit;

namespace PnvApp.Tests.Services;

public sealed class GreetingServiceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GreetingService systemUnderTests = new();

    [Fact]
    public void GetGreeting_ReturnsGreetingWithName()
    {
        var name = _fixture.Create<string>();

        var greeting = systemUnderTests.GetGreeting(name);

        greeting.Text.Should().Contain(name);
    }

    [Fact]
    public void MainViewModel_SayHello_UsesGreetingService()
    {
        var greetingService = A.Fake<IGreetingService>();
        A.CallTo(() => greetingService.GetGreeting(A<string>._))
            .Returns(new Greeting("тест"));
        var viewModel = new MainViewModel(greetingService);

        viewModel.SayHelloCommand.Execute(null);

        viewModel.Greeting.Should().Be("тест");
    }
}
```

`viewModel.SayHelloCommand` есть в обоих MVVM-вариантах (генерируется
`[RelayCommand]` / `[ReactiveCommand]`).

### 6.10. README, .gitignore

`README.md` — краткое русское описание, структура, команды:

```bash
dotnet run --project Source/<Name>
dotnet test
```

Упомянуть: `EfCore` доступен только при включённом DI; `Skills` лежат в `Skills/`.

`.gitignore` — стандартный для .NET + Rider (`.idea/`, `bin/`, `obj/`,
`*.user`, `artifacts/`, `*.db`).

## 7. Скиллы

### 7.1. Формат

Каждый скилл — `template-src/skills/<id>/SKILL.md`:

```markdown
---
name: <id>
description: <когда применять, по-русски, 1–2 предложения>
---

# <Заголовок>

...
```

Требования:

- `name` совпадает с именем папки, lower-kebab-case.
- `description` — по-русски, объясняет, **когда** применять.
- Никаких ссылок на конкретные репозитории (`ZaggyCode`, `Ai1CAgent`) —
  только на структуру шаблона (`Source/<Name>`, `Skills`, `View`, `Models`,
  `Services`, `Infrastructure`).
- Никаких файлов с `#::NO_AI` / `//#:NO_AI`.
- Никакого macOS-специфичного контента.
- Код в примерах — в стиле проекта.

### 7.2. Состав (минимум)

Всегда:

- `code-style`
- `collections`
- `async`
- `error-handling`
- `paths`
- `clarifying-questions`
- `architecture`
- `code-sections`
- `models-organization`
- `services-organization`
- `view-organization`
- `xaml`
- `avalonia-converters`
- `avalonia-theming`
- `avalonia-colors`
- `app-settings`
- `trimming`
- `json-sourcegen`

По опциям:

- `dependency-injection` — `UseDi && !UseScrutor`
- `dependency-injection-scrutor` — `UseScrutor`
- `options` — `UseDi`
- `logging` — `UseLogging`
- `mvvm-communitytoolkit` — `UseCommunityToolkit`
- `mvvm-reactiveui` — `UseReactiveUI`
- `reactiveui-interactions` — `UseReactiveUI`
- `reactiveui-sourcegenerators` — `UseReactiveUI`
- `ef-core-sqlite` — `UseEfCore`
- `ef-core-interceptors` — `UseEfCore`
- `infrastructure-organization` — `UseEfCore`
- `testing` — `Tests`

Допускается объединять/переименовывать, но условия в `pack.py`/`template.json`
должны соответствовать фактическим папкам.

### 7.3. Конфигурация для OpenCode и Claude Code

В корне сгенерированного проекта (только при `Skills=true`):

`opencode.json`:

```json
{
  "$schema": "https://opencode.ai/config.json",
  "skills": ["Skills"]
}
```

`AGENTS.md` и `CLAUDE.md`:

- сказать, что проектные скиллы лежат в `Skills/<id>/SKILL.md`;
- открывать и читать релевантный `SKILL.md` целиком перед задачей;
- не перечислять все скиллы вручную (список меняется от опций).

Claude Code не умеет указывать произвольную папку со скиллами через настройки,
поэтому в сгенерированном проекте создаётся зеркало
`.claude/skills/<id>/SKILL.md` с тем же содержимым, что и `Skills/<id>/SKILL.md`.
Это делает `pack.py` (копирует каталог), а `template.json` подключает его
отдельной `sources`-записью с `condition: "(Skills)"` и тем же набором
`modifiers`/`exclude`, что и `Skills`.

## 8. Definition of Done

- `build/pack.py` собирает `artifacts/Pnv.Templates.1.0.0.nupkg`.
- `dotnet new install artifacts/Pnv.Templates.1.0.0.nupkg` проходит.
- `dotnet new list` показывает `ПНВ: ДесктопПриложение` / `pnv-desktop`.
- `dotnet new pnv-desktop --help` показывает все опции с русскими описаниями.
- Все валидные комбинации опций генерируются без ошибок.
- Минимум: все комбинации `Di × Mvvm × SplitProjects` с `Tests=true`,
  `Skills=true`, `EfCore` (где допустимо) собираются `dotnet build` без ошибок
  и предупреждений (из-за `TreatWarningsAsErrors`).
- Каждая версия Avalonia из списка собирается хотя бы в одной комбинации.
- `dotnet test` проходит для варианта по умолчанию и для split+EF.
- В сгенерированных проектах нет файлов `bin/`, `obj/`, `*.nupkg`.
- В `Skills` нет macOS-специфичных и `#::NO_AI` файлов.
- README репозитория описывает установку и использование.
