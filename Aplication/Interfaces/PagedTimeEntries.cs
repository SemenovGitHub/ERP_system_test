using Domain.TimeEntries;

namespace Application.Interfaces;

public sealed class PagedTimeEntries
{
    public IReadOnlyList<TimeEntry> Items { get; init; } = [];

    public long TotalCount { get; init; }

    public decimal TotalHours { get; init; }

    public decimal TotalCost { get; init; }
}
