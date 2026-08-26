using MediatR;

namespace Application.Models.Projects.Queries;

public sealed class GetProjectsQuery : IRequest<PagedProjectsResponse>
{
    public IReadOnlyCollection<Guid>? Ids { get; set; }

    public string? Code { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
