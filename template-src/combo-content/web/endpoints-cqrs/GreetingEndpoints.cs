using MediatR;
using Pnv.Combo.Web.Application.Features.Greetings;

namespace Pnv.Combo.Web.Endpoints;

public static class GreetingEndpoints
{
    public static IEndpointRouteBuilder MapGreetingEndpoints(this IEndpointRouteBuilder app)
    {
        var greetings = app.MapGroup("/api/greetings").WithTags("Greetings");

        greetings.MapGet("/", async (ISender sender, CancellationToken cancellationToken)
            => TypedResults.Ok(await sender.Send(new GetGreetingsQuery(), cancellationToken)));

        greetings.MapPost("/", async (CreateGreetingRequest request, ISender sender, CancellationToken cancellationToken)
            => TypedResults.Created("/api/greetings", await sender.Send(new CreateGreetingCommand(request.Text), cancellationToken)));

        return app;
    }
}

public sealed record CreateGreetingRequest(string Text);
