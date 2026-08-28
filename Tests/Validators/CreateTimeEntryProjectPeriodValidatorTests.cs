using Domain.Exceptions;
using Domain.Interfaces;
using Domain.Models;
using Domain.Validators;
using ERP.Tests;
using FluentAssertions;
using Moq;

namespace Tests.Validators;

public sealed class CreateTimeEntryProjectPeriodValidatorTests
{
    private readonly Guid _employeeId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private readonly Guid _projectId = Guid.Parse("33333333-3333-3333-3333-333333333333");

    [Fact]
    public async Task Create_WhenDateIsInsidePeriod_DoesNotThrow()
    {
        var act = () => Validator().ValidateAsync(Entry(new DateOnly(2026, 6, 1)));

        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Create_WhenDateEqualsStartOrEnd_DoesNotThrow()
    {
        var project = TestData.Project(_projectId);
        var validator = Validator(project);

        await validator.Invoking(v => v.ValidateAsync(Entry(project.StartDate))).Should().NotThrowAsync();
        await validator.Invoking(v => v.ValidateAsync(Entry(project.EndDate!.Value))).Should().NotThrowAsync();
    }

    [Fact]
    public async Task Create_WhenDateIsBeforeStart_ThrowsProjectDateOutOfRange()
    {
        var project = TestData.Project(_projectId);
        var date = project.StartDate.AddDays(-1);

        var act = () => Validator(project).ValidateAsync(Entry(date));

        var exception = (await act.Should().ThrowAsync<BusinessException>()).Which;
        exception.Code.Should().Be(ErrorCodes.ProjectDateOutOfRange);
        exception.Message.Should().Be(
            $"Дата записи {date:dd.MM.yyyy} раньше начала проекта {project.Code} ({project.StartDate:dd.MM.yyyy}).");
    }

    [Fact]
    public async Task Create_WhenDateIsAfterEnd_ThrowsProjectDateOutOfRange()
    {
        var project = TestData.Project(_projectId);
        var date = project.EndDate!.Value.AddDays(1);

        var act = () => Validator(project).ValidateAsync(Entry(date));

        var exception = (await act.Should().ThrowAsync<BusinessException>()).Which;
        exception.Code.Should().Be(ErrorCodes.ProjectDateOutOfRange);
        exception.Message.Should().Be(
            $"Дата записи {date:dd.MM.yyyy} позже окончания проекта {project.Code} ({project.EndDate:dd.MM.yyyy}).");
    }

    [Fact]
    public async Task Create_WhenProjectHasNoEndDate_AllowsDateAfterStart()
    {
        var project = new ProjectModel
        {
            Id = _projectId,
            Code = "П-001",
            Name = "Реконструкция цеха",
            Budget = 20000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = null
        };

        var act = () => Validator(project).ValidateAsync(Entry(new DateOnly(2030, 1, 1)));

        await act.Should().NotThrowAsync();
    }

    private ICreateTimeEntryValidator Validator(ProjectModel? project = null)
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
            .ReturnsAsync(project ?? TestData.Project(_projectId));

        return TestContainer.Resolve<ICreateTimeEntryValidator>(
            employeeRepositoryMock: employeeRepositoryMock,
            projectRepositoryMock: projectRepositoryMock);
    }

    private TimeEntryModel Entry(DateOnly date) =>
        new()
        {
            EmployeeId = _employeeId,
            ProjectId = _projectId,
            Date = date,
            Hours = 8
        };
}
