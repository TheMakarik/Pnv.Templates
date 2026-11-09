using Pnv.Desktop.Services;
using ReactiveUI.SourceGenerators;

namespace Pnv.Desktop.ViewModels;

public sealed partial class MainViewModel(IGreetingService greetingService) : ViewModelBase
{
    [Reactive]
    private string _greeting = "Привет!";

    [ReactiveCommand]
    private void SayHello() => Greeting = greetingService.GetGreeting("мир").Text;
}
