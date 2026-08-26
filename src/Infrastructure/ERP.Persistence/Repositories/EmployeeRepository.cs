using ERP.Application.Abstractions.Repositories;
using ERP.Domain.Entities;
using ERP.Domain.ValueObjects;
using ERP.Persistence.Configuration;
using Microsoft.Extensions.Options;

namespace ERP.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of employee repository
/// </summary>
internal sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly MongoDbSettings _settings;

    public EmployeeRepository(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        // TODO: Implement MongoDB operations
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Employee>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResult<Employee>> QueryAsync(
        IReadOnlyCollection<Guid>? ids = null,
        string? department = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task UpdateRatesAsync(
        Guid id,
        IReadOnlyList<Rate> rates,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}