using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
#if (ApiClient == "None")
using Pnv.Combo.Desktop.Services;
#else
using Pnv.Combo.Desktop.Api;
#endif

namespace Pnv.Combo.Desktop.ViewModels;

#if (ApiClient == "None")
public sealed partial class MainViewModel(IGreetingService greetingService) : ViewModelBase
{
    [ObservableProperty]
    public partial ObservableCollection<string> Greetings { get; set; } = [];

    [ObservableProperty]
    public partial string NewGreetingText { get; set; } = string.Empty;

    public Task InitializeAsync()
    {
        Greetings = [greetingService.GetGreeting("мир").Text];
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void Add()
    {
        if (string.IsNullOrWhiteSpace(NewGreetingText))
        {
            return;
        }

        Greetings.Add(greetingService.GetGreeting(NewGreetingText).Text);
        NewGreetingText = string.Empty;
    }
}
#else
public sealed partial class MainViewModel(IGreetingsApiClient greetingsApiClient) : ViewModelBase
{
    [ObservableProperty]
    public partial ObservableCollection<string> Greetings { get; set; } = [];

    [ObservableProperty]
    public partial string NewGreetingText { get; set; } = string.Empty;

    public async Task InitializeAsync()
    {
        try
        {
            Greetings = [.. (await greetingsApiClient.GetGreetingsAsync()).Select(greeting => greeting.Text)];
        }
        catch (HttpRequestException)
        {
            // API недоступен — начинаем с пустого списка.
        }
    }

    [RelayCommand]
    private async Task Add(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(NewGreetingText))
        {
            return;
        }

        var greeting = await greetingsApiClient.CreateGreetingAsync(NewGreetingText, cancellationToken);
        Greetings.Add(greeting.Text);
        NewGreetingText = string.Empty;
    }
}
#endif
