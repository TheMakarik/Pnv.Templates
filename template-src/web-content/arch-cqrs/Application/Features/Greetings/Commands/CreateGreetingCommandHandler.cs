#if (UseEfCore)
using Microsoft.EntityFrameworkCore;
#endif
using MediatR;
using Pnv.Web.Domain;
using Pnv.Web.Infrastructure;

namespace Pnv.Web.Application.Features.Greetings;

#if (UseEfCore)
public sealed class CreateGreetingCommandHandler(AppDbContext dbContext)
    : IRequestHandler<CreateGreetingCommand, Greeting>
{
    public async Task<Greeting> Handle(CreateGreetingCommand command, CancellationToken cancellationToken)
    {
        var record = new GreetingRecord { Text = command.Text, CreatedAt = DateTimeOffset.UtcNow };

        dbContext.Greetings.Add(record);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new Greeting(record.Text);
    }
}
#else
public sealed class CreateGreetingCommandHandler(InMemoryGreetingStore store)
    : IRequestHandler<CreateGreetingCommand, Greeting>
{
    public Task<Greeting> Handle(CreateGreetingCommand command, CancellationToken cancellationToken)
        => Task.FromResult(store.Add(command.Text));
}
#endif
