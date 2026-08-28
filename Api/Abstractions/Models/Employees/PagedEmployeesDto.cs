using Api.Abstractions.Models.Employees;

namespace ERP.Abstractions.Models.Employees;

public sealed class PagedEmployeesDto
{
    public IReadOnlyCollection<EmployeeDto> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public long TotalCount { get; set; }
}
