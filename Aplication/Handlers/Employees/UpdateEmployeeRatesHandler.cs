using Application.Interfaces;
using Application.Models.Employees.Commands;
using Application.Models.Employees.Responses;
using Domain.Employees;
using Domain.Exceptions;
using MediatR;

namespace Application.Handlers.Employees;

public sealed class UpdateEmployeeRatesHandler
    : IRequestHandler<UpdateEmployeeRatesCommand, EmployeeResponse>
{
    private readonly IEmployeeRepository _employees;

    public UpdateEmployeeRatesHandler(IEmployeeRepository employees)
    {
        _employees = employees;
    }

    public async Task<EmployeeResponse> Handle(
        UpdateEmployeeRatesCommand request,
        CancellationToken cancellationToken)
    {
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
