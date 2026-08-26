using Domain.Projects;

namespace Application.Interfaces;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Project>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken);

    Task<PagedResult<Project>> QueryAsync(
        IReadOnlyCollection<Guid>? ids,
        string? code,
        int page,
        int pageSize,
        CancellationToken cancellationToken);
}
