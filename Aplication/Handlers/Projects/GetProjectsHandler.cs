using Domain.Interfaces;
using Application.Models.Projects.Queries;
using Application.Models.Projects.Responses;
using MediatR;

namespace Application.Handlers.Projects;

public sealed class GetProjectsHandler
    : IRequestHandler<GetProjectsQuery, PagedProjectsResponse>
{
    private readonly IProjectRepository _projects;

    public GetProjectsHandler(IProjectRepository projects)
    {
        _projects = projects;
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

        return new PagedProjectsResponse
        {
            Items = paged.Items.Select(project => new ProjectResponse
            {
                Id = project.Id,
                Code = project.Code,
                Name = project.Name,
                Budget = project.Budget,
                StartDate = project.StartDate,
                EndDate = project.EndDate
            }).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = paged.TotalCount
        };
    }
}
