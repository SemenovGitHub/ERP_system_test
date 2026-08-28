using Application.Interfaces;
using Application.Mapping;
using Application.Models.TimeEntries.Commands;
using Application.Models.TimeEntries.Responses;
using Application.Validators;
using Domain.Exceptions;
using Domain.Models;
using MediatR;

namespace Application.Handlers.TimeEntries;

public sealed class CreateTimeEntryHandler
    : IRequestHandler<CreateTimeEntryCommand, TimeEntryResponse>
{
    private readonly IEmployeeRepository _employees;
    private readonly IProjectRepository _projects;
    private readonly ITimeEntryRepository _timeEntries;
    private readonly IDomainValidator<CreateTimeEntryCommand> _validator;

    public CreateTimeEntryHandler(
        IEmployeeRepository employees,
        IProjectRepository projects,
        ITimeEntryRepository timeEntries,
        IDomainValidator<CreateTimeEntryCommand> validator)
    {
        _employees = employees;
        _projects = projects;
        _timeEntries = timeEntries;
        _validator = validator;
    }

    public async Task<TimeEntryResponse> Handle(
        CreateTimeEntryCommand request,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(request, cancellationToken);

        var employee = await RequireEmployee(request.EmployeeId, cancellationToken);
        var project = await RequireProject(request.ProjectId, cancellationToken);

        var hoursForDay = await _timeEntries.GetHoursForDayAsync(
            request.EmployeeId,
            request.Date,
            excludeEntryId: null,
            cancellationToken);

        var now = DateTime.UtcNow;
        var entry = new TimeEntryModel
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

    private async Task<EmployeeModel> RequireEmployee(Guid id, CancellationToken cancellationToken)
    {
        return await _employees.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Сотрудник не найден.", 404);
    }

    private async Task<ProjectModel> RequireProject(Guid id, CancellationToken cancellationToken)
    {
        return await _projects.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Проект не найден.", 404);
    }
}
