using Pnv.Combo.Desktop.Models;

namespace Pnv.Combo.Desktop.Services;

public interface IGreetingService
{
    Greeting GetGreeting(string name);
}
