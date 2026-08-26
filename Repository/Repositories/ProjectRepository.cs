using Application.Interfaces;
using Domain.Projects;
using MongoDB.Driver;
using Repository.Documents;
using Repository.Mongo;

namespace Repository.Repositories;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly MongoCollections _collections;

    public ProjectRepository(MongoCollections collections)
    {
        _collections = collections;
    }

    public async Task<Project?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await _collections.ProjectsCollection
            .Find(project => project.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document is null ? null : DocumentMapper.ToDomain(document);
    }

    public async Task<IReadOnlyList<Project>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var documents = await _collections.ProjectsCollection
            .Find(Builders<ProjectDocument>.Filter.In(project => project.Id, ids))
            .ToListAsync(cancellationToken);

        return documents.Select(DocumentMapper.ToDomain).ToList();
    }

    public async Task<PagedResult<Project>> QueryAsync(
        IReadOnlyCollection<Guid>? ids,
        string? code,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var builder = Builders<ProjectDocument>.Filter;
        var filter = builder.Empty;

        if (ids is { Count: > 0 })
        {
            filter &= builder.In(project => project.Id, ids);
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            filter &= builder.Eq(project => project.Code, code);
        }

        var skip = (page - 1) * pageSize;
        var totalTask = _collections.ProjectsCollection.CountDocumentsAsync(
            filter,
            cancellationToken: cancellationToken);
        var itemsTask = _collections.ProjectsCollection
            .Find(filter)
            .SortBy(project => project.Code)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        await Task.WhenAll(totalTask, itemsTask);

        return new PagedResult<Project>
        {
            Items = itemsTask.Result.Select(DocumentMapper.ToDomain).ToList(),
            TotalCount = totalTask.Result
        };
    }
}
