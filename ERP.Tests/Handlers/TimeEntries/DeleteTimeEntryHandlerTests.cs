using Application.Handlers.TimeEntries;
using Application.Interfaces;
using Application.Models.TimeEntries.Commands;
using Domain.Exceptions;
using Domain.TimeEntries;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Handlers.TimeEntries;

public sealed class DeleteTimeEntryHandlerTests : HandlerTestBase
{
    private readonly Mock<ITimeEntryRepository> _timeEntries;
    private readonly Mock<IPeriodRepository> _periods;
    private readonly DeleteTimeEntryHandler _handler;

    public DeleteTimeEntryHandlerTests()
    {
        _timeEntries = RegisterMock<ITimeEntryRepository>();
        _periods = RegisterMock<IPeriodRepository>();
        _handler = CreateHandler<DeleteTimeEntryHandler>();
    }

    [Fact]
    public async Task Handle_WhenEntryExistsAndPeriodIsOpen_DeletesEntry()
    {
        var existing = TestData.TimeEntry(Guid.NewGuid(), Guid.NewGuid());
        _timeEntries
            .Setup(x => x.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _periods
            .Setup(x => x.IsClosedAsync(existing.Date.Year, existing.Date.Month, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _timeEntries
            .Setup(x => x.DeleteAsync(existing.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _handler.Handle(new DeleteTimeEntryCommand { Id = existing.Id }, CancellationToken.None);

        _timeEntries.Verify(x => x.DeleteAsync(existing.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEntryDoesNotExist_ThrowsBusinessException()
    {
        var id = Guid.NewGuid();
        _timeEntries
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeEntry?)null);

        var act = () => _handler.Handle(new DeleteTimeEntryCommand { Id = id }, CancellationToken.None);

        var exception = await act.Should().ThrowAsync<BusinessException>();
        exception.Which.Code.Should().Be(ErrorCodes.NotFound);
        exception.Which.Message.Should().Be("Запись табеля не найдена.");
        _timeEntries.Verify(x => x.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
