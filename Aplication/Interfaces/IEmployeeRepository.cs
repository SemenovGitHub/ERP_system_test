using Domain.Employees;

namespace Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Employee>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);

    Task<PagedResult<Employee>> QueryAsync(
        IReadOnlyCollection<Guid>? ids,
        string? department,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task UpdateRatesAsync(Guid id, IReadOnlyList<Rate> rates, CancellationToken cancellationToken);
}
