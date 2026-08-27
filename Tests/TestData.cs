using Domain.Employees;
using Domain.Projects;

namespace ERP.Tests;

internal static class TestData
{
    public static Employee Employee(Guid id, IReadOnlyList<Rate> rates) =>
        new()
        {
            Id = id,
            FullName = "Иванов И. И.",
            Department = "Проектный",
            Rates = rates
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
}

