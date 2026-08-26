namespace ERP.Abstractions.Models.Projects;

public sealed class ProjectDto
{
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;
    
    public decimal Budget { get; set; }
    
    public DateTime StartDate { get; set; }
    
    public DateTime? EndDate { get; set; }
}