using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Pnv.Desktop.View;
using Pnv.Desktop.ViewModels;
#if (UseDi)
using Microsoft.Extensions.DependencyInjection;
#else
using Pnv.Desktop.Services;
#endif

namespace Pnv.Desktop;

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
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainViewModel(new GreetingService()),
            };
#endif
        }

        base.OnFrameworkInitializationCompleted();
    }
}
