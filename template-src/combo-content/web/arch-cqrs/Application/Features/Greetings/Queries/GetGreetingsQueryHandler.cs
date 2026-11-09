#if (UseWebEfCore)
using Microsoft.EntityFrameworkCore;
#endif
using MediatR;
using Pnv.Combo.Web.Domain;
using Pnv.Combo.Web.Infrastructure;

namespace Pnv.Combo.Web.Application.Features.Greetings;

#if (UseWebEfCore)
public sealed class GetGreetingsQueryHandler(AppDbContext dbContext)
    : IRequestHandler<GetGreetingsQuery, IReadOnlyList<Greeting>>
{
    public async Task<IReadOnlyList<Greeting>> Handle(GetGreetingsQuery query, CancellationToken cancellationToken)
        => await dbContext.Greetings
            .OrderBy(record => record.Id)
            .Select(record => new Greeting(record.Text))
            .ToListAsync(cancellationToken);
}
#else
public sealed class GetGreetingsQueryHandler(InMemoryGreetingStore store)
    : IRequestHandler<GetGreetingsQuery, IReadOnlyList<Greeting>>
{
    public Task<IReadOnlyList<Greeting>> Handle(GetGreetingsQuery query, CancellationToken cancellationToken)
        => Task.FromResult(store.GetAll());
}
#endif
