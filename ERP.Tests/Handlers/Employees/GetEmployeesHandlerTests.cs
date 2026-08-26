using Application.Handlers.Employees;
using Application.Interfaces;
using Application.Models.Employees.Queries;
using Domain.Employees;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Handlers.Employees;

public sealed class GetEmployeesHandlerTests : HandlerTestBase
{
    private readonly Mock<IEmployeeRepository> _employees;
    private readonly GetEmployeesHandler _handler;

    public GetEmployeesHandlerTests()
    {
        _employees = RegisterMock<IEmployeeRepository>();
        _handler = CreateHandler<GetEmployeesHandler>();
    }

    [Fact]
    public async Task Handle_WhenEmployeesExist_ReturnsMappedPage()
    {
        var employee = TestData.Employee();
        _employees
            .Setup(x => x.QueryAsync(null, "Инженерный", 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Employee>
            {
                Items = [employee],
                TotalCount = 1
            });

        var result = await _handler.Handle(
            new GetEmployeesQuery { Department = "Инженерный", Page = 1, PageSize = 10 },
            CancellationToken.None);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
        result.TotalCount.Should().Be(1);
        result.Items.Should().ContainSingle();
        result.Items.First().Id.Should().Be(employee.Id);
        result.Items.First().FullName.Should().Be(employee.FullName);
        result.Items.First().Department.Should().Be(employee.Department);
        result.Items.First().Rates.Should().ContainSingle(rate => rate.Value == 600);
    }

    [Fact]
    public async Task Handle_WhenPageSizeIsInvalid_QueriesWithDefaultPageSize()
    {
        _employees
            .Setup(x => x.QueryAsync(null, null, 1, 20, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResult<Employee> { Items = [], TotalCount = 0 });

        var result = await _handler.Handle(
            new GetEmployeesQuery { Page = 0, PageSize = 0 },
            CancellationToken.None);

        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
        _employees.Verify(
            x => x.QueryAsync(null, null, 1, 20, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
