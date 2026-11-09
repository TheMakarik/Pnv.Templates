#if (UseRefit)
using Refit;
#else
using System.Net.Http.Json;
#endif

namespace Pnv.Combo.Desktop.Api;

public sealed class GreetingsApiClient(HttpClient httpClient) : IGreetingsApiClient
{
#if (UseRefit)
    private readonly IGreetingsApi _api = RestService.For<IGreetingsApi>(httpClient);

    public async Task<IReadOnlyList<GreetingDto>> GetGreetingsAsync(CancellationToken cancellationToken = default)
        => await _api.GetGreetingsAsync(cancellationToken);

    public async Task<GreetingDto> CreateGreetingAsync(string text, CancellationToken cancellationToken = default)
        => await _api.CreateGreetingAsync(new CreateGreetingRequest(text), cancellationToken);
#else
    public async Task<IReadOnlyList<GreetingDto>> GetGreetingsAsync(CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<IReadOnlyList<GreetingDto>>("/api/greetings", cancellationToken) ?? [];

    public async Task<GreetingDto> CreateGreetingAsync(string text, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/greetings", new { text }, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GreetingDto>(cancellationToken: cancellationToken)
               ?? throw new InvalidOperationException("Пустой ответ API.");
    }
#endif
}
