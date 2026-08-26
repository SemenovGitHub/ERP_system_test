using Application.Handlers.TimeEntries;
using Application.Interfaces;
using Application.Models.TimeEntries.Commands;
using Domain.Employees;
using Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Handlers.TimeEntries;

public sealed class CreateTimeEntryHandlerTests : HandlerTestBase
{
    private readonly Mock<IEmployeeRepository> _employees;
    private readonly Mock<IProjectRepository> _projects;
    private readonly Mock<IPeriodRepository> _periods;
    private readonly Mock<ITimeEntryRepository> _timeEntries;
    private readonly CreateTimeEntryHandler _handler;

    public CreateTimeEntryHandlerTests()
    {
        _employees = RegisterMock<IEmployeeRepository>();
        _projects = RegisterMock<IProjectRepository>();
        _periods = RegisterMock<IPeriodRepository>();
        _timeEntries = RegisterMock<ITimeEntryRepository>();
        _handler = CreateHandler<CreateTimeEntryHandler>();
    }

    [Fact]
    public async Task Handle_WhenCommandIsValid_AddsEntryAndReturnsResponse()
    {
        var employee = TestData.Employee();
        var project = TestData.Project();
        SetupLookups(employee, project);
        _timeEntries
            .Setup(x => x.GetHoursForDayAsync(employee.Id, TestData.Date, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(0);

        var result = await _handler.Handle(
            new CreateTimeEntryCommand
            {
                EmployeeId = employee.Id,
                ProjectId = project.Id,
                Date = TestData.Date,
                Hours = 8,
                Comment = "Работы на объекте"
            },
            CancellationToken.None);

        result.EmployeeFullName.Should().Be(employee.FullName);
        result.ProjectName.Should().Be(project.Name);
        result.Hours.Should().Be(8);
        result.Rate.Should().Be(600);
        result.Cost.Should().Be(4800);
        result.Comment.Should().Be("Работы на объекте");
        _timeEntries.Verify(
            x => x.AddAsync(
                It.Is<Domain.TimeEntries.TimeEntry>(entry =>
                    entry.EmployeeId == employee.Id
                    && entry.ProjectId == project.Id
                    && entry.Hours == 8
                    && entry.Version == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmployeeDoesNotExist_ThrowsBusinessException()
    {
        var employeeId = Guid.NewGuid();
        var project = TestData.Project();
        _employees
            .Setup(x => x.GetByIdAsync(employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);
        _projects
            .Setup(x => x.GetByIdAsync(project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        var act = () => _handler.Handle(
            new CreateTimeEntryCommand
            {
                EmployeeId = employeeId,
                ProjectId = project.Id,
                Date = TestData.Date,
                Hours = 8
            },
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<BusinessException>();
        exception.Which.Code.Should().Be(ErrorCodes.NotFound);
        exception.Which.Message.Should().Be("Сотрудник не найден.");
        _timeEntries.Verify(
            x => x.AddAsync(It.IsAny<Domain.TimeEntries.TimeEntry>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void SetupLookups(Domain.Employees.Employee employee, Domain.Projects.Project project)
    {
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
            .Setup(x => x.AddAsync(It.IsAny<Domain.TimeEntries.TimeEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }
}
