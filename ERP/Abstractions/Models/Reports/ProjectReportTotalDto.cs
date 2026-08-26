namespace ERP.Abstractions.Models.Reports;

public sealed class ProjectReportTotalDto
{
    public decimal Hours { get; set; }

    public decimal Cost { get; set; }

    public decimal Budget { get; set; }

    public decimal BudgetUsagePercent { get; set; }

    public bool IsOverBudget { get; set; }

    public bool IsRisk { get; set; }
}