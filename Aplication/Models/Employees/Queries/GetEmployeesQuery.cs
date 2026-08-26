using MediatR;

namespace Application.Models.Employees.Queries;

public sealed class GetEmployeesQuery : IRequest<PagedEmployeesResponse>
{
    public IReadOnlyCollection<Guid>? Ids { get; set; }

    public string? Department { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
