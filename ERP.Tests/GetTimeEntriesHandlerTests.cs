using Application.Handlers.TimeEntries;
using Application.Interfaces;
using Application.Models.TimeEntries.Queries;
using Domain.Employees;
using Domain.Projects;
using Domain.TimeEntries;

namespace ERP.Tests;

public class GetTimeEntriesHandlerTests
{
    [Fact]
    public async Task Loads_only_employees_and_projects_from_the_current_page()
    {
        var pageEmployeeId = Guid.NewGuid();
        var otherEmployeeId = Guid.NewGuid();
        var pageProjectId = Guid.NewGuid();
        var date = new DateOnly(2026, 8, 1);

        var employees = new RecordingEmployeeRepository(
        [
            new Employee
            {
                Id = pageEmployeeId,
                FullName = "Ivan",
                Department = "Dev",
                Rates = [new Rate { From = date, Value = 100 }]
            },
            new Employee
            {
                Id = otherEmployeeId,
                FullName = "Petr",
                Department = "Dev",
                Rates = [new Rate { From = date, Value = 100 }]
            }
        ]);
        var projects = new RecordingProjectRepository(
        [
            new Project
            {
                Id = pageProjectId,
                Code = "P1",
                Name = "Project",
                Budget = 1000,
                StartDate = date
            }
        ]);
        var timeEntries = new StubTimeEntries(
            new TimeEntry
            {
                Id = Guid.NewGuid(),
                EmployeeId = pageEmployeeId,
                ProjectId = pageProjectId,
                Date = date,
                Hours = 8,
                Version = 1
            });

        var handler = new GetTimeEntriesHandler(timeEntries, employees, projects);

        var result = await handler.Handle(
            new GetTimeEntriesQuery { Year = 2026, Month = 8, Page = 1, PageSize = 20 },
            CancellationToken.None);

        Assert.Equal([pageEmployeeId], employees.RequestedIds);
        Assert.Equal([pageProjectId], projects.RequestedIds);
        Assert.Single(result.Items);
        Assert.Equal("Ivan", result.Items.First().EmployeeFullName);
    }

    private sealed class RecordingEmployeeRepository : IEmployeeRepository
    {
        private readonly IReadOnlyList<Employee> _all;

        public RecordingEmployeeRepository(IReadOnlyList<Employee> all) => _all = all;

        public IReadOnlyList<Guid> RequestedIds { get; private set; } = [];

        public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(_all.FirstOrDefault(employee => employee.Id == id));

        public Task<IReadOnlyList<Employee>> GetByIdsAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken)
        {
            RequestedIds = ids.ToArray();
            var idSet = ids.ToHashSet();
            return Task.FromResult<IReadOnlyList<Employee>>(
                _all.Where(employee => idSet.Contains(employee.Id)).ToList());
        }

        public Task<PagedResult<Employee>> QueryAsync(
            IReadOnlyCollection<Guid>? ids,
            string? department,
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Employee query is not used when loading a timesheet page.");

        public Task UpdateRatesAsync(
            Guid id,
            IReadOnlyList<Rate> rates,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }

    private sealed class RecordingProjectRepository : IProjectRepository
    {
        private readonly IReadOnlyList<Project> _all;

        public RecordingProjectRepository(IReadOnlyList<Project> all) => _all = all;

        public IReadOnlyList<Guid> RequestedIds { get; private set; } = [];

        public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult(_all.FirstOrDefault(project => project.Id == id));

        public Task<IReadOnlyList<Project>> GetByIdsAsync(
            IReadOnlyCollection<Guid> ids,
            CancellationToken cancellationToken)
        {
            RequestedIds = ids.ToArray();
            var idSet = ids.ToHashSet();
            return Task.FromResult<IReadOnlyList<Project>>(
                _all.Where(project => idSet.Contains(project.Id)).ToList());
        }

        public Task<PagedResult<Project>> QueryAsync(
            IReadOnlyCollection<Guid>? ids,
            string? code,
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Project query is not used when loading a timesheet page.");
    }

    private sealed class StubTimeEntries : ITimeEntryRepository
    {
        private readonly TimeEntry _entry;

        public StubTimeEntries(TimeEntry entry) => _entry = entry;

        public Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
            Task.FromResult<TimeEntry?>(_entry);

        public Task<PagedTimeEntries> GetPagedAsync(
            int year,
            int month,
            Guid? employeeId,
            Guid? projectId,
            int page,
            int pageSize,
            CancellationToken cancellationToken) =>
            Task.FromResult(new PagedTimeEntries
            {
                Items = [_entry],
                TotalCount = 1,
                TotalHours = _entry.Hours,
                TotalCost = 800
            });

        public Task<decimal> GetHoursForDayAsync(
            Guid employeeId,
            DateOnly date,
            Guid? excludeEntryId,
            CancellationToken cancellationToken) =>
            Task.FromResult(_entry.Hours);

        public Task<IReadOnlyDictionary<(Guid EmployeeId, DateOnly Date), decimal>> GetHoursByDayAsync(
            IReadOnlyCollection<(Guid EmployeeId, DateOnly Date)> keys,
            CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyDictionary<(Guid EmployeeId, DateOnly Date), decimal>>(
                new Dictionary<(Guid, DateOnly), decimal>
                {
                    [(_entry.EmployeeId, _entry.Date)] = _entry.Hours
                });

        public Task AddAsync(TimeEntry entry, CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task UpdateAsync(
            TimeEntry entry,
            int expectedVersion,
            CancellationToken cancellationToken) =>
            Task.CompletedTask;

        public Task DeleteAsync(Guid id, CancellationToken cancellationToken) =>
            Task.CompletedTask;
    }
}
