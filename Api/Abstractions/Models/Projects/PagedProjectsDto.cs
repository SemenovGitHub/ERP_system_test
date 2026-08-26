namespace ERP.Abstractions.Models.Projects;

public sealed class PagedProjectsDto
{
    public IReadOnlyCollection<ProjectDto> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public long TotalCount { get; set; }
}
