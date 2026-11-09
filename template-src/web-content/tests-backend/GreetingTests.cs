using AutoFixture;
using FluentAssertions;
using Xunit;
#if (UseCqrs)
using MediatR;
using Pnv.Web.Application.Features.Greetings;
#if (!UseEfCore)
using Pnv.Web.Infrastructure;
#endif
#else
using Pnv.Web.Domain;
using Pnv.Web.Application;
#if (!UseEfCore && Architecture == "CleanArchitecture")
using Pnv.Web.Infrastructure;
#endif
#endif
#if (UseEfCore)
using Microsoft.EntityFrameworkCore;
using Pnv.Web.Infrastructure;
#endif

namespace Pnv.Web.Tests;

public sealed class GreetingTests
{
    private readonly IFixture _fixture = new Fixture();

#if (UseEfCore)
    private static AppDbContext CreateDbContext()
        => new(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase($"pnv-web-{Guid.NewGuid():N}")
            .Options);
#endif

#if (UseCqrs)
    [Fact]
    public async Task CreateGreetingCommand_CreatesGreetingWithText()
    {
        // Arrange
        var name = _fixture.Create<string>();

        // Act
#if (UseEfCore)
        var handler = new CreateGreetingCommandHandler(CreateDbContext());
#else
        var handler = new CreateGreetingCommandHandler(new InMemoryGreetingStore());
#endif
        var greeting = await handler.Handle(new CreateGreetingCommand(name), CancellationToken.None);

        // Assert
        greeting.Text.Should().Contain(name);
    }

    [Fact]
    public async Task GetGreetingsQuery_ReturnsCreatedGreetings()
    {
        // Arrange
        var name = _fixture.Create<string>();

#if (UseEfCore)
        using var dbContext = CreateDbContext();
        var createHandler = new CreateGreetingCommandHandler(dbContext);
        var queryHandler = new GetGreetingsQueryHandler(dbContext);
#else
        var store = new InMemoryGreetingStore();
        var createHandler = new CreateGreetingCommandHandler(store);
        var queryHandler = new GetGreetingsQueryHandler(store);
#endif

        // Act
        await createHandler.Handle(new CreateGreetingCommand(name), CancellationToken.None);
        var greetings = await queryHandler.Handle(new GetGreetingsQuery(), CancellationToken.None);

        // Assert
        greetings.Should().ContainSingle(greeting => greeting.Text.Contains(name));
    }
#else
    [Fact]
    public async Task CreateGreetingAsync_CreatesGreetingWithText()
    {
        // Arrange
        var name = _fixture.Create<string>();

        // Act
#if (UseEfCore)
        IGreetingService service = new GreetingService(CreateDbContext());
#else
        IGreetingService service = new GreetingService();
#endif
        var greeting = await service.CreateGreetingAsync(name);

        // Assert
        greeting.Text.Should().Contain(name);
    }

    [Fact]
    public async Task GetGreetingsAsync_ReturnsCreatedGreetings()
    {
        // Arrange
        var name = _fixture.Create<string>();

#if (UseEfCore)
        IGreetingService service = new GreetingService(CreateDbContext());
#else
        IGreetingService service = new GreetingService();
#endif
        await service.CreateGreetingAsync(name);

        // Act
        var greetings = await service.GetGreetingsAsync();

        // Assert
        greetings.Should().ContainSingle(greeting => greeting.Text.Contains(name));
    }
#endif
}
