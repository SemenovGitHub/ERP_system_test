using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Domain.Validators;
using ERP.Tests;
using FluentAssertions;
using Moq;

namespace Tests.Validators;

public sealed class CreateTimeEntryHoursValidatorTests
{
    private readonly Guid _employeeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task Create_WhenTotalEquals24_DoesNotThrow()
    {
        var validator = Validator(hoursAlready: 16);

        var act = () => validator.ValidateAsync(Entry(8));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Create_WhenTotalExceeds24_ThrowsDailyHoursLimit()
    {
        var validator = Validator(hoursAlready: 16);

        var act = () => validator.ValidateAsync(Entry(10));

        var exception = (await act.Should().ThrowAsync<BusinessException>()).Which;
        exception.Code.Should().Be(ErrorCodes.DailyHoursLimit);
        exception.Message.Should().Be(
            "Суммарно у сотрудника за день не может быть больше 24 часов. Уже учтено 16, попытка добавить 10 (итого 26).");
    }

    private ICreateTimeEntryValidator Validator(decimal hoursAlready)
    {
        var employeeRepositoryMock = new Mock<IEmployeeRepository>();
        employeeRepositoryMock
            .Setup(repository => repository.GetByIdAsync(_employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Employee(
                _employeeId,
                [new RateModel { From = new DateOnly(2026, 1, 1), Value = 500 }]));

        var projectRepositoryMock = new Mock<IProjectRepository>();
        projectRepositoryMock
            .Setup(repository => repository.GetByIdAsync(_projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Project(_projectId));

        var timeEntryRepositoryMock = new Mock<ITimeEntryRepository>();
        timeEntryRepositoryMock
            .Setup(repository => repository.GetHoursForDayAsync(
                _employeeId,
                It.IsAny<DateOnly>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(hoursAlready);

        return TestContainer.Resolve<ICreateTimeEntryValidator>(
            employeeRepositoryMock: employeeRepositoryMock,
            projectRepositoryMock: projectRepositoryMock,
            timeEntryRepositoryMock: timeEntryRepositoryMock);
    }

    private TimeEntryModel Entry(decimal hours) =>
        new()
        {
            EmployeeId = _employeeId,
            ProjectId = _projectId,
            Date = new DateOnly(2026, 3, 15),
            Hours = hours
        };
}
