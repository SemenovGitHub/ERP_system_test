namespace ERP.Abstractions.Models.TimeEntries;

public sealed class TimeEntryDto
{
    public Guid Id { get; set; }

    public string EmployeeFullName { get; set; } = null!;

    public string ProjectCode { get; set; } = null!;

    public decimal Hours { get; set; }

    public decimal Rate { get; set; }

    public decimal Cost { get; set; }
}