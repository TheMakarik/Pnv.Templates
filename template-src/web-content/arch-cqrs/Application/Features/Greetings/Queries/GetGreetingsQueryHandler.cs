#if (UseEfCore)
using Microsoft.EntityFrameworkCore;
#endif
using MediatR;
using Pnv.Web.Domain;
using Pnv.Web.Infrastructure;

namespace Pnv.Web.Application.Features.Greetings;

#if (UseEfCore)
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
