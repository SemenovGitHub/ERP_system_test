using Domain.Models;

namespace ERP.Tests;

internal static class TestData
{
    public static EmployeeModel Employee(Guid id, IReadOnlyList<RateModel> rates) =>
        new()
        {
            Id = id,
            FullName = "Иванов И. И.",
            Department = "Проектный",
            Rates = rates
        };

    public static ProjectModel Project(Guid? id = null) =>
        new()
        {
            Id = id ?? Guid.NewGuid(),
            Code = "П-001",
            Name = "Реконструкция цеха",
            Budget = 20000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };
}
