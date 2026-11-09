namespace Pnv.Combo.Desktop.Api;

public interface IGreetingsApiClient
{
    Task<IReadOnlyList<GreetingDto>> GetGreetingsAsync(CancellationToken cancellationToken = default);
    Task<GreetingDto> CreateGreetingAsync(string text, CancellationToken cancellationToken = default);
}

public sealed record GreetingDto(string Text);
