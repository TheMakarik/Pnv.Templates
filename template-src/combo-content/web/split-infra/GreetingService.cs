using Microsoft.EntityFrameworkCore;
using Pnv.Combo.Web.Application;
using Pnv.Combo.Web.Domain;

namespace Pnv.Combo.Web.Infrastructure;

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
