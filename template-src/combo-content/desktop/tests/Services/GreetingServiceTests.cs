#if (ApiClient == "None")
using System.Windows.Input;
#endif
using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using Pnv.Combo.Desktop.Services;
using Pnv.Combo.Desktop.ViewModels;
#if (ApiClient == "None")
using Pnv.Combo.Desktop.Models;
#else
using Pnv.Combo.Desktop.Api;
#endif
using Xunit;

namespace Pnv.Combo.Desktop.Tests.Services;

public sealed class GreetingServiceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GreetingService systemUnderTests = new();

    [Fact]
    public void GetGreeting_ReturnsGreetingWithName()
    {
        var name = _fixture.Create<string>();

        var greeting = systemUnderTests.GetGreeting(name);

        greeting.Text.Should().Contain(name);
    }

#if (ApiClient == "None")
    [Fact]
    public void Add_UsesGreetingService()
    {
        var greetingService = A.Fake<IGreetingService>();
        A.CallTo(() => greetingService.GetGreeting(A<string>._))
            .Returns(new Greeting("тест"));
        var viewModel = new MainViewModel(greetingService)
        {
            NewGreetingText = "мир",
        };

        ((ICommand)viewModel.AddCommand).Execute(null);

        viewModel.Greetings.Should().Contain("тест");
    }
#else
    [Fact]
    public async Task InitializeAsync_LoadsGreetingsFromApi()
    {
        var greetingsApiClient = A.Fake<IGreetingsApiClient>();
        A.CallTo(() => greetingsApiClient.GetGreetingsAsync(A<CancellationToken>._))
            .Returns(new List<GreetingDto> { new("тест") });
        var viewModel = new MainViewModel(greetingsApiClient);

        await viewModel.InitializeAsync();

        viewModel.Greetings.Should().ContainSingle().Which.Should().Be("тест");
    }
#endif
}
