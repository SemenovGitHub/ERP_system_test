namespace ERP.Abstractions.Models.Reports;

public sealed class ProjectReportDto
{
    public IReadOnlyCollection<ProjectReportItemDto> Items { get; set; }
        = [];

    public ProjectReportTotalDto Total { get; set; } = null!;
}