using Application.Models.Employees.Responses;

namespace Application.Models.Employees.Queries;

public sealed class PagedEmployeesResponse
{
    public IReadOnlyCollection<EmployeeResponse> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public long TotalCount { get; set; }
}
