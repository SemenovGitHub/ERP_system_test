namespace ERP.Abstractions.Models.TimeEntries;

public sealed class PagedTimeEntriesDto
{
    public IReadOnlyCollection<TimeEntryDto> Items { get; set; }
        = [];

    public int Page { get; set; }

    public int PageSize { get; set; }

    public long TotalCount { get; set; }
}