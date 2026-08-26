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

public sealed class UpdateTimeEntryHandler
    : IRequestHandler<UpdateTimeEntryCommand, TimeEntryResponse>
{
    private readonly IEmployeeRepository _employees;
    private readonly IProjectRepository _projects;
    private readonly IPeriodRepository _periods;
    private readonly ITimeEntryRepository _timeEntries;

    public UpdateTimeEntryHandler(
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
        UpdateTimeEntryCommand request,
        CancellationToken cancellationToken)
    {
        HoursRules.EnsureValidEntryHours(request.Hours);

        var existing = await _timeEntries.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Запись табеля не найдена.", 404);

        await EnsurePeriodOpen(existing.Date, cancellationToken);
        await EnsurePeriodOpen(request.Date, cancellationToken);

        var employee = await RequireEmployee(request.EmployeeId, cancellationToken);
        var project = await RequireProject(request.ProjectId, cancellationToken);

        ProjectPeriodRules.EnsureDateFits(project, request.Date);
        RateResolver.Require(employee.Rates, request.Date);

        var hoursForDay = await _timeEntries.GetHoursForDayAsync(
            request.EmployeeId,
            request.Date,
            excludeEntryId: request.Id,
            cancellationToken);
        HoursRules.EnsureDailyLimit(hoursForDay, request.Hours);

        var updated = new TimeEntry
        {
            Id = existing.Id,
            EmployeeId = request.EmployeeId,
            ProjectId = request.ProjectId,
            Date = request.Date,
            Hours = request.Hours,
            Comment = request.Comment,
            Version = existing.Version + 1,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        await _timeEntries.UpdateAsync(updated, request.Version, cancellationToken);

        return TimeEntryMapper.Map(updated, employee, project, hoursForDay + request.Hours);
    }

    private async Task EnsurePeriodOpen(DateOnly date, CancellationToken cancellationToken)
    {
        var isClosed = await _periods.IsClosedAsync(date.Year, date.Month, cancellationToken);
        ClosedPeriodRules.EnsureOpen(isClosed, date);
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
