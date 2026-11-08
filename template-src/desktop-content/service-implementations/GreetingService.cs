using PnvApp.Models;

namespace PnvApp.Services;

public sealed class GreetingService : IGreetingService
{
    public Greeting GetGreeting(string name) => new($"Привет, {name}!");
}
