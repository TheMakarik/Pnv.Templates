using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Pnv.Combo.Desktop.Api;

namespace Pnv.Combo.Desktop.ViewModels;

public sealed partial class MainViewModel(IGreetingsApiClient greetingsApiClient) : ViewModelBase
{
    [ObservableProperty]
    public partial ObservableCollection<GreetingDto> Greetings { get; set; } = [];

    [ObservableProperty]
    public partial string NewGreetingText { get; set; } = string.Empty;

    public async Task InitializeAsync()
    {
        try
        {
            Greetings = [.. await greetingsApiClient.GetGreetingsAsync()];
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
        Greetings.Add(greeting);
        NewGreetingText = string.Empty;
    }
}
