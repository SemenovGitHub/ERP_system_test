using Application.Interfaces;
using Application.Mapping;
using Application.Models.TimeEntries.Commands;
using Application.Models.TimeEntries.Responses;
using Domain.Employees;
using Domain.Exceptions;
using Domain.Projects;
using Domain.Rules;
using Domain.TimeEntries;
using MediatR;

namespace Application.Handlers.TimeEntries;

public sealed class CreateTimeEntryHandler
    : IRequestHandler<CreateTimeEntryCommand, TimeEntryResponse>
{
    private readonly IEmployeeRepository _employees;
    private readonly IProjectRepository _projects;
    private readonly IPeriodRepository _periods;
    private readonly ITimeEntryRepository _timeEntries;

    public CreateTimeEntryHandler(
        IEmployeeRepository employees,
        IProjectRepository projects,
        IPeriodRepository periods,
        ITimeEntryRepository timeEntries)
    {
        _employees = employees;
        _projects = projects;
        _periods = periods;
        _timeEntries = timeEntries;
    }

    public async Task<TimeEntryResponse> Handle(
        CreateTimeEntryCommand request,
        CancellationToken cancellationToken)
    {
        HoursRules.EnsureValidEntryHours(request.Hours);

        var employee = await RequireEmployee(request.EmployeeId, cancellationToken);
        var project = await RequireProject(request.ProjectId, cancellationToken);

        var isClosed = await _periods.IsClosedAsync(
            request.Date.Year,
            request.Date.Month,
            cancellationToken);
        ClosedPeriodRules.EnsureOpen(isClosed, request.Date);
        ProjectPeriodRules.EnsureDateFits(project, request.Date);
        RateResolver.Require(employee.Rates, request.Date);

        var hoursForDay = await _timeEntries.GetHoursForDayAsync(
            request.EmployeeId,
            request.Date,
            excludeEntryId: null,
            cancellationToken);
        HoursRules.EnsureDailyLimit(hoursForDay, request.Hours);

        var now = DateTime.UtcNow;
        var entry = new TimeEntry
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            ProjectId = request.ProjectId,
            Date = request.Date,
            Hours = request.Hours,
            Comment = request.Comment,
            Version = 1,
            CreatedAt = now
        };

        await _timeEntries.AddAsync(entry, cancellationToken);

        return TimeEntryMapper.Map(entry, employee, project, hoursForDay + request.Hours);
    }

    private async Task<Employee> RequireEmployee(Guid id, CancellationToken cancellationToken)
    {
        return await _employees.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Сотрудник не найден.", 404);
    }

    private async Task<Project> RequireProject(Guid id, CancellationToken cancellationToken)
    {
        return await _projects.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Проект не найден.", 404);
    }
}
