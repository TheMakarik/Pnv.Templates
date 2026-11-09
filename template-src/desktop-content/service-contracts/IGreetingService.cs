using Pnv.Desktop.Models;

namespace Pnv.Desktop.Services;

public interface IGreetingService
{
    Greeting GetGreeting(string name);
}
