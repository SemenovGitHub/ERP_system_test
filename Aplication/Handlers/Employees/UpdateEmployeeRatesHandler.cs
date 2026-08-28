using Application.Models.Employees.Commands;
using Application.Models.Employees.Responses;
using AutoMapper;
using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Domain.Validators;
using MediatR;

namespace Application.Handlers.Employees;

public sealed class UpdateEmployeeRatesHandler
    : IRequestHandler<UpdateEmployeeRatesCommand, EmployeeResponse>
{
    private readonly IEmployeeRepository _employeesRepository;
    private readonly IDomainValidator<EmployeeModel> _validator;
    private readonly IMapper _mapper;

    public UpdateEmployeeRatesHandler(
        IEmployeeRepository employeesRepository,
        IDomainValidator<EmployeeModel> validator,
        IMapper mapper)
    {
        _employeesRepository = employeesRepository;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<EmployeeResponse> Handle(
        UpdateEmployeeRatesCommand request,
        CancellationToken cancellationToken)
    {
        var model = _mapper.Map<EmployeeModel>(request);
        await _validator.ValidateAsync(model, cancellationToken);

        var employee = await _employeesRepository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Сотрудник не найден.", 404);

        var rates = model.Rates
            .OrderBy(rate => rate.From)
            .ToList();

        await _employeesRepository.UpdateRatesAsync(employee.Id, rates, cancellationToken);

        employee.Rates = rates;
        return _mapper.Map<EmployeeResponse>(employee);
    }
}
