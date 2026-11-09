#if (UseRefit)
using Refit;

namespace Pnv.Combo.Desktop.Api;

public interface IGreetingsApi
{
    [Get("/api/greetings")]
    Task<IReadOnlyList<GreetingDto>> GetGreetingsAsync(CancellationToken cancellationToken = default);

    [Post("/api/greetings")]
    Task<GreetingDto> CreateGreetingAsync([Body] CreateGreetingRequest request, CancellationToken cancellationToken = default);
}

public sealed record CreateGreetingRequest(string Text);
#endif
