using Pnv.Combo.Web.Application;

namespace Pnv.Combo.Web.Endpoints;

public static class GreetingEndpoints
{
    public static IEndpointRouteBuilder MapGreetingEndpoints(this IEndpointRouteBuilder app)
    {
        var greetings = app.MapGroup("/api/greetings").WithTags("Greetings");

        greetings.MapGet("/", async (IGreetingService greetingService, CancellationToken cancellationToken)
            => TypedResults.Ok(await greetingService.GetGreetingsAsync(cancellationToken)));

        greetings.MapPost("/", async (CreateGreetingRequest request, IGreetingService greetingService, CancellationToken cancellationToken)
            => TypedResults.Created("/api/greetings", await greetingService.CreateGreetingAsync(request.Text, cancellationToken)));

        return app;
    }
}

public sealed record CreateGreetingRequest(string Text);
