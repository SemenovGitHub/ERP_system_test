using Application.Models.Employees.Queries;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Employees;

public sealed class GetEmployeesHandler
    : IRequestHandler<GetEmployeesQuery, PagedEmployeesResponse>
{
    private readonly IEmployeeRepository _employees;
    private readonly IMapper _mapper;

    public GetEmployeesHandler(IEmployeeRepository employees, IMapper mapper)
    {
        _employees = employees;
        _mapper = mapper;
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

        var response = _mapper.Map<PagedEmployeesResponse>(paged);
        response.Page = page;
        response.PageSize = pageSize;
        return response;
    }
}
