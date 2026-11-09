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
        // Arrange
        var name = _fixture.Create<string>();

        // Act
        var greeting = systemUnderTests.GetGreeting(name);

        // Assert
        greeting.Text.Should().Contain(name);
    }

#if (ApiClient == "None")
    [Fact]
    public void Add_UsesGreetingService()
    {
        // Arrange
        var greetingService = A.Fake<IGreetingService>();
        A.CallTo(() => greetingService.GetGreeting(A<string>._))
            .Returns(new Greeting("тест"));
        var viewModel = new MainViewModel(greetingService)
        {
            NewGreetingText = "мир",
        };

        // Act
        ((ICommand)viewModel.AddCommand).Execute(null);

        // Assert
        viewModel.Greetings.Should().Contain("тест");
    }
#else
    [Fact]
    public async Task InitializeAsync_LoadsGreetingsFromApi()
    {
        // Arrange
        var greetingsApiClient = A.Fake<IGreetingsApiClient>();
        A.CallTo(() => greetingsApiClient.GetGreetingsAsync(A<CancellationToken>._))
            .Returns(new List<GreetingDto> { new("тест") });
        var viewModel = new MainViewModel(greetingsApiClient);

        // Act
        await viewModel.InitializeAsync();

        // Assert
        viewModel.Greetings.Should().ContainSingle().Which.Should().Be("тест");
    }
#endif
}
