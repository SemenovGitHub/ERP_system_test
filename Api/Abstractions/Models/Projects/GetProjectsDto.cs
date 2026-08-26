namespace ERP.Abstractions.Models.Projects;

public sealed class GetProjectsDto
{
    public IReadOnlyCollection<Guid>? Ids { get; set; }

    public string? Code { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}
