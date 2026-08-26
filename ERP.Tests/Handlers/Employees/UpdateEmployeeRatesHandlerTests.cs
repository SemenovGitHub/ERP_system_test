using Application.Handlers.Employees;
using Application.Interfaces;
using Application.Models.Employees.Commands;
using Domain.Employees;
using Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace ERP.Tests.Handlers.Employees;

public sealed class UpdateEmployeeRatesHandlerTests : HandlerTestBase
{
    private readonly Mock<IEmployeeRepository> _employees;
    private readonly UpdateEmployeeRatesHandler _handler;

    public UpdateEmployeeRatesHandlerTests()
    {
        _employees = RegisterMock<IEmployeeRepository>();
        _handler = CreateHandler<UpdateEmployeeRatesHandler>();
    }

    [Fact]
    public async Task Handle_WhenEmployeeExists_UpdatesRatesAndReturnsEmployee()
    {
        var employee = TestData.Employee();
        var from = new DateOnly(2026, 4, 1);
        _employees
            .Setup(x => x.GetByIdAsync(employee.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(employee);
        _employees
            .Setup(x => x.UpdateRatesAsync(employee.Id, It.IsAny<IReadOnlyList<Rate>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _handler.Handle(
            new UpdateEmployeeRatesCommand
            {
                Id = employee.Id,
                Rates = [new RateItem { From = from, Value = 800 }]
            },
            CancellationToken.None);

        result.Id.Should().Be(employee.Id);
        result.FullName.Should().Be(employee.FullName);
        result.Rates.Should().ContainSingle(rate => rate.From == from && rate.Value == 800);
        _employees.Verify(
            x => x.UpdateRatesAsync(
                employee.Id,
                It.Is<IReadOnlyList<Rate>>(rates => rates.Count == 1 && rates[0].Value == 800),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenEmployeeDoesNotExist_ThrowsBusinessException()
    {
        var id = Guid.NewGuid();
        _employees
            .Setup(x => x.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Employee?)null);

        var act = () => _handler.Handle(
            new UpdateEmployeeRatesCommand
            {
                Id = id,
                Rates = [new RateItem { From = TestData.Date, Value = 800 }]
            },
            CancellationToken.None);

        var exception = await act.Should().ThrowAsync<BusinessException>();
        exception.Which.Code.Should().Be(ErrorCodes.NotFound);
        exception.Which.Message.Should().Be("Сотрудник не найден.");
        _employees.Verify(
            x => x.UpdateRatesAsync(It.IsAny<Guid>(), It.IsAny<IReadOnlyList<Rate>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
