#if (UseEfCore)
using Microsoft.EntityFrameworkCore;
using Pnv.Web.Infrastructure;
#endif
using Pnv.Web.Domain;

namespace Pnv.Web.Application;

#if (UseEfCore)
public sealed class GreetingService(AppDbContext dbContext) : IGreetingService
{
    public async Task<IReadOnlyList<Greeting>> GetGreetingsAsync(CancellationToken cancellationToken = default)
        => await dbContext.Greetings
            .OrderBy(record => record.Id)
            .Select(record => new Greeting(record.Text))
            .ToListAsync(cancellationToken);

    public async Task<Greeting> CreateGreetingAsync(string text, CancellationToken cancellationToken = default)
    {
        var record = new GreetingRecord { Text = text, CreatedAt = DateTimeOffset.UtcNow };

        dbContext.Greetings.Add(record);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new Greeting(record.Text);
    }
}
#else
public sealed class GreetingService : IGreetingService
{
    private readonly object _lock = new();
    private readonly List<Greeting> _greetings = [];

    public Task<IReadOnlyList<Greeting>> GetGreetingsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lock)
        {
            return Task.FromResult<IReadOnlyList<Greeting>>([.. _greetings]);
        }
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
