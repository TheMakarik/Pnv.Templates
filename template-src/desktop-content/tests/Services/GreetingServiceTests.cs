using System.Windows.Input;
using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using PnvApp.Models;
using PnvApp.Services;
using PnvApp.ViewModels;
using Xunit;

namespace PnvApp.Tests.Services;

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
    public void SayHello_UsesGreetingService()
    {
        // Arrange
        var greetingService = A.Fake<IGreetingService>();
        A.CallTo(() => greetingService.GetGreeting(A<string>._))
            .Returns(new Greeting("тест"));
        var viewModel = new MainViewModel(greetingService);

        // Act
        ((ICommand)viewModel.SayHelloCommand).Execute(null);

        // Assert
        viewModel.Greeting.Should().Be("тест");
    }
}
