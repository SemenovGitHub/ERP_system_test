using Application.Interfaces;
using Application.Models.Employees.Commands;
using Application.Models.Employees.Responses;
using Application.Validators;
using Domain.Employees;
using Domain.Exceptions;
using MediatR;

namespace Application.Handlers.Employees;

public sealed class UpdateEmployeeRatesHandler
    : IRequestHandler<UpdateEmployeeRatesCommand, EmployeeResponse>
{
    private readonly IEmployeeRepository _employees;
    private readonly IDomainValidator<UpdateEmployeeRatesCommand> _validator;

    public UpdateEmployeeRatesHandler(
        IEmployeeRepository employees,
        IDomainValidator<UpdateEmployeeRatesCommand> validator)
    {
        _employees = employees;
        _validator = validator;
    }

    public async Task<EmployeeResponse> Handle(
        UpdateEmployeeRatesCommand request,
        CancellationToken cancellationToken)
    {
        await _validator.ValidateAsync(request, cancellationToken);

        var employee = await _employees.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new BusinessException(ErrorCodes.NotFound, "Сотрудник не найден.", 404);

        var rates = request.Rates
            .Select(rate => new Rate { From = rate.From, Value = rate.Value })
            .OrderBy(rate => rate.From)
            .ToList();

        await _employees.UpdateRatesAsync(employee.Id, rates, cancellationToken);

        return new EmployeeResponse
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Department = employee.Department,
            Rates = rates.Select(rate => new RateResponse
            {
                From = rate.From,
                Value = rate.Value
            }).ToList()
        };
    }
}
