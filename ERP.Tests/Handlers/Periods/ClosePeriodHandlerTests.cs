using Application.Handlers.Periods;
using Application.Interfaces;
using Application.Models.Periods.Commands;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Handlers.Periods;

public sealed class ClosePeriodHandlerTests : HandlerTestBase
{
    private readonly Mock<IPeriodRepository> _periods;
    private readonly ClosePeriodHandler _handler;

    public ClosePeriodHandlerTests()
    {
        _periods = RegisterMock<IPeriodRepository>();
        _handler = CreateHandler<ClosePeriodHandler>();
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_ClosesPeriod()
    {
        _periods
            .Setup(x => x.CloseAsync(2026, 3, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(new ClosePeriodCommand { Year = 2026, Month = 3 }, CancellationToken.None);

        _periods.Verify(x => x.CloseAsync(2026, 3, It.IsAny<CancellationToken>()), Times.Once);
        _periods.Verify(
            x => x.IsClosedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_WhenRepositoryFails_PropagatesException()
    {
        _periods
            .Setup(x => x.CloseAsync(2026, 3, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Не удалось закрыть период."));

        var act = () => _handler.Handle(
            new ClosePeriodCommand { Year = 2026, Month = 3 },
            CancellationToken.None);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Не удалось закрыть период.");
    }
}
