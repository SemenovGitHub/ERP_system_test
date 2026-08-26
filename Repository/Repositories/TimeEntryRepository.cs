using Application.Interfaces;
using Domain.Exceptions;
using Domain.TimeEntries;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Repository.Documents;
using Repository.Mongo;

namespace Repository.Repositories;

public sealed class TimeEntryRepository : ITimeEntryRepository
{
    private readonly MongoCollections _collections;

    public TimeEntryRepository(MongoCollections collections)
    {
        _collections = collections;
    }

    public async Task<TimeEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var document = await _collections.TimeEntriesCollection
            .Find(entry => entry.Id == id)
            .FirstOrDefaultAsync(cancellationToken);

        return document is null ? null : DocumentMapper.ToDomain(document);
    }

    public async Task<PagedTimeEntries> GetPagedAsync(
        int year,
        int month,
        Guid? employeeId,
        Guid? projectId,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        var filter = MonthFilter(year, month, employeeId, projectId);
        var skip = (page - 1) * pageSize;

        var items = await _collections.TimeEntriesCollection
            .Find(filter)
            .SortBy(entry => entry.Date)
            .ThenBy(entry => entry.Id)
            .Skip(skip)
            .Limit(pageSize)
            .ToListAsync(cancellationToken);

        var totals = await AggregateTotalsAsync(filter, cancellationToken);

        return new PagedTimeEntries
        {
            Items = items.Select(DocumentMapper.ToDomain).ToList(),
            TotalCount = totals.Count,
            TotalHours = totals.Hours,
            TotalCost = totals.Cost
        };
    }

    public async Task<decimal> GetHoursForDayAsync(
        Guid employeeId,
        DateOnly date,
        Guid? excludeEntryId,
        CancellationToken cancellationToken)
    {
        var utcDate = DocumentMapper.ToUtcDate(date);
        var builder = Builders<TimeEntryDocument>.Filter;
        var filter = builder.Eq(entry => entry.EmployeeId, employeeId)
                     & builder.Eq(entry => entry.Date, utcDate);

        if (excludeEntryId is { } excludeId)
        {
            filter &= builder.Ne(entry => entry.Id, excludeId);
        }

        var grouped = await _collections.TimeEntriesCollection
            .Aggregate()
            .Match(filter)
            .Group(
                _ => 1,
                group => new { Hours = group.Sum(entry => entry.Hours) })
            .FirstOrDefaultAsync(cancellationToken);

        return grouped?.Hours ?? 0;
    }

    public async Task<IReadOnlyDictionary<(Guid EmployeeId, DateOnly Date), decimal>> GetHoursByDayAsync(
        IReadOnlyCollection<(Guid EmployeeId, DateOnly Date)> keys,
        CancellationToken cancellationToken)
    {
        if (keys.Count == 0)
        {
            return new Dictionary<(Guid, DateOnly), decimal>();
        }

        var clauses = keys
            .Select(key =>
                Builders<TimeEntryDocument>.Filter.Eq(entry => entry.EmployeeId, key.EmployeeId)
                & Builders<TimeEntryDocument>.Filter.Eq(
                    entry => entry.Date,
                    DocumentMapper.ToUtcDate(key.Date)))
            .ToArray();

        var grouped = await _collections.TimeEntriesCollection
            .Aggregate()
            .Match(Builders<TimeEntryDocument>.Filter.Or(clauses))
            .Group(
                entry => new { entry.EmployeeId, entry.Date },
                group => new
                {
                    group.Key.EmployeeId,
                    group.Key.Date,
                    Hours = group.Sum(entry => entry.Hours)
                })
            .ToListAsync(cancellationToken);

        return grouped.ToDictionary(
            row => (row.EmployeeId, DocumentMapper.ToDateOnly(row.Date)),
            row => row.Hours);
    }

    public Task AddAsync(TimeEntry entry, CancellationToken cancellationToken) =>
        _collections.TimeEntriesCollection.InsertOneAsync(
            DocumentMapper.ToDocument(entry),
            cancellationToken: cancellationToken);

    public async Task UpdateAsync(
        TimeEntry entry,
        int expectedVersion,
        CancellationToken cancellationToken)
    {
        var document = DocumentMapper.ToDocument(entry);
        var result = await _collections.TimeEntriesCollection.ReplaceOneAsync(
            existing => existing.Id == entry.Id && existing.Version == expectedVersion,
            document,
            cancellationToken: cancellationToken);

        if (result.MatchedCount > 0)
        {
            return;
        }

        var exists = await _collections.TimeEntriesCollection
            .Find(existing => existing.Id == entry.Id)
            .AnyAsync(cancellationToken);

        if (!exists)
        {
            throw new BusinessException(ErrorCodes.NotFound, "Запись табеля не найдена.", 404);
        }

        throw new BusinessException(
            ErrorCodes.ConcurrencyConflict,
            "Запись уже изменили. Обновите данные и сохраните снова.",
            409);
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken) =>
        _collections.TimeEntriesCollection.DeleteOneAsync(
            entry => entry.Id == id,
            cancellationToken);

    private async Task<(long Count, decimal Hours, decimal Cost)> AggregateTotalsAsync(
        FilterDefinition<TimeEntryDocument> filter,
        CancellationToken cancellationToken)
    {
        var pipeline = new[]
        {
            new BsonDocument("$match", Render(filter)),
            LookupEmployeesStage(),
            new BsonDocument("$unwind", "$employee"),
            CostStage(),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", BsonNull.Value },
                { "Count", new BsonDocument("$sum", 1) },
                { "Hours", new BsonDocument("$sum", "$Hours") },
                { "Cost", new BsonDocument("$sum", "$Cost") }
            })
        };

        var row = await _collections.TimeEntriesCollection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .FirstOrDefaultAsync(cancellationToken);

        if (row is null)
        {
            return (0, 0, 0);
        }

        return (row["Count"].ToInt64(), row["Hours"].ToDecimal(), row["Cost"].ToDecimal());
    }

    private FilterDefinition<TimeEntryDocument> MonthFilter(
        int year,
        int month,
        Guid? employeeId,
        Guid? projectId)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var builder = Builders<TimeEntryDocument>.Filter;
        var filter = builder.Gte(entry => entry.Date, start)
                     & builder.Lt(entry => entry.Date, start.AddMonths(1));

        if (employeeId is { } employee)
        {
            filter &= builder.Eq(entry => entry.EmployeeId, employee);
        }

        if (projectId is { } project)
        {
            filter &= builder.Eq(entry => entry.ProjectId, project);
        }

        return filter;
    }

    internal static BsonDocument LookupEmployeesStage() =>
        new("$lookup", new BsonDocument
        {
            { "from", MongoCollections.Employees },
            { "localField", nameof(TimeEntryDocument.EmployeeId) },
            { "foreignField", "_id" },
            { "as", "employee" }
        });

    internal static BsonDocument CostStage() =>
        new("$addFields", new BsonDocument
        {
            {
                "Cost",
                new BsonDocument("$round", new BsonArray
                {
                    new BsonDocument("$multiply", new BsonArray
                    {
                        "$Hours",
                        new BsonDocument("$getField", new BsonDocument
                        {
                            { "field", "Value" },
                            {
                                "input",
                                new BsonDocument("$reduce", new BsonDocument
                                {
                                    {
                                        "input",
                                        new BsonDocument("$filter", new BsonDocument
                                        {
                                            { "input", "$employee.Rates" },
                                            { "as", "rate" },
                                            {
                                                "cond",
                                                new BsonDocument("$lte", new BsonArray
                                                {
                                                    "$$rate.From",
                                                    "$Date"
                                                })
                                            }
                                        })
                                    },
                                    { "initialValue", BsonNull.Value },
                                    {
                                        "in",
                                        new BsonDocument("$cond", new BsonArray
                                        {
                                            new BsonDocument("$or", new BsonArray
                                            {
                                                new BsonDocument("$eq", new BsonArray
                                                {
                                                    "$$value",
                                                    BsonNull.Value
                                                }),
                                                new BsonDocument("$gt", new BsonArray
                                                {
                                                    "$$this.From",
                                                    "$$value.From"
                                                })
                                            }),
                                            "$$this",
                                            "$$value"
                                        })
                                    }
                                })
                            }
                        })
                    }),
                    2
                })
            }
        });

    private static BsonDocument Render(FilterDefinition<TimeEntryDocument> filter)
    {
        var serializer = BsonSerializer.LookupSerializer<TimeEntryDocument>();
        var args = new RenderArgs<TimeEntryDocument>(serializer, BsonSerializer.SerializerRegistry);
        return filter.Render(args);
    }
}
