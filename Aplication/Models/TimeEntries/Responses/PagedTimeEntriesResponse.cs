namespace Application.Models.TimeEntries.Responses;

public sealed class PagedTimeEntriesResponse
{
    public IReadOnlyCollection<TimeEntryResponse> Items { get; set; } = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public long TotalCount { get; set; }

    public decimal TotalHours { get; set; }

    public decimal TotalCost { get; set; }
}
