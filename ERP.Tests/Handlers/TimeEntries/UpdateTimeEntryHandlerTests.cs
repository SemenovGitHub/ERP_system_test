using Application.Handlers.TimeEntries;
using Application.Interfaces;
using Application.Models.TimeEntries.Commands;
using Domain.Exceptions;
using Domain.TimeEntries;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Handlers.TimeEntries;

public sealed class UpdateTimeEntryHandlerTests : HandlerTestBase
{
    private readonly Mock<IEmployeeRepository> _employees;
    private readonly Mock<IProjectRepository> _projects;
    private readonly Mock<IPeriodRepository> _periods;
    private readonly Mock<ITimeEntryRepository> _timeEntries;
    private readonly UpdateTimeEntryHandler _handler;

    public UpdateTimeEntryHandlerTests()
    {
        _employees = RegisterMock<IEmployeeRepository>();
        _projects = RegisterMock<IProjectRepository>();
        _periods = RegisterMock<IPeriodRepository>();
        _timeEntries = RegisterMock<ITimeEntryRepository>();
        _handler = CreateHandler<UpdateTimeEntryHandler>();
    }

    [Fact]
    public async Task Handle_WhenEntryExists_UpdatesAndReturnsResponse()
    {
        var employee = TestData.Employee();
        var project = TestData.Project();
        var existing = TestData.TimeEntry(employee.Id, project.Id);

        _timeEntries
            .Setup(x => x.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _employees
            .Setup(x => x.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _projects
            .Setup(x => x.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        _periods
            .Setup(x => x.IsClosedAsync(TestData.Date.Year, TestData.Date.Month, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _timeEntries
            .Setup(x => x.GetHoursForDayAsync(employee.Id, TestData.Date, existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);
        _timeEntries
            .Setup(x => x.UpdateAsync(It.IsAny<TimeEntry>(), 1, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new UpdateTimeEntryCommand
            {
                Id = existing.Id,
                EmployeeId = employee.Id,
                ProjectId = project.Id,
                Date = TestData.Date,
                Hours = 4,
                Comment = "Обновлено",
                Version = 1
            },
            CancellationToken.None);

        result.Hours.Should().Be(4);
        result.Comment.Should().Be("Обновлено");
        result.Version.Should().Be(2);
        _timeEntries.Verify(
            x => x.UpdateAsync(
                It.Is<TimeEntry>(entry => entry.Id == existing.Id && entry.Hours == 4 && entry.Version == 2),
                1,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEntryDoesNotExist_ThrowsBusinessException()
    {
        var id = Guid.NewGuid();
        _timeEntries
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((TimeEntry?)null);

        var act = () => _handler.Handle(
            new UpdateTimeEntryCommand
            {
                Id = id,
                EmployeeId = Guid.NewGuid(),
                ProjectId = Guid.NewGuid(),
                Date = TestData.Date,
                Hours = 4,
                Version = 1
            },
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<BusinessException>();
        exception.Which.Code.Should().Be(ErrorCodes.NotFound);
        exception.Which.Message.Should().Be("Запись табеля не найдена.");
        _timeEntries.Verify(
            x => x.UpdateAsync(It.IsAny<TimeEntry>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
