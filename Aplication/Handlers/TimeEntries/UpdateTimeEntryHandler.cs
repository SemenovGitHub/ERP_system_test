using Application.Mapping;
using Application.Models.TimeEntries.Commands;
using Application.Models.TimeEntries.Responses;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Domain.Validators;
using MediatR;

namespace Application.Handlers.TimeEntries;

public sealed class UpdateTimeEntryHandler
    : IRequestHandler<UpdateTimeEntryCommand, TimeEntryResponse>
{
    private readonly IEmployeeRepository _employees;
    private readonly IProjectRepository _projects;
    private readonly ITimeEntryRepository _timeEntries;
    private readonly IUpdateTimeEntryValidator _validator;
    private readonly IMapper _mapper;

    public UpdateTimeEntryHandler(
        IEmployeeRepository employees,
        IProjectRepository projects,
        ITimeEntryRepository timeEntries,
        IUpdateTimeEntryValidator validator,
        IMapper mapper)
    {
        _employees = employees;
        _projects = projects;
        _timeEntries = timeEntries;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<TimeEntryResponse> Handle(
        UpdateTimeEntryCommand request,
        CancellationToken cancellationToken)
    {
        var model = _mapper.Map<TimeEntryModel>(request);
        await _validator.ValidateAsync(model, cancellationToken);

        var existing = await _timeEntries.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Запись табеля не найдена.", 404);

        var employee = await RequireEmployee(request.EmployeeId, cancellationToken);
        var project = await RequireProject(request.ProjectId, cancellationToken);

        var hoursForDay = await _timeEntries.GetHoursForDayAsync(
            request.EmployeeId,
            request.Date,
            excludeEntryId: request.Id,
            cancellationToken);

        var updated = new TimeEntryModel
        {
            Id = existing.Id,
            EmployeeId = model.EmployeeId,
            ProjectId = model.ProjectId,
            Date = model.Date,
            Hours = model.Hours,
            Comment = model.Comment,
            Version = existing.Version + 1,
            CreatedAt = existing.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        await _timeEntries.UpdateAsync(updated, request.Version, cancellationToken);

        return TimeEntryMapper.Map(updated, employee, project, hoursForDay + request.Hours);
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
