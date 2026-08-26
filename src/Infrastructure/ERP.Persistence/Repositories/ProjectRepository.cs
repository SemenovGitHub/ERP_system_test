using ERP.Application.Abstractions.Repositories;
using ERP.Domain.Entities;
using ERP.Persistence.Configuration;
using Microsoft.Extensions.Options;

namespace ERP.Persistence.Repositories;

/// <summary>
/// MongoDB implementation of project repository
/// </summary>
internal sealed class ProjectRepository : IProjectRepository
{
    private readonly MongoDbSettings _settings;

    public ProjectRepository(IOptions<MongoDbSettings> settings)
    {
        _settings = settings.Value;
    }

    public Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<Project>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<PagedResult<Project>> QueryAsync(
        IReadOnlyCollection<Guid>? ids = null,
        string? code = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}