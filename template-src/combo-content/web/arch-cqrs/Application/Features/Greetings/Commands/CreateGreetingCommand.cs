using MediatR;
using Pnv.Combo.Web.Domain;

namespace Pnv.Combo.Web.Application.Features.Greetings;

public sealed record CreateGreetingCommand(string Text) : IRequest<Greeting>;
