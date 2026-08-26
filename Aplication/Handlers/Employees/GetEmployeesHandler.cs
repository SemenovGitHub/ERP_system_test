using Application.Interfaces;
using Application.Models.Employees.Queries;
using Application.Models.Employees.Responses;
using MediatR;

namespace Application.Handlers.Employees;

public sealed class GetEmployeesHandler
    : IRequestHandler<GetEmployeesQuery, PagedEmployeesResponse>
{
    private readonly IEmployeeRepository _employees;

    public GetEmployeesHandler(IEmployeeRepository employees)
    {
        _employees = employees;
    }

    public async Task<PagedEmployeesResponse> Handle(
        GetEmployeesQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var paged = await _employees.QueryAsync(
            request.Ids,
            request.Department,
            page,
            pageSize,
            cancellationToken);

        return new PagedEmployeesResponse
        {
            Items = paged.Items.Select(employee => new EmployeeResponse
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Department = employee.Department,
                Rates = employee.Rates.Select(rate => new RateResponse
                {
                    From = rate.From,
                    Value = rate.Value
                }).ToList()
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = paged.TotalCount
        };
    }
}
