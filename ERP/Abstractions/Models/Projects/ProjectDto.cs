namespace ERP.Abstractions.Models.Projects;

public sealed class ProjectDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;
}