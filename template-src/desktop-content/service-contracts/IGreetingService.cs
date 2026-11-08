using PnvApp.Models;

namespace PnvApp.Services;

public interface IGreetingService
{
    Greeting GetGreeting(string name);
}
