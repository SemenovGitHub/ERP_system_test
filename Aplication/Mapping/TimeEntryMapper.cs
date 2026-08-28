using Application.Models.TimeEntries.Responses;
using Application.Validators;
using Application.Validators.TimeEntries;
using Domain.Models;

namespace Application.Mapping;

internal static class TimeEntryMapper
{
    public static TimeEntryResponse Map(
        TimeEntryModel entry,
        EmployeeModel employee,
        ProjectModel project,
        decimal hoursForDay)
    {
        var rate = TimeEntryConstraints.FindRate(employee.Rates, entry.Date)
            ?? throw new InvalidOperationException("На дату записи нет ставки.");

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
            Cost = MoneyValidator.Cost(entry.Hours, rate),
            Comment = entry.Comment,
            IsOvertime = TimeEntryConstraints.IsOvertime(hoursForDay),
            Version = entry.Version
        };
    }
}
