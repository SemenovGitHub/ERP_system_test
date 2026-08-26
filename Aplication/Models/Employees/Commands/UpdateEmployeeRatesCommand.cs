using Application.Models.Employees.Responses;
using MediatR;

namespace Application.Models.Employees.Commands;

public sealed class UpdateEmployeeRatesCommand : IRequest<EmployeeResponse>
{
    public Guid Id { get; set; }

    public IReadOnlyCollection<RateItem> Rates { get; set; } = [];
}

public sealed class RateItem
{
    public DateOnly From { get; set; }

    public decimal Value { get; set; }
}
