using PnvApp.Services;
using ReactiveUI.SourceGenerators;

namespace PnvApp.ViewModels;

public sealed partial class MainViewModel(IGreetingService greetingService) : ViewModelBase
{
    [Reactive]
    private string _greeting = "Привет!";

    [ReactiveCommand]
    private void SayHello() => Greeting = greetingService.GetGreeting("мир").Text;
}
