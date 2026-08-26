using Domain.Exceptions;
using Domain.Rules;
using FluentAssertions;

namespace ERP.Tests.Rules;

public sealed class ProjectPeriodRulesTests
{
    [Fact]
    public void EnsureDateFits_WhenDateIsInsidePeriod_DoesNotThrow()
    {
        var project = TestData.Project();

        var act = () => ProjectPeriodRules.EnsureDateFits(project, new DateOnly(2026, 6, 1));

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureDateFits_WhenDateEqualsStartOrEnd_DoesNotThrow()
    {
        var project = TestData.Project();

        var onStart = () => ProjectPeriodRules.EnsureDateFits(project, project.StartDate);
        var onEnd = () => ProjectPeriodRules.EnsureDateFits(project, project.EndDate!.Value);

        onStart.Should().NotThrow();
        onEnd.Should().NotThrow();
    }

    [Fact]
    public void EnsureDateFits_WhenDateIsBeforeStart_ThrowsProjectDateOutOfRange()
    {
        var project = TestData.Project();
        var date = project.StartDate.AddDays(-1);

        var act = () => ProjectPeriodRules.EnsureDateFits(project, date);

        var exception = act.Should().Throw<BusinessException>().Which;
        exception.Code.Should().Be(ErrorCodes.ProjectDateOutOfRange);
        exception.Message.Should().Be(
            $"Дата записи {date:dd.MM.yyyy} раньше начала проекта {project.Code} ({project.StartDate:dd.MM.yyyy}).");
    }

    [Fact]
    public void EnsureDateFits_WhenDateIsAfterEnd_ThrowsProjectDateOutOfRange()
    {
        var project = TestData.Project();
        var date = project.EndDate!.Value.AddDays(1);

        var act = () => ProjectPeriodRules.EnsureDateFits(project, date);

        var exception = act.Should().Throw<BusinessException>().Which;
        exception.Code.Should().Be(ErrorCodes.ProjectDateOutOfRange);
        exception.Message.Should().Be(
            $"Дата записи {date:dd.MM.yyyy} позже окончания проекта {project.Code} ({project.EndDate:dd.MM.yyyy}).");
    }

    [Fact]
    public void EnsureDateFits_WhenProjectHasNoEndDate_AllowsDateAfterStart()
    {
        var project = new Domain.Projects.Project
        {
            Id = Guid.NewGuid(),
            Code = "П-001",
            Name = "Реконструкция цеха",
            Budget = 20000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = null
        };

        var act = () => ProjectPeriodRules.EnsureDateFits(project, new DateOnly(2030, 1, 1));

        act.Should().NotThrow();
    }
}
