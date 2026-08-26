using Domain.TimeEntries;

namespace Application.Interfaces;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<PagedTimeEntries> GetPagedAsync(
        int year,
        int month,
        Guid? employeeId,
        Guid? projectId,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<decimal> GetHoursForDayAsync(
        Guid employeeId,
        DateOnly date,
        Guid? excludeEntryId,
        CancellationToken cancellationToken);

    Task<IReadOnlyDictionary<(Guid EmployeeId, DateOnly Date), decimal>> GetHoursByDayAsync(
        IReadOnlyCollection<(Guid EmployeeId, DateOnly Date)> keys,
        CancellationToken cancellationToken);

    Task AddAsync(TimeEntry entry, CancellationToken cancellationToken);

    Task UpdateAsync(TimeEntry entry, int expectedVersion, CancellationToken cancellationToken);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
}
