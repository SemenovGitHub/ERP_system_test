namespace Application.Models.Reports.Responses;

public sealed class ProjectReportItemResponse
{
    public Guid ProjectId { get; set; }

    public string ProjectCode { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public decimal Hours { get; set; }

    public decimal Cost { get; set; }

    public decimal Budget { get; set; }

    public decimal BudgetUsagePercent { get; set; }

    public bool IsOverBudget { get; set; }

    public bool IsRisk { get; set; }
}
