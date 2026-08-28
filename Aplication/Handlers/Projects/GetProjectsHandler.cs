using Application.Models.Projects.Queries;
using AutoMapper;
using Domain.Interfaces;
using MediatR;

namespace Application.Handlers.Projects;

public sealed class GetProjectsHandler
    : IRequestHandler<GetProjectsQuery, PagedProjectsResponse>
{
    private readonly IProjectRepository _projects;
    private readonly IMapper _mapper;

    public GetProjectsHandler(IProjectRepository projects, IMapper mapper)
    {
        _projects = projects;
        _mapper = mapper;
    }

    public async Task<PagedProjectsResponse> Handle(
        GetProjectsQuery request,
        CancellationToken cancellationToken)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var paged = await _projects.QueryAsync(
            request.Ids,
            request.Code,
            page,
            pageSize,
            cancellationToken);

        var response = _mapper.Map<PagedProjectsResponse>(paged);
        response.Page = page;
        response.PageSize = pageSize;
        return response;
    }
}
