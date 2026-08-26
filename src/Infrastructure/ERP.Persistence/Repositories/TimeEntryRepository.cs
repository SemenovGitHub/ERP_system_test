using ERP.Application.Abstractions.Repositories;
using ERP.Domain.Entities;
using ERP.Persistence.Configuration;
using Microsoft.Extensions.Options;

namespace ERP.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of time entry repository
/// </summary>
internal sealed class TimeEntryRepository : ITimeEntryRepository
{
    private readonly MongoDbSettings _settings;

    public TimeEntryRepository(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResult<TimeEntry>> GetByFilterAsync(
        int year,
        int month,
        Guid? employeeId = null,
        Guid? projectId = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<Guid> CreateAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(TimeEntry timeEntry, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}