using MediatR;
using Pnv.Web.Domain;

namespace Pnv.Web.Application.Features.Greetings;

public sealed record GetGreetingsQuery : IRequest<IReadOnlyList<Greeting>>;
