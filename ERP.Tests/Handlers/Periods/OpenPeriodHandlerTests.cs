using Application.Handlers.Periods;
using Application.Interfaces;
using Application.Models.Periods.Commands;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Handlers.Periods;

public sealed class OpenPeriodHandlerTests : HandlerTestBase
{
    private readonly Mock<IPeriodRepository> _periods;
    private readonly OpenPeriodHandler _handler;

    public OpenPeriodHandlerTests()
    {
        _periods = RegisterMock<IPeriodRepository>();
        _handler = CreateHandler<OpenPeriodHandler>();
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_OpensPeriod()
    {
        _periods
            .Setup(x => x.OpenAsync(2026, 3, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(new OpenPeriodCommand { Year = 2026, Month = 3 }, CancellationToken.None);

        _periods.Verify(x => x.OpenAsync(2026, 3, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRepositoryFails_PropagatesException()
    {
        _periods
            .Setup(x => x.OpenAsync(2026, 3, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Не удалось открыть период."));

        var act = () => _handler.Handle(
            new OpenPeriodCommand { Year = 2026, Month = 3 },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Не удалось открыть период.");
    }
}
