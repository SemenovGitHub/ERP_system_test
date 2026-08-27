using Application.Interfaces;
using Application.Models.TimeEntries.Commands;
using Application.Validators;
using Domain.Employees;
using Domain.Exceptions;
using Domain.TimeEntries;
using ERP.Tests;
using FluentAssertions;
using Moq;

namespace Tests.Validators;

public sealed class TimeEntryClosedPeriodValidatorTests
{
    private readonly Guid _employeeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");
    private readonly Guid _entryId = Guid.Parse("55555555-5555-5555-5555-555555555555");

    [Fact]
    public async Task Create_WhenPeriodIsOpen_DoesNotThrow()
    {
        var validator = CreateValidator(closed: false);

        var act = () => validator.ValidateAsync(CreateCommand());

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Create_WhenPeriodIsClosed_ThrowsClosedPeriod()
    {
        var validator = CreateValidator(closed: true);

        var act = () => validator.ValidateAsync(CreateCommand());

        var exception = (await act.Should().ThrowAsync<BusinessException>()).Which;
        exception.Code.Should().Be(ErrorCodes.ClosedPeriod);
        exception.StatusCode.Should().Be(409);
        exception.Message.Should().Be("Период 03.2026 закрыт. Создавать, изменять и удалять записи нельзя.");
    }

    [Fact]
    public async Task Delete_WhenPeriodIsClosed_ThrowsClosedPeriod()
    {
        var timeEntryRepositoryMock = new Mock<ITimeEntryRepository>();
        timeEntryRepositoryMock
            .Setup(repository => repository.GetByIdAsync(_entryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TimeEntry
            {
                Id = _entryId,
                EmployeeId = _employeeId,
                ProjectId = _projectId,
                Date = new DateOnly(2026, 3, 15),
                Hours = 8
            });

        var periodRepositoryMock = new Mock<IPeriodRepository>();
        periodRepositoryMock
            .Setup(repository => repository.IsClosedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var validator = TestContainer.Resolve<DeleteTimeEntryCommand>(
            timeEntryRepositoryMock: timeEntryRepositoryMock,
            periodRepositoryMock: periodRepositoryMock);

        var act = () => validator.ValidateAsync(new DeleteTimeEntryCommand { Id = _entryId });

        var exception = (await act.Should().ThrowAsync<BusinessException>()).Which;
        exception.Code.Should().Be(ErrorCodes.ClosedPeriod);
        exception.StatusCode.Should().Be(409);
    }

    private IDomainValidator<CreateTimeEntryCommand> CreateValidator(bool closed)
    {
        var employeeRepositoryMock = new Mock<IEmployeeRepository>();
        employeeRepositoryMock
            .Setup(repository => repository.GetByIdAsync(_employeeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Employee(
                _employeeId,
                [new Rate { From = new DateOnly(2026, 1, 1), Value = 500 }]));

        var projectRepositoryMock = new Mock<IProjectRepository>();
        projectRepositoryMock
            .Setup(repository => repository.GetByIdAsync(_projectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(TestData.Project(_projectId));

        var periodRepositoryMock = new Mock<IPeriodRepository>();
        periodRepositoryMock
            .Setup(repository => repository.IsClosedAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(closed);

        return TestContainer.Resolve<CreateTimeEntryCommand>(
            employeeRepositoryMock: employeeRepositoryMock,
            projectRepositoryMock: projectRepositoryMock,
            periodRepositoryMock: periodRepositoryMock);
    }

    private CreateTimeEntryCommand CreateCommand() =>
        new()
        {
            EmployeeId = _employeeId,
            ProjectId = _projectId,
            Date = new DateOnly(2026, 3, 15),
            Hours = 8
        };
}
