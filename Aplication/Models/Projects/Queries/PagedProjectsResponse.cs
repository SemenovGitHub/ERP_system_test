using Application.Models.Projects.Responses;

namespace Application.Models.Projects.Queries;

public sealed class PagedProjectsResponse
{
    public IReadOnlyCollection<ProjectResponse> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public long TotalCount { get; set; }
}
