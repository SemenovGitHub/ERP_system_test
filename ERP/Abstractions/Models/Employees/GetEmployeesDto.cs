namespace ERP.Abstractions.Models.Employees;

public sealed class GetEmployeesDto
{
    public IReadOnlyCollection<Guid>? Ids { get; set; }

    public string? Department { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
