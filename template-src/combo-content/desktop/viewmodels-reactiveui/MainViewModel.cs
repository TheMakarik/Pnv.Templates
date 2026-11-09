using System.Collections.ObjectModel;
using Pnv.Combo.Desktop.Api;
using ReactiveUI.SourceGenerators;

namespace Pnv.Combo.Desktop.ViewModels;

public sealed partial class MainViewModel(IGreetingsApiClient greetingsApiClient) : ViewModelBase
{
    [Reactive]
    private ObservableCollection<GreetingDto> _greetings = [];

    [Reactive]
    private string _newGreetingText = string.Empty;

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

    [ReactiveCommand]
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
