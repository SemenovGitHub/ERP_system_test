namespace Application.Interfaces;

public sealed class ProjectReportRow
{
    public Guid ProjectId { get; init; }

    public string ProjectCode { get; init; } = null!;

    public string ProjectName { get; init; } = null!;

    public decimal Hours { get; init; }

    public decimal Cost { get; init; }

    public decimal Budget { get; init; }
}
