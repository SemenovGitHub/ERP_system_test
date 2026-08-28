namespace Domain.Interfaces;

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = [];

    public long TotalCount { get; init; }
}
