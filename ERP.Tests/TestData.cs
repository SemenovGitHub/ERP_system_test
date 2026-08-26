using Domain.Projects;

namespace ERP.Tests;

internal static class TestData
{
    public static Project Project() =>
        new()
        {
            Id = Guid.NewGuid(),
            Code = "П-001",
            Name = "Реконструкция цеха",
            Budget = 20000,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31)
        };
}
