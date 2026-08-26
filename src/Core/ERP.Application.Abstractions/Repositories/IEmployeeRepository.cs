using ERP.Domain.Employees;

namespace ERP.Application.Abstractions.Repositories;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Employee>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);
    
    Task<PagedResult<Employee>> QueryAsync(
        IReadOnlyCollection<Guid>? ids = null,
        string? department = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    
    Task UpdateRatesAsync(
        Guid id, 
        IReadOnlyList<Rate> rates, 
        CancellationToken cancellationToken = default);
}

public sealed class PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public long TotalCount { get; init; }
}