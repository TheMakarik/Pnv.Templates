using Pnv.Combo.Web.Domain;

namespace Pnv.Combo.Web.Application;

public interface IGreetingService
{
    Task<IReadOnlyList<Greeting>> GetGreetingsAsync(CancellationToken cancellationToken = default);
    Task<Greeting> CreateGreetingAsync(string text, CancellationToken cancellationToken = default);
}
