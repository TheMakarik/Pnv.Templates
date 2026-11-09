#if (!UseWebEfCore)
using Pnv.Combo.Web.Domain;

namespace Pnv.Combo.Web.Application;

public sealed class GreetingService : IGreetingService
{
    private readonly object _lock = new();
    private readonly List<Greeting> _greetings = [];

    public Task<IReadOnlyList<Greeting>> GetGreetingsAsync(CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Greeting> greetings;
        lock (_lock)
        {
            greetings = [.. _greetings];
        }

        return Task.FromResult(greetings);
    }

    public Task<Greeting> CreateGreetingAsync(string text, CancellationToken cancellationToken = default)
    {
        var greeting = new Greeting(text);

        lock (_lock)
        {
            _greetings.Add(greeting);
        }

        return Task.FromResult(greeting);
    }
}
#endif
