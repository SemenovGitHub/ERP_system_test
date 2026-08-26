namespace ERP.Abstractions.Models.TimeEntries;

public sealed class TimeEntryDto
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid ProjectId { get; set; }

    public DateTime Date { get; set; }

    public string EmployeeFullName { get; set; } = null!;

    public string ProjectCode { get; set; } = null!;

    public string ProjectName { get; set; } = null!;

    public decimal Hours { get; set; }

    public decimal Rate { get; set; }

    public decimal Cost { get; set; }

    public string? Comment { get; set; }

    public bool IsOvertime { get; set; }

    public int Version { get; set; }
}
