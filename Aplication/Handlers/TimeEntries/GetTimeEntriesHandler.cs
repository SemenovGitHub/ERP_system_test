using Application.Models.TimeEntries.Queries;
using Application.Models.TimeEntries.Responses;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Validators;
using Domain.Validators.TimeEntries;
using MediatR;

namespace Application.Handlers.TimeEntries;

public sealed class GetTimeEntriesHandler
    : IRequestHandler<GetTimeEntriesQuery, PagedTimeEntriesResponse>
{
    private readonly ITimeEntryRepository _timeEntriesRepository;
    private readonly IEmployeeRepository _employeesRepository;
    private readonly IProjectRepository _projectsRepository;
    private readonly IMapper _mapper;

    public GetTimeEntriesHandler(
        ITimeEntryRepository timeEntriesRepository,
        IEmployeeRepository employeesRepository,
        IProjectRepository projectsRepository,
        IMapper mapper)
    {
        _timeEntriesRepository = timeEntriesRepository;
        _employeesRepository = employeesRepository;
        _projectsRepository = projectsRepository;
        _mapper = mapper;
    }

    public async Task<PagedTimeEntriesResponse> Handle(
        GetTimeEntriesQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var paged = await _timeEntriesRepository.GetPagedAsync(
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

        var employees = (await _employeesRepository.GetByIdsAsync(employeeIds, cancellationToken))
            .ToDictionary(employee => employee.Id);
        var projects = (await _projectsRepository.GetByIdsAsync(projectIds, cancellationToken))
            .ToDictionary(project => project.Id);

        var dayKeys = paged.Items
            .Select(entry => (entry.EmployeeId, entry.Date))
            .Distinct()
            .ToArray();
        var hoursByDay = await _timeEntriesRepository.GetHoursByDayAsync(dayKeys, cancellationToken);

        var items = paged.Items.Select(entry =>
        {
            var employee = employees.GetValueOrDefault(entry.EmployeeId)
                           ?? throw new BusinessException(ErrorCodes.NotFound, "Сотрудник записи не найден.", 404);
            var project = projects.GetValueOrDefault(entry.ProjectId)
                          ?? throw new BusinessException(ErrorCodes.NotFound, "Проект записи не найден.", 404);
            hoursByDay.TryGetValue((entry.EmployeeId, entry.Date), out var hoursForDay);

            var response = _mapper.Map<TimeEntryResponse>(entry);
            var rate = TimeEntryConstraints.FindRate(employee.Rates, entry.Date)
                ?? throw new InvalidOperationException("На дату записи нет ставки.");
            response.EmployeeFullName = employee.FullName;
            response.ProjectCode = project.Code;
            response.ProjectName = project.Name;
            response.Rate = rate;
            response.Cost = MoneyValidator.Cost(entry.Hours, rate);
            response.IsOvertime = TimeEntryConstraints.IsOvertime(hoursForDay);
            return response;
        }).ToList();

        var pagedResponse = _mapper.Map<PagedTimeEntriesResponse>(paged);
        pagedResponse.Items = items;
        pagedResponse.Page = page;
        pagedResponse.PageSize = pageSize;
        return pagedResponse;
    }
}
