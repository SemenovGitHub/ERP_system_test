using MongoDB.Driver;
using Repository.Documents;

namespace Repository.Mongo;

public sealed class MongoIndexInitializer
{
    private readonly MongoCollections _collections;

    public MongoIndexInitializer(MongoCollections collections)
    {
        _collections = collections;
    }

    public async Task EnsureIndexesAsync(CancellationToken cancellationToken)
    {
        await _collections.TimeEntriesCollection.Indexes.CreateManyAsync(
            [
                new CreateIndexModel<TimeEntryDocument>(
                    Builders<TimeEntryDocument>.IndexKeys
                        .Ascending(entry => entry.Date)
                        .Ascending(entry => entry.EmployeeId)
                        .Ascending(entry => entry.ProjectId),
                    new CreateIndexOptions { Name = "ix_time_entries_month_filters" }),
                new CreateIndexModel<TimeEntryDocument>(
                    Builders<TimeEntryDocument>.IndexKeys
                        .Ascending(entry => entry.EmployeeId)
                        .Ascending(entry => entry.Date),
                    new CreateIndexOptions { Name = "ix_time_entries_employee_day" })
            ],
            cancellationToken);

        await _collections.ProjectsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<ProjectDocument>(
                Builders<ProjectDocument>.IndexKeys.Ascending(project => project.Code),
                new CreateIndexOptions { Name = "ux_projects_code", Unique = true }),
            cancellationToken: cancellationToken);

        await _collections.ClosedPeriodsCollection.Indexes.CreateOneAsync(
            new CreateIndexModel<ClosedPeriodDocument>(
                Builders<ClosedPeriodDocument>.IndexKeys
                    .Ascending(period => period.Year)
                    .Ascending(period => period.Month),
                new CreateIndexOptions { Name = "ux_closed_periods_year_month", Unique = true }),
            cancellationToken: cancellationToken);
    }
}
