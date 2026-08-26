namespace ERP.Abstractions.Models.TimeEntries;

public sealed class TimeEntryDto
{
    public Guid Id { get; set; }

    public string EmployeeFullName { get; set; } = null!;

    public string ProjectName { get; set; } = null!;
    
    public DateTime Date { get; set; }

    public decimal Hours { get; set; }

    public string? Comment { get; set; }
}