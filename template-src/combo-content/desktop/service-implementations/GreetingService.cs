using Pnv.Combo.Desktop.Models;

namespace Pnv.Combo.Desktop.Services;

public sealed class GreetingService : IGreetingService
{
    public Greeting GetGreeting(string name) => new($"Привет, {name}!");
}
