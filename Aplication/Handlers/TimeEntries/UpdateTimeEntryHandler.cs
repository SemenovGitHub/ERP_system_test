using Application.Models.TimeEntries.Commands;
using Application.Models.TimeEntries.Responses;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Domain.Validators;
using Domain.Validators.TimeEntries;
using MediatR;

namespace Application.Handlers.TimeEntries;

public sealed class UpdateTimeEntryHandler
    : IRequestHandler<UpdateTimeEntryCommand, TimeEntryResponse>
{
    private readonly IEmployeeRepository _employeesRepository;
    private readonly IProjectRepository _projectsRepository;
    private readonly ITimeEntryRepository _timeEntriesRepository;
    private readonly ITimeEntryValidator _validator;
    private readonly IMapper _mapper;

    public UpdateTimeEntryHandler(
        IEmployeeRepository employeesRepository,
        IProjectRepository projectsRepository,
        ITimeEntryRepository timeEntriesRepository,
        IEnumerable<ITimeEntryValidator> validators,
        IMapper mapper)
    {
        _employeesRepository = employeesRepository;
        _projectsRepository = projectsRepository;
        _timeEntriesRepository = timeEntriesRepository;
        _validator = validators
            .Single(validator => validator.Name == UpdateTimeEntryValidator.Name);
        _mapper = mapper;
    }

    public async Task<TimeEntryResponse> Handle(
        UpdateTimeEntryCommand request,
        CancellationToken cancellationToken)
    {
        var model = _mapper.Map<TimeEntryModel>(request);
        await _validator.ValidateAsync(model, cancellationToken).ThrowIfInvalidAsync();

        var existing = await _timeEntriesRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Запись табеля не найдена.", 404);

        var employee = await RequireEmployee(request.EmployeeId, cancellationToken);
        var project = await RequireProject(request.ProjectId, cancellationToken);

        var hoursForDay = await _timeEntriesRepository.GetHoursForDayAsync(
            request.EmployeeId,
            request.Date,
            excludeEntryId: request.Id,
            cancellationToken);

        model.Version = existing.Version + 1;
        model.CreatedAt = existing.CreatedAt;
        model.UpdatedAt = DateTime.UtcNow;

        await _timeEntriesRepository.UpdateAsync(model, request.Version, cancellationToken);

        var response = _mapper.Map<TimeEntryResponse>(model);
        var rate = TimeEntryConstraints.FindRate(employee.Rates, model.Date)
            ?? throw new InvalidOperationException("На дату записи нет ставки.");
        response.EmployeeFullName = employee.FullName;
        response.ProjectCode = project.Code;
        response.ProjectName = project.Name;
        response.Rate = rate;
        response.Cost = MoneyValidator.Cost(model.Hours, rate);
        response.IsOvertime = TimeEntryConstraints.IsOvertime(hoursForDay + request.Hours);
        return response;
    }

    private async Task<EmployeeModel> RequireEmployee(Guid id, CancellationToken cancellationToken)
    {
        return await _employeesRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Сотрудник не найден.", 404);
    }

    private async Task<ProjectModel> RequireProject(Guid id, CancellationToken cancellationToken)
    {
        return await _projectsRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Проект не найден.", 404);
    }
}
