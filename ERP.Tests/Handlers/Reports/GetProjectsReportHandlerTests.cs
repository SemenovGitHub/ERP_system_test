using Application.Handlers.Reports;
using Application.Interfaces;
using Application.Models.Reports.Queries;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Handlers.Reports;

public sealed class GetProjectsReportHandlerTests : HandlerTestBase
{
    private readonly Mock<IProjectReportRepository> _reports;
    private readonly GetProjectsReportHandler _handler;

    public GetProjectsReportHandlerTests()
    {
        _reports = RegisterMock<IProjectReportRepository>();
        _handler = CreateHandler<GetProjectsReportHandler>();
    }

    [Fact]
    public async Task Handle_WhenRowsExist_ReturnsMappedReportWithTotals()
    {
        var projectId = Guid.NewGuid();
        _reports
            .Setup(x => x.GetByMonthAsync(2026, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ProjectReportRow
                {
                    ProjectId = projectId,
                    ProjectCode = "П-001",
                    ProjectName = "Реконструкция цеха",
                    Hours = 80,
                    Cost = 8000,
                    Budget = 20000
                }
            ]);

        var result = await _handler.Handle(
            new GetProjectsReportQuery { Year = 2026, Month = 3 },
            CancellationToken.None);

        result.Items.Should().ContainSingle();
        var item = result.Items.First();
        item.ProjectId.Should().Be(projectId);
        item.ProjectCode.Should().Be("П-001");
        item.Hours.Should().Be(80);
        item.Cost.Should().Be(8000);
        item.Budget.Should().Be(20000);
        item.IsOverBudget.Should().BeFalse();
        result.Total.Hours.Should().Be(80);
        result.Total.Cost.Should().Be(8000);
    }

    [Fact]
    public async Task Handle_WhenCostExceedsBudget_MarksItemAsOverBudget()
    {
        _reports
            .Setup(x => x.GetByMonthAsync(2026, 3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(
            [
                new ProjectReportRow
                {
                    ProjectId = Guid.NewGuid(),
                    ProjectCode = "П-002",
                    ProjectName = "Инженерные сети",
                    Hours = 100,
                    Cost = 12000,
                    Budget = 5000
                }
            ]);

        var result = await _handler.Handle(
            new GetProjectsReportQuery { Year = 2026, Month = 3 },
            CancellationToken.None);

        var item = result.Items.Should().ContainSingle().Subject;
        item.IsOverBudget.Should().BeTrue();
        item.IsRisk.Should().BeTrue();
        result.Total.Cost.Should().Be(12000);
    }
}
