using Application.Handlers.Projects;
using Application.Interfaces;
using Application.Models.Projects.Queries;
using Domain.Projects;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Handlers.Projects;

public sealed class GetProjectsHandlerTests : HandlerTestBase
{
    private readonly Mock<IProjectRepository> _projects;
    private readonly GetProjectsHandler _handler;

    public GetProjectsHandlerTests()
    {
        _projects = RegisterMock<IProjectRepository>();
        _handler = CreateHandler<GetProjectsHandler>();
    }

    [Fact]
    public async Task Handle_WhenProjectsExist_ReturnsMappedPage()
    {
        var project = TestData.Project();
        _projects
            .Setup(x => x.QueryAsync(null, "П-001", 1, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Project>
            {
                Items = [project],
                TotalCount = 1
            });

        var result = await _handler.Handle(
            new GetProjectsQuery { Code = "П-001", Page = 1, PageSize = 5 },
            CancellationToken.None);

        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Id.Should().Be(project.Id);
        result.Items.First().Code.Should().Be(project.Code);
        result.Items.First().Name.Should().Be(project.Name);
        result.Items.First().Budget.Should().Be(project.Budget);
        result.Items.First().StartDate.Should().Be(project.StartDate);
        result.Items.First().EndDate.Should().Be(project.EndDate);
    }

    [Fact]
    public async Task Handle_WhenPageIsInvalid_QueriesWithDefaultPagination()
    {
        _projects
            .Setup(x => x.QueryAsync(null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Project> { Items = [], TotalCount = 0 });

        var result = await _handler.Handle(
            new GetProjectsQuery { Page = -1, PageSize = 500 },
            CancellationToken.None);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        _projects.Verify(
            x => x.QueryAsync(null, null, 1, 20, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
