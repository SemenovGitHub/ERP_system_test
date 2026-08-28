using Application.Interfaces;
using Application.Models.TimeEntries.Commands;
using Application.Validators;
using Domain.Models;
using Domain.Exceptions;
using ERP.Tests;
using FluentAssertions;
using Moq;

namespace Tests.Validators;

public sealed class CreateTimeEntryCommandValidatorTests
{
    private readonly Guid _employeeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task Create_WhenSeveralRatesExist_AcceptsDateCoveredByLatestApplicableRate()
    {
        var validator = CreateValidator(rates:
        [
            new RateModel { From = new DateOnly(2026, 1, 1), Value = 500 },
            new RateModel { From = new DateOnly(2026, 3, 1), Value = 700 },
            new RateModel { From = new DateOnly(2026, 6, 1), Value = 900 }
        ]);

        var act = () => validator.ValidateAsync(Command(new DateOnly(2026, 4, 15)));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Create_WhenDateEqualsRateFrom_DoesNotThrow()
    {
        var validator = CreateValidator(rates:
        [
            new RateModel { From = new DateOnly(2026, 1, 1), Value = 500 },
            new RateModel { From = new DateOnly(2026, 3, 1), Value = 700 }
        ]);

        var act = () => validator.ValidateAsync(Command(new DateOnly(2026, 3, 1)));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Create_WhenNoRateCoversDate_ThrowsNoRate()
    {
        var validator = CreateValidator(rates:
        [
            new RateModel { From = new DateOnly(2026, 3, 1), Value = 700 }
        ]);

        var act = () => validator.ValidateAsync(Command(new DateOnly(2026, 2, 28)));

        var exception = (await act.Should().ThrowAsync<BusinessException>()).Which;
        exception.Code.Should().Be(ErrorCodes.NoRate);
        exception.Message.Should().Be("На дату записи у сотрудника нет ни одной ставки. Запись создать нельзя.");
    }

    private IDomainValidator<CreateTimeEntryCommand> CreateValidator(RateModel[] rates)
    {
        var employeeRepositoryMock = new Mock<IEmployeeRepository>();
        employeeRepositoryMock
            .Setup(repository => repository.GetByIdAsync(_employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Employee(_employeeId, rates));

        var projectRepositoryMock = new Mock<IProjectRepository>();
        projectRepositoryMock
            .Setup(repository => repository.GetByIdAsync(_projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Project(_projectId));

        return TestContainer.Resolve<CreateTimeEntryCommand>(
            employeeRepositoryMock: employeeRepositoryMock,
            projectRepositoryMock: projectRepositoryMock);
    }

    private CreateTimeEntryCommand Command(DateOnly date) =>
        new()
        {
            EmployeeId = _employeeId,
            ProjectId = _projectId,
            Date = date,
            Hours = 8
        };
}
