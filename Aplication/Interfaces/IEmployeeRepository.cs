using Domain.Models;

namespace Application.Interfaces;

public interface IEmployeeRepository
{
    Task<EmployeeModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<EmployeeModel>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);

    Task<PagedResult<EmployeeModel>> QueryAsync(
        IReadOnlyCollection<Guid>? ids,
        string? department,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task UpdateRatesAsync(Guid id, IReadOnlyList<RateModel> rates, CancellationToken cancellationToken);
}
