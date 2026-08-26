using Application.Handlers.TimeEntries;
using Application.Interfaces;
using Application.Models.TimeEntries.Queries;
using Domain.Employees;
using Domain.Exceptions;
using Domain.Projects;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Handlers.TimeEntries;

public sealed class GetTimeEntriesHandlerTests : HandlerTestBase
{
    private readonly Mock<ITimeEntryRepository> _timeEntries;
    private readonly Mock<IEmployeeRepository> _employees;
    private readonly Mock<IProjectRepository> _projects;
    private readonly GetTimeEntriesHandler _handler;

    public GetTimeEntriesHandlerTests()
    {
        _timeEntries = RegisterMock<ITimeEntryRepository>();
        _employees = RegisterMock<IEmployeeRepository>();
        _projects = RegisterMock<IProjectRepository>();
        _handler = CreateHandler<GetTimeEntriesHandler>();
    }

    [Fact]
    public async Task Handle_WhenEntriesExist_LoadsRelatedEntitiesAndReturnsPage()
    {
        var employee = TestData.Employee();
        var project = TestData.Project();
        var entry = TestData.TimeEntry(employee.Id, project.Id);

        _timeEntries
            .Setup(x => x.GetPagedAsync(2026, 3, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedTimeEntries
            {
                Items = [entry],
                TotalCount = 1,
                TotalHours = 8,
                TotalCost = 4800
            });
        _employees
            .Setup(x => x.GetByIdsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(employee.Id)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Employee> { employee });
        _projects
            .Setup(x => x.GetByIdsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.Contains(project.Id)),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Project> { project });
        _timeEntries
            .Setup(x => x.GetHoursByDayAsync(
                It.IsAny<IReadOnlyCollection<(Guid EmployeeId, DateOnly Date)>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<(Guid, DateOnly), decimal>
            {
                [(employee.Id, entry.Date)] = 8
            });

        var result = await _handler.Handle(
            new GetTimeEntriesQuery { Year = 2026, Month = 3, Page = 1, PageSize = 10 },
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.TotalHours.Should().Be(8);
        result.Items.Should().ContainSingle();
        result.Items.First().EmployeeFullName.Should().Be(employee.FullName);
        result.Items.First().ProjectName.Should().Be(project.Name);
        result.Items.First().Hours.Should().Be(8);
        result.Items.First().Rate.Should().Be(600);
        _employees.Verify(
            x => x.GetByIdsAsync(
                It.Is<IReadOnlyCollection<Guid>>(ids => ids.SequenceEqual(new[] { employee.Id })),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenRelatedEmployeeIsMissing_ThrowsBusinessException()
    {
        var employeeId = Guid.NewGuid();
        var project = TestData.Project();
        var entry = TestData.TimeEntry(employeeId, project.Id);

        _timeEntries
            .Setup(x => x.GetPagedAsync(2026, 3, null, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedTimeEntries { Items = [entry], TotalCount = 1 });
        _employees
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Employee>());
        _projects
            .Setup(x => x.GetByIdsAsync(It.IsAny<IReadOnlyCollection<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Project> { project });
        _timeEntries
            .Setup(x => x.GetHoursByDayAsync(
                It.IsAny<IReadOnlyCollection<(Guid EmployeeId, DateOnly Date)>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Dictionary<(Guid, DateOnly), decimal>());

        var act = () => _handler.Handle(
            new GetTimeEntriesQuery { Year = 2026, Month = 3, Page = 1, PageSize = 10 },
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<BusinessException>();
        exception.Which.Code.Should().Be(ErrorCodes.NotFound);
        exception.Which.Message.Should().Be("Сотрудник записи не найден.");
    }
}
