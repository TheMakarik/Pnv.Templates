using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Pnv.Combo.Desktop.View;
using Pnv.Combo.Desktop.ViewModels;
#if (UseDi)
using Microsoft.Extensions.DependencyInjection;
#endif
#if (ApiClient == "None")
using Pnv.Combo.Desktop.Services;
#else
using Pnv.Combo.Desktop.Api;
#endif

namespace Pnv.Combo.Desktop;

public sealed partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
#if (UseDi)
            var host = new Bootstrapper().BuildHost();

            desktop.MainWindow = new MainWindow
            {
                DataContext = host.Services.GetRequiredService<MainViewModel>(),
            };

            desktop.ShutdownRequested += (_, _) => host.Dispose();
#else
#if (ApiClient == "None")
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(new GreetingService()),
            };
#else
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(
                    new GreetingsApiClient(new HttpClient { BaseAddress = new Uri("http://localhost:5080/") })),
            };
#endif
#endif
        }

        base.OnFrameworkInitializationCompleted();
    }
}
