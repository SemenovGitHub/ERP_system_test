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
    private readonly IEmployeeRepository _employees;
    private readonly IDomainValidator<EmployeeModel> _validator;
    private readonly IMapper _mapper;

    public UpdateEmployeeRatesHandler(
        IEmployeeRepository employees,
        IDomainValidator<EmployeeModel> validator,
        IMapper mapper)
    {
        _employees = employees;
        _validator = validator;
        _mapper = mapper;
    }

    public async Task<EmployeeResponse> Handle(
        UpdateEmployeeRatesCommand request,
        CancellationToken cancellationToken)
    {
        var model = _mapper.Map<EmployeeModel>(request);
        await _validator.ValidateAsync(model, cancellationToken);

        var employee = await _employees.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Сотрудник не найден.", 404);

        var rates = model.Rates
            .OrderBy(rate => rate.From)
            .ToList();

        await _employees.UpdateRatesAsync(employee.Id, rates, cancellationToken);

        return _mapper.Map<EmployeeResponse>(new EmployeeModel
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Department = employee.Department,
            Rates = rates
        });
    }
}
