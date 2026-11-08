using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PnvApp.Services;

namespace PnvApp.ViewModels;

public sealed partial class MainViewModel(IGreetingService greetingService) : ViewModelBase
{
    [ObservableProperty]
    public partial string Greeting { get; set; } = "Привет!";

    [RelayCommand]
    private void SayHello() => Greeting = greetingService.GetGreeting("мир").Text;
}
