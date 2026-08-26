using ERP.Application.Abstractions.Commands;
using ERP.Application.Abstractions.Repositories;
using ERP.Domain.Employees;
using ERP.Domain.Exceptions;
using ERP.Domain.Projects;
using ERP.Domain.Rules;
using ERP.Domain.TimeEntries;
using ERP.Infrastructure.Commands;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.Handlers;

/// <summary>
/// Handler for creating time entries with comprehensive business validation
/// 
/// Validation Flow:
/// 1. Input validation (handled by ValidationBehavior + FluentValidation)
/// 2. Business entity validation (in handler)
/// 3. Business rules validation (using Domain rules)
/// 4. Data persistence
/// </summary>
public sealed class CreateTimeEntryHandler : ICommandHandler<CreateTimeEntryCommand, CreateTimeEntryResponse>
{
    private readonly IEmployeeRepository _employees;
    private readonly IProjectRepository _projects;
    private readonly ITimeEntryRepository _timeEntries;
    private readonly ILogger<CreateTimeEntryHandler> _logger;

    public CreateTimeEntryHandler(
        IEmployeeRepository employees,
        IProjectRepository projects,
        ITimeEntryRepository timeEntries,
        ILogger<CreateTimeEntryHandler> logger)
    {
        _employees = employees;
        _projects = projects;
        _timeEntries = timeEntries;
        _logger = logger;
    }

    public async Task<CreateTimeEntryResponse> Handle(
        CreateTimeEntryCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug("Creating time entry for Employee {EmployeeId}, Project {ProjectId}, Date {Date}, Hours {Hours}",
            request.EmployeeId, request.ProjectId, request.Date, request.Hours);

        // Step 1: Validate entities exist
        var employee = await GetRequiredEmployeeAsync(request.EmployeeId, cancellationToken);
        var project = await GetRequiredProjectAsync(request.ProjectId, cancellationToken);

        // Step 2: Business rules validation
        await ValidateBusinessRulesAsync(request, employee, project, cancellationToken);

        // Step 3: Calculate derived values
        var rate = RateResolver.GetRateForDate(employee.Rates, request.Date);
        var cost = request.Hours * rate.Value;
        
        var currentHoursForDay = await _timeEntries.GetHoursForDayAsync(
            request.EmployeeId, 
            request.Date, 
            excludeEntryId: null, 
            cancellationToken);
        
        var totalHoursForDay = currentHoursForDay + request.Hours;
        var isOvertime = HoursRules.IsOvertime(totalHoursForDay);

        // Step 4: Create and save time entry
        var timeEntry = new TimeEntry
        {
            Id = Guid.NewGuid(),
            EmployeeId = request.EmployeeId,
            ProjectId = request.ProjectId,
            Date = request.Date,
            Hours = request.Hours,
            Comment = request.Comment,
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };

        var createdId = await _timeEntries.CreateAsync(timeEntry, cancellationToken);

        _logger.LogInformation("Time entry {TimeEntryId} created successfully for {Hours} hours", 
            createdId, request.Hours);

        return new CreateTimeEntryResponse(
            Id: createdId,
            EmployeeId: employee.Id,
            EmployeeFullName: employee.FullName,
            ProjectId: project.Id,
            ProjectCode: project.Code,
            Date: request.Date,
            Hours: request.Hours,
            Comment: request.Comment,
            Rate: rate.Value,
            Cost: cost,
            IsOvertime: isOvertime,
            TotalHoursForDay: totalHoursForDay
        );
    }

    private async Task<Employee> GetRequiredEmployeeAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var employee = await _employees.GetByIdAsync(employeeId, cancellationToken);
        if (employee == null)
        {
            throw new BusinessException(
                ErrorCodes.NotFound, 
                $"Сотрудник с ID {employeeId} не найден");
        }
        return employee;
    }

    private async Task<Project> GetRequiredProjectAsync(Guid projectId, CancellationToken cancellationToken)
    {
        var project = await _projects.GetByIdAsync(projectId, cancellationToken);
        if (project == null)
        {
            throw new BusinessException(
                ErrorCodes.NotFound, 
                $"Проект с ID {projectId} не найден");
        }
        return project;
    }

    private async Task ValidateBusinessRulesAsync(
        CreateTimeEntryCommand request,
        Employee employee,
        Project project,
        CancellationToken cancellationToken)
    {
        // 1. Validate hours format (already done by FluentValidation, but let's be explicit)
        HoursRules.EnsureValidEntryHours(request.Hours);

        // 2. Validate project date range
        ProjectPeriodRules.EnsureDateFits(project, request.Date);

        // 3. Validate employee has rate for the date
        RateResolver.Require(employee.Rates, request.Date);

        // 4. Validate daily hours limit
        var currentHoursForDay = await _timeEntries.GetHoursForDayAsync(
            request.EmployeeId, 
            request.Date, 
            excludeEntryId: null, 
            cancellationToken);
        
        HoursRules.EnsureDailyLimit(currentHoursForDay, request.Hours);

        // 5. Check if period is closed (if we had periods functionality)
        // TODO: Add period validation when implementing periods
        // var isClosed = await _periods.IsClosedAsync(request.Date.Year, request.Date.Month, cancellationToken);
        // ClosedPeriodRules.EnsureOpen(isClosed, request.Date);
    }
}

/// <summary>
/// Extension methods for TimeEntryRepository to support business operations
/// </summary>
public static class TimeEntryRepositoryExtensions
{
    /// <summary>
    /// Gets total hours logged for an employee on a specific date
    /// </summary>
    public static async Task<decimal> GetHoursForDayAsync(
        this ITimeEntryRepository repository,
        Guid employeeId,
        DateOnly date,
        Guid? excludeEntryId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Implement in concrete repository
        // This is a placeholder - should be implemented in MongoDB repository
        return 0m;
    }
}