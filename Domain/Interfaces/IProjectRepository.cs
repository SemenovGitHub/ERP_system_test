using Domain.Models;

namespace Domain.Interfaces;

public interface IProjectRepository
{
    Task<ProjectModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProjectModel>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);

    Task<PagedResult<ProjectModel>> QueryAsync(
        IReadOnlyCollection<Guid>? ids,
        string? code,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
