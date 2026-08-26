using Application.Handlers.Reports;
using Application.Interfaces;
using Application.Models.Reports.Queries;

namespace ERP.Tests;

public class GetProjectsReportHandlerTests
{
    [Fact]
    public async Task Marks_zero_budget_project_with_cost_as_over_budget()
    {
        var handler = new GetProjectsReportHandler(new StubReports(
        [
            new ProjectReportRow
            {
                ProjectId = Guid.NewGuid(),
                ProjectCode = "ZERO",
                ProjectName = "Zero budget",
                Hours = 8,
                Cost = 4000,
                Budget = 0
            }
        ]));

        var result = await handler.Handle(
            new GetProjectsReportQuery { Year = 2026, Month = 8 },
            CancellationToken.None);

        var item = Assert.Single(result.Items);
        Assert.True(item.IsOverBudget);
        Assert.True(item.IsRisk);
    }

    private sealed class StubReports : IProjectReportRepository
    {
        private readonly IReadOnlyList<ProjectReportRow> _rows;

        public StubReports(IReadOnlyList<ProjectReportRow> rows) => _rows = rows;

        public Task<IReadOnlyList<ProjectReportRow>> GetByMonthAsync(
            int year,
            int month,
            CancellationToken cancellationToken) =>
            Task.FromResult(_rows);
    }
}
