using Application.Interfaces;
using Application.Mapping;
using Application.Models.TimeEntries.Queries;
using Application.Models.TimeEntries.Responses;
using Domain.Exceptions;
using MediatR;

namespace Application.Handlers.TimeEntries;

public sealed class GetTimeEntriesHandler
    : IRequestHandler<GetTimeEntriesQuery, PagedTimeEntriesResponse>
{
    private readonly ITimeEntryRepository _timeEntries;
    private readonly IEmployeeRepository _employees;
    private readonly IProjectRepository _projects;

    public GetTimeEntriesHandler(
        ITimeEntryRepository timeEntries,
        IEmployeeRepository employees,
        IProjectRepository projects)
    {
        _timeEntries = timeEntries;
        _employees = employees;
        _projects = projects;
    }

    public async Task<PagedTimeEntriesResponse> Handle(
        GetTimeEntriesQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var paged = await _timeEntries.GetPagedAsync(
            request.Year,
            request.Month,
            request.EmployeeId,
            request.ProjectId,
            page,
            pageSize,
            cancellationToken);

        var employeeIds = paged.Items
            .Select(entry => entry.EmployeeId)
            .Distinct()
            .ToArray();
        var projectIds = paged.Items
            .Select(entry => entry.ProjectId)
            .Distinct()
            .ToArray();

        var employees = (await _employees.GetByIdsAsync(employeeIds, cancellationToken))
            .ToDictionary(employee => employee.Id);
        var projects = (await _projects.GetByIdsAsync(projectIds, cancellationToken))
            .ToDictionary(project => project.Id);

        var dayKeys = paged.Items
            .Select(entry => (entry.EmployeeId, entry.Date))
            .Distinct()
            .ToArray();
        var hoursByDay = await _timeEntries.GetHoursByDayAsync(dayKeys, cancellationToken);

        var items = paged.Items.Select(entry =>
        {
            var employee = employees.GetValueOrDefault(entry.EmployeeId)
                ?? throw new BusinessException(ErrorCodes.NotFound, "Сотрудник записи не найден.", 404);
            var project = projects.GetValueOrDefault(entry.ProjectId)
                ?? throw new BusinessException(ErrorCodes.NotFound, "Проект записи не найден.", 404);
            hoursByDay.TryGetValue((entry.EmployeeId, entry.Date), out var hoursForDay);
            return TimeEntryMapper.Map(entry, employee, project, hoursForDay);
        }).ToList();

        return new PagedTimeEntriesResponse
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = paged.TotalCount,
            TotalHours = paged.TotalHours,
            TotalCost = paged.TotalCost
        };
    }
}
