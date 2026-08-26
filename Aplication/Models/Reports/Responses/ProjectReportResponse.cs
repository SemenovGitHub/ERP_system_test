namespace Application.Models.Reports.Responses;

public sealed class ProjectReportResponse
{
    public IReadOnlyCollection<ProjectReportItemResponse> Items { get; set; } = [];

    public ProjectReportTotalResponse Total { get; set; } = null!;
}
