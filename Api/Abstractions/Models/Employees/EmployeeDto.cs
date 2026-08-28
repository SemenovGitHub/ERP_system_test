using ERP.Abstractions.Models.Employees;

namespace Api.Abstractions.Models.Employees;

public sealed class EmployeeDto
{
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Department { get; set; } = null!;

    public List<RateDto> Rates { get; set; } = [];
}
