using Application.Interfaces;
using Application.Models.Reports.Queries;
using Application.Models.Reports.Responses;
using Domain.Rules;
using MediatR;

namespace Application.Handlers.Reports;

public sealed class GetProjectsReportHandler
    : IRequestHandler<GetProjectsReportQuery, ProjectReportResponse>
{
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

        var items = rows.Select(row => new ProjectReportItemResponse
        {
            ProjectId = row.ProjectId,
            ProjectCode = row.ProjectCode,
            ProjectName = row.ProjectName,
            Hours = row.Hours,
            Cost = row.Cost,
            Budget = row.Budget,
            BudgetUsagePercent = BudgetRules.UsagePercent(row.Cost, row.Budget),
            IsOverBudget = BudgetRules.IsOverBudget(row.Cost, row.Budget),
            IsRisk = BudgetRules.IsRisk(row.Cost, row.Budget)
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
