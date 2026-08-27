using Application.Interfaces;
using Application.Models.Reports.Queries;
using Application.Models.Reports.Responses;
using Application.Validators;
using MediatR;

namespace Application.Handlers.Reports;

public sealed class GetProjectsReportHandler
    : IRequestHandler<GetProjectsReportQuery, ProjectReportResponse>
{
    private const decimal RiskThresholdPercent = 80m;

    private readonly IProjectReportRepository _reports;

    public GetProjectsReportHandler(IProjectReportRepository reports)
    {
        _reports = reports;
    }

    public async Task<ProjectReportResponse> Handle(
        GetProjectsReportQuery request,
        CancellationToken cancellationToken)
    {
        var rows = await _reports.GetByMonthAsync(request.Year, request.Month, cancellationToken);

        var items = rows.Select(row =>
        {
            var percent = row.Budget == 0
                ? 0
                : MoneyValidator.Round(row.Cost / row.Budget * 100);
            var overBudget = row.Cost > row.Budget;

            return new ProjectReportItemResponse
            {
                ProjectId = row.ProjectId,
                ProjectCode = row.ProjectCode,
                ProjectName = row.ProjectName,
                Hours = row.Hours,
                Cost = row.Cost,
                Budget = row.Budget,
                BudgetUsagePercent = percent,
                IsOverBudget = overBudget,
                IsRisk = overBudget || percent > RiskThresholdPercent
            };
        }).ToList();

        return new ProjectReportResponse
        {
            Items = items,
            Total = new ProjectReportTotalResponse
            {
                Hours = items.Sum(item => item.Hours),
                Cost = items.Sum(item => item.Cost)
            }
        };
    }
}
