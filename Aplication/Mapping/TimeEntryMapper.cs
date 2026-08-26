using Application.Models.TimeEntries.Responses;
using Domain.Employees;
using Domain.Projects;
using Domain.Rules;
using Domain.TimeEntries;

namespace Application.Mapping;

internal static class TimeEntryMapper
{
    public static TimeEntryResponse Map(
        TimeEntry entry,
        Employee employee,
        Project project,
        decimal hoursForDay)
    {
        var rate = RateResolver.Require(employee.Rates, entry.Date);

        return new TimeEntryResponse
        {
            Id = entry.Id,
            EmployeeId = entry.EmployeeId,
            ProjectId = entry.ProjectId,
            Date = entry.Date,
            EmployeeFullName = employee.FullName,
            ProjectCode = project.Code,
            ProjectName = project.Name,
            Hours = entry.Hours,
            Rate = rate,
            Cost = Money.Cost(entry.Hours, rate),
            Comment = entry.Comment,
            IsOvertime = HoursRules.IsOvertime(hoursForDay),
            Version = entry.Version
        };
    }
}
