using Application.Interfaces;
using MongoDB.Driver;
using Repository.Documents;
using Repository.Mongo;

namespace Repository.Repositories;

public sealed class PeriodRepository : IPeriodRepository
{
    private readonly MongoCollections _collections;

    public PeriodRepository(MongoCollections collections)
    {
        _collections = collections;
    }

    public async Task<bool> IsClosedAsync(int year, int month, CancellationToken cancellationToken)
    {
        var count = await _collections.ClosedPeriodsCollection
            .CountDocumentsAsync(
                period => period.Year == year && period.Month == month,
                cancellationToken: cancellationToken);

        return count > 0;
    }

    public async Task CloseAsync(int year, int month, CancellationToken cancellationToken)
    {
        await _collections.ClosedPeriodsCollection.UpdateOneAsync(
            period => period.Year == year && period.Month == month,
            Builders<ClosedPeriodDocument>.Update
                .SetOnInsert(period => period.Id, Guid.NewGuid())
                .SetOnInsert(period => period.Year, year)
                .SetOnInsert(period => period.Month, month),
            new UpdateOptions { IsUpsert = true },
            cancellationToken);
    }

    public Task OpenAsync(int year, int month, CancellationToken cancellationToken) =>
        _collections.ClosedPeriodsCollection.DeleteOneAsync(
            period => period.Year == year && period.Month == month,
            cancellationToken);
}
