using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using Pnv.Combo.Desktop.Api;
using Pnv.Combo.Desktop.Models;
using Pnv.Combo.Desktop.Services;
using Pnv.Combo.Desktop.ViewModels;
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
        viewModel.Greetings.Should().ContainSingle().Which.Text.Should().Be("тест");
    }
}
