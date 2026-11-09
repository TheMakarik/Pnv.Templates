using System.Net.Http.Json;

namespace Pnv.Web.Frontend.Services;

public sealed class GreetingsApiClient(HttpClient httpClient)
{
    public async Task<IReadOnlyList<GreetingDto>> GetGreetingsAsync(CancellationToken cancellationToken = default)
        => await httpClient.GetFromJsonAsync<IReadOnlyList<GreetingDto>>("/api/greetings", cancellationToken) ?? [];

    public async Task<GreetingDto> CreateGreetingAsync(string text, CancellationToken cancellationToken = default)
    {
        var response = await httpClient.PostAsJsonAsync("/api/greetings", new { text }, cancellationToken);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<GreetingDto>(cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Пустой ответ API.");
    }
}

public sealed record GreetingDto(string Text);
