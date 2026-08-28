using Domain.Interfaces;
using AutoMapper;
using Domain.Exceptions;
using Domain.Models;
using MongoDB.Driver;
using Repository.Documents;
using Repository.Mongo;

namespace Repository.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly MongoCollections _collections;
    private readonly IMapper _mapper;

    public EmployeeRepository(MongoCollections collections, IMapper mapper)
    {
        _collections = collections;
        _mapper = mapper;
    }

    public async Task<EmployeeModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await _collections.EmployeesCollection
            .Find(employee => employee.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document is null ? null : _mapper.Map<EmployeeModel>(document);
    }

    public async Task<IReadOnlyList<EmployeeModel>> GetByIdsAsync(
        IReadOnlyCollection<Guid> ids,
        CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var documents = await _collections.EmployeesCollection
            .Find(Builders<EmployeeDocument>.Filter.In(employee => employee.Id, ids))
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<EmployeeModel>>(documents);
    }

    public async Task<PagedResult<EmployeeModel>> QueryAsync(
        IReadOnlyCollection<Guid>? ids,
        string? department,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var builder = Builders<EmployeeDocument>.Filter;
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
            .ToListAsync(cancellationToken);

        await Task.WhenAll(totalTask, itemsTask);

        return new PagedResult<EmployeeModel>
        {
            Items = _mapper.Map<List<EmployeeModel>>(itemsTask.Result),
            TotalCount = totalTask.Result
        };
    }

    public async Task UpdateRatesAsync(
        Guid id,
        IReadOnlyList<RateModel> rates,
        CancellationToken cancellationToken)
    {
        var rateDocuments = _mapper.Map<List<RateDocument>>(rates);

        var result = await _collections.EmployeesCollection.UpdateOneAsync(
            employee => employee.Id == id,
            Builders<EmployeeDocument>.Update.Set(employee => employee.Rates, rateDocuments),
            cancellationToken: cancellationToken);

        if (result.MatchedCount == 0)
        {
            throw new BusinessException(ErrorCodes.NotFound, "Сотрудник не найден.", 404);
        }
    }
}
