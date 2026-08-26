namespace ERP.Abstractions.Models.Employees;

public sealed class UpdateEmployeeRatesDto
{
    public IReadOnlyCollection<RateDto> Rates { get; set; } = [];
}
