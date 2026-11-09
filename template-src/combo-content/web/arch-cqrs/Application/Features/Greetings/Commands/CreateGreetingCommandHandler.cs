#if (UseWebEfCore)
using Microsoft.EntityFrameworkCore;
#endif
using MediatR;
using Pnv.Combo.Web.Domain;
using Pnv.Combo.Web.Infrastructure;

namespace Pnv.Combo.Web.Application.Features.Greetings;

#if (UseWebEfCore)
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
