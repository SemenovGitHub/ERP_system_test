using Application.Interfaces;
using Domain.Employees;
using Domain.Exceptions;
using MongoDB.Driver;
using Repository.Mongo;

namespace Repository.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly MongoCollections _collections;

    public EmployeeRepository(MongoCollections collections)
    {
        _collections = collections;
    }

    public async Task<Employee?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await _collections.EmployeesCollection
            .Find(employee => employee.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document is null ? null : DocumentMapper.ToDomain(document);
    }

    public async Task<IReadOnlyList<Employee>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var documents = await _collections.EmployeesCollection
            .Find(Builders<Documents.EmployeeDocument>.Filter.In(employee => employee.Id, ids))
            .ToListAsync(cancellationToken);

        return documents.Select(DocumentMapper.ToDomain).ToList();
    }

    public async Task<PagedResult<Employee>> QueryAsync(
        IReadOnlyCollection<Guid>? ids,
        string? department,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var builder = Builders<Documents.EmployeeDocument>.Filter;
        var filter = builder.Empty;

        if (ids is { Count: > 0 })
        {
            filter &= builder.In(employee => employee.Id, ids);
        }

        if (!string.IsNullOrWhiteSpace(department))
        {
            filter &= builder.Eq(employee => employee.Department, department);
        }

        var skip = (page - 1) * pageSize;
        var totalTask = _collections.EmployeesCollection.CountDocumentsAsync(
            filter,
            cancellationToken: cancellationToken);
        var itemsTask = _collections.EmployeesCollection
            .Find(filter)
            .SortBy(employee => employee.FullName)
            .Skip(skip)
            .Limit(pageSize)
            .Project(employee => new Documents.EmployeeDocument
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Department = employee.Department
            })
            .ToListAsync(cancellationToken);

        await Task.WhenAll(totalTask, itemsTask);

        return new PagedResult<Employee>
        {
            Items = itemsTask.Result.Select(DocumentMapper.ToDomain).ToList(),
            TotalCount = totalTask.Result
        };
    }

    public async Task UpdateRatesAsync(
        Guid id,
        IReadOnlyList<Rate> rates,
        CancellationToken cancellationToken)
    {
        var rateDocuments = rates
            .Select(rate => new Documents.RateDocument
            {
                From = DocumentMapper.ToUtcDate(rate.From),
                Value = rate.Value
            })
            .ToList();

        var result = await _collections.EmployeesCollection.UpdateOneAsync(
            employee => employee.Id == id,
            Builders<Documents.EmployeeDocument>.Update.Set(employee => employee.Rates, rateDocuments),
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new BusinessException(ErrorCodes.NotFound, "Сотрудник не найден.", 404);
        }
    }
}
