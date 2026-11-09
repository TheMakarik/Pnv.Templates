using Pnv.Desktop.Models;

namespace Pnv.Desktop.Services;

public sealed class GreetingService : IGreetingService
{
    public Greeting GetGreeting(string name) => new($"Привет, {name}!");
}
