using Domain.Employees;
using Domain.Projects;
using Domain.TimeEntries;

namespace ERP.Tests;

internal static class TestData
{
    public static readonly DateOnly Date = new(2026, 3, 15);

    public static Employee Employee(Guid? id = null, DateOnly? rateFrom = null, decimal rate = 600) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            FullName = "Иванов Иван Иванович",
            Department = "Инженерный",
            Rates = [new Rate { From = rateFrom ?? new DateOnly(2026, 1, 1), Value = rate }]
        };

    public static Project Project(Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Code = "П-001",
            Name = "Реконструкция цеха",
            Budget = 20000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };

    public static TimeEntry TimeEntry(Guid employeeId, Guid projectId, Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            EmployeeId = employeeId,
            ProjectId = projectId,
            Date = Date,
            Hours = 8,
            Comment = "Работы на объекте",
            Version = 1,
            CreatedAt = DateTime.UtcNow
        };
}
