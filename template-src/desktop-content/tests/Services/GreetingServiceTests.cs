using System.Windows.Input;
using AutoFixture;
using FakeItEasy;
using FluentAssertions;
using Pnv.Desktop.Models;
using Pnv.Desktop.Services;
using Pnv.Desktop.ViewModels;
using Xunit;

namespace Pnv.Desktop.Tests.Services;

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

    [Fact]
    public void SayHello_UsesGreetingService()
    {
        var greetingService = A.Fake<IGreetingService>();
        A.CallTo(() => greetingService.GetGreeting(A<string>._))
            .Returns(new Greeting("тест"));
        var viewModel = new MainViewModel(greetingService);

        ((ICommand)viewModel.SayHelloCommand).Execute(null);

        viewModel.Greeting.Should().Be("тест");
    }
}
