using ERP.Domain.TimeEntries;

namespace ERP.Application.Abstractions.Repositories;

public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<PagedResult<TimeEntry>> GetByFilterAsync(
        int year,
        int month,
        Guid? employeeId = null,
        Guid? projectId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    
    Task<Guid> CreateAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default);
    
    Task UpdateAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default);
    
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}