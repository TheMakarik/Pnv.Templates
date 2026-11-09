using Pnv.Web.Domain;

namespace Pnv.Web.Infrastructure;

public sealed class InMemoryGreetingStore
{
    private readonly object _lock = new();
    private readonly List<Greeting> _greetings = [];

    public Greeting Add(string text)
    {
        var greeting = new Greeting(text);

        lock (_lock)
        {
            _greetings.Add(greeting);
        }

        return greeting;
    }

    public IReadOnlyList<Greeting> GetAll()
    {
        lock (_lock)
        {
            return [.. _greetings];
        }
    }
}
