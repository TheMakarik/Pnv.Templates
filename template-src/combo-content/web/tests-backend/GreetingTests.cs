using AutoFixture;
using FluentAssertions;
using Xunit;
#if (UseCqrs)
using MediatR;
using Pnv.Combo.Web.Application.Features.Greetings;
#if (!UseWebEfCore)
using Pnv.Combo.Web.Infrastructure;
#endif
#else
using Pnv.Combo.Web.Domain;
using Pnv.Combo.Web.Application;
#if (!UseWebEfCore && Architecture == "CleanArchitecture")
using Pnv.Combo.Web.Infrastructure;
#endif
#endif
#if (UseWebEfCore)
using Microsoft.EntityFrameworkCore;
using Pnv.Combo.Web.Infrastructure;
#endif

namespace Pnv.Combo.Web.Tests;

public sealed class GreetingTests
{
    private readonly IFixture _fixture = new Fixture();

#if (UseWebEfCore)
    private static AppDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"pnv-web-{Guid.NewGuid():N}")
            .Options);
#endif

#if (UseCqrs)
    [Fact]
    public async Task CreateGreetingCommand_CreatesGreetingWithText()
    {
        var name = _fixture.Create<string>();

#if (UseWebEfCore)
        var handler = new CreateGreetingCommandHandler(CreateDbContext());
#else
        var handler = new CreateGreetingCommandHandler(new InMemoryGreetingStore());
#endif
        var greeting = await handler.Handle(new CreateGreetingCommand(name), CancellationToken.None);

        greeting.Text.Should().Contain(name);
    }

    [Fact]
    public async Task GetGreetingsQuery_ReturnsCreatedGreetings()
    {
        var name = _fixture.Create<string>();

#if (UseWebEfCore)
        using var dbContext = CreateDbContext();
        var createHandler = new CreateGreetingCommandHandler(dbContext);
        var queryHandler = new GetGreetingsQueryHandler(dbContext);
#else
        var store = new InMemoryGreetingStore();
        var createHandler = new CreateGreetingCommandHandler(store);
        var queryHandler = new GetGreetingsQueryHandler(store);
#endif

        await createHandler.Handle(new CreateGreetingCommand(name), CancellationToken.None);
        var greetings = await queryHandler.Handle(new GetGreetingsQuery(), CancellationToken.None);

        greetings.Should().ContainSingle(greeting => greeting.Text.Contains(name));
    }
#else
    [Fact]
    public async Task CreateGreetingAsync_CreatesGreetingWithText()
    {
        var name = _fixture.Create<string>();

#if (UseWebEfCore)
        IGreetingService service = new GreetingService(CreateDbContext());
#else
        IGreetingService service = new GreetingService();
#endif
        var greeting = await service.CreateGreetingAsync(name);

        greeting.Text.Should().Contain(name);
    }

    [Fact]
    public async Task GetGreetingsAsync_ReturnsCreatedGreetings()
    {
        var name = _fixture.Create<string>();

#if (UseWebEfCore)
        IGreetingService service = new GreetingService(CreateDbContext());
#else
        IGreetingService service = new GreetingService();
#endif
        await service.CreateGreetingAsync(name);

        var greetings = await service.GetGreetingsAsync();

        greetings.Should().ContainSingle(greeting => greeting.Text.Contains(name));
    }
#endif
}
