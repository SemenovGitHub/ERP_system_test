namespace ERP.Abstractions.Models.TimeEntries;

public sealed class CreateTimeEntryDto
{
    public Guid EmployeeId { get; set; }

    public Guid ProjectId { get; set; }

    public DateOnly Date { get; set; }

    public decimal Hours { get; set; }

    public string? Comment { get; set; }
}