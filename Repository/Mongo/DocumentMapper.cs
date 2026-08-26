using Domain.Employees;
using Domain.Projects;
using Domain.TimeEntries;
using Repository.Documents;

namespace Repository.Mongo;

internal static class DocumentMapper
{
    public static DateTime ToUtcDate(DateOnly date) =>
        DateTime.SpecifyKind(date.ToDateTime(TimeOnly.MinValue), DateTimeKind.Utc);

    public static DateOnly ToDateOnly(DateTime date) =>
        DateOnly.FromDateTime(DateTime.SpecifyKind(date, DateTimeKind.Utc));

    public static Employee ToDomain(EmployeeDocument document) =>
        new()
        {
            Id = document.Id,
            FullName = document.FullName,
            Department = document.Department,
            Rates = document.Rates
                .Select(rate => new Rate
                {
                    From = ToDateOnly(rate.From),
                    Value = rate.Value
                })
                .ToList()
        };

    public static EmployeeDocument ToDocument(Employee employee) =>
        new()
        {
            Id = employee.Id,
            FullName = employee.FullName,
            Department = employee.Department,
            Rates = employee.Rates
                .Select(rate => new RateDocument
                {
                    From = ToUtcDate(rate.From),
                    Value = rate.Value
                })
                .ToList()
        };

    public static Project ToDomain(ProjectDocument document) =>
        new()
        {
            Id = document.Id,
            Code = document.Code,
            Name = document.Name,
            Budget = document.Budget,
            StartDate = ToDateOnly(document.StartDate),
            EndDate = document.EndDate is { } end ? ToDateOnly(end) : null
        };

    public static TimeEntry ToDomain(TimeEntryDocument document) =>
        new()
        {
            Id = document.Id,
            EmployeeId = document.EmployeeId,
            ProjectId = document.ProjectId,
            Date = ToDateOnly(document.Date),
            Hours = document.Hours,
            Comment = document.Comment,
            Version = document.Version,
            CreatedAt = document.CreatedAt,
            UpdatedAt = document.UpdatedAt
        };

    public static TimeEntryDocument ToDocument(TimeEntry entry) =>
        new()
        {
            Id = entry.Id,
            EmployeeId = entry.EmployeeId,
            ProjectId = entry.ProjectId,
            Date = ToUtcDate(entry.Date),
            Hours = entry.Hours,
            Comment = entry.Comment,
            Version = entry.Version,
            CreatedAt = DateTime.SpecifyKind(entry.CreatedAt, DateTimeKind.Utc),
            UpdatedAt = entry.UpdatedAt is { } updated
                ? DateTime.SpecifyKind(updated, DateTimeKind.Utc)
                : null
        };
}
