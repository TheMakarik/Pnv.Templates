using Avalonia;
#if (UseReactiveUI)
using ReactiveUI.Avalonia;
#endif

namespace Pnv.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
        => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if (UseReactiveUI)
            .UseReactiveUI(_ => { })
#endif
            .WithInterFont()
            .LogToTrace();
}
