using Pnv.Web.Domain;

namespace Pnv.Web.Application;

public interface IGreetingService
{
    Task<IReadOnlyList<Greeting>> GetGreetingsAsync(CancellationToken cancellationToken = default);
    Task<Greeting> CreateGreetingAsync(string text, CancellationToken cancellationToken = default);
}
