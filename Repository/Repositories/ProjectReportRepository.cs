using Domain.Interfaces;
using Domain.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Repository.Mongo;

namespace Repository.Repositories;

public sealed class ProjectReportRepository : IProjectReportRepository
{
    private readonly MongoCollections _collections;

    public ProjectReportRepository(MongoCollections collections)
    {
        _collections = collections;
    }

    public async Task<IReadOnlyList<ProjectReportModel>> GetByMonthAsync(
        int year,
        int month,
        CancellationToken cancellationToken)
    {
        var start = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
        var end = start.AddMonths(1);

        var pipeline = new[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                {
                    "Date",
                    new BsonDocument
                    {
                        { "$gte", start },
                        { "$lt", end }
                    }
                }
            }),
            TimeEntryRepository.LookupEmployeesStage(),
            new BsonDocument("$unwind", "$employee"),
            TimeEntryRepository.CostStage(),
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", MongoCollections.Projects },
                { "localField", "ProjectId" },
                { "foreignField", "_id" },
                { "as", "project" }
            }),
            new BsonDocument("$unwind", "$project"),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$ProjectId" },
                { "Hours", new BsonDocument("$sum", "$Hours") },
                { "Cost", new BsonDocument("$sum", "$Cost") },
                { "ProjectCode", new BsonDocument("$first", "$project.Code") },
                { "ProjectName", new BsonDocument("$first", "$project.Name") },
                { "Budget", new BsonDocument("$first", "$project.Budget") }
            }),
            new BsonDocument("$sort", new BsonDocument("ProjectCode", 1))
        };

        var rows = await _collections.TimeEntriesCollection
            .Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);

        return rows.Select(row => new ProjectReportModel
        {
            ProjectId = row["_id"].AsGuid,
            ProjectCode = row["ProjectCode"].AsString,
            ProjectName = row["ProjectName"].AsString,
            Hours = row["Hours"].ToDecimal(),
            Cost = row["Cost"].ToDecimal(),
            Budget = row["Budget"].ToDecimal()
        }).ToList();
    }
}
