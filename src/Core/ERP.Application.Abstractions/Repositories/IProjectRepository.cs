using ERP.Domain.Projects;

namespace ERP.Application.Abstractions.Repositories;

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<Project>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default);
    
    Task<PagedResult<Project>> QueryAsync(
        IReadOnlyCollection<Guid>? ids = null,
        string? code = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}