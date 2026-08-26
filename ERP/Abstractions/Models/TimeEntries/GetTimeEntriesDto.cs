namespace ERP.Abstractions.Models.TimeEntries;

public sealed class GetTimeEntriesDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public Guid? EmployeeId { get; set; }

    public Guid? ProjectId { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;
}