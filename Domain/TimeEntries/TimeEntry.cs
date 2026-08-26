namespace Domain.TimeEntries;

public sealed class TimeEntry
{
    public Guid Id { get; init; }

    public Guid EmployeeId { get; init; }

    public Guid ProjectId { get; init; }

    public DateOnly Date { get; init; }

    public decimal Hours { get; init; }

    public string? Comment { get; init; }

    public int Version { get; init; }

    public DateTime CreatedAt { get; init; }

    public DateTime? UpdatedAt { get; init; }
}
