using MongoDB.Driver;
using Repository.Documents;
using Repository.Mongo;

namespace Seeder;

public sealed class DatabaseSeeder
{
    public static readonly Guid IvanovId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid PetrovaId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    public static readonly Guid Project001Id = Guid.Parse("33333333-3333-3333-3333-333333333333");
    public static readonly Guid Project002Id = Guid.Parse("44444444-4444-4444-4444-444444444444");
    public static readonly Guid EntryFeb20Id = Guid.Parse("55555555-5555-5555-5555-555555555551");
    public static readonly Guid EntryMar05IvanovId = Guid.Parse("55555555-5555-5555-5555-555555555552");
    public static readonly Guid EntryMar05PetrovaId = Guid.Parse("55555555-5555-5555-5555-555555555553");
    public static readonly Guid EntryMar06PetrovaId = Guid.Parse("55555555-5555-5555-5555-555555555554");

    private readonly MongoCollections _collections;
    private readonly MongoIndexInitializer _indexes;

    public DatabaseSeeder(MongoCollections collections, MongoIndexInitializer indexes)
    {
        _collections = collections;
        _indexes = indexes;
    }

    public async Task RunAsync(CancellationToken cancellationToken)
    {
        await _indexes.EnsureIndexesAsync(cancellationToken);

        await _collections.EmployeesCollection.DeleteManyAsync(FilterDefinition<EmployeeDocument>.Empty, cancellationToken);
        await _collections.ProjectsCollection.DeleteManyAsync(FilterDefinition<ProjectDocument>.Empty, cancellationToken);
        await _collections.TimeEntriesCollection.DeleteManyAsync(FilterDefinition<TimeEntryDocument>.Empty, cancellationToken);
        await _collections.ClosedPeriodsCollection.DeleteManyAsync(FilterDefinition<ClosedPeriodDocument>.Empty, cancellationToken);

        var createdAt = new DateTime(2026, 2, 1, 0, 0, 0, DateTimeKind.Utc);

        await _collections.EmployeesCollection.InsertManyAsync(
            [
                new EmployeeDocument
                {
                    Id = IvanovId,
                    FullName = "Иванов И. И.",
                    Department = "Проектный",
                    Rates =
                    [
                        new RateDocument { From = Utc(2026, 1, 1), Value = 500m },
                        new RateDocument { From = Utc(2026, 3, 1), Value = 600m }
                    ]
                },
                new EmployeeDocument
                {
                    Id = PetrovaId,
                    FullName = "Петрова А. С.",
                    Department = "Проектный",
                    Rates =
                    [
                        new RateDocument { From = Utc(2026, 2, 1), Value = 700m }
                    ]
                }
            ],
            cancellationToken: cancellationToken);

        await _collections.ProjectsCollection.InsertManyAsync(
            [
                new ProjectDocument
                {
                    Id = Project001Id,
                    Code = "П-001",
                    Name = "Реконструкция цеха",
                    Budget = 20000m,
                    StartDate = Utc(2026, 1, 1),
                    EndDate = Utc(2026, 3, 31)
                },
                new ProjectDocument
                {
                    Id = Project002Id,
                    Code = "П-002",
                    Name = "Инженерные сети",
                    Budget = 5000m,
                    StartDate = Utc(2026, 3, 1),
                    EndDate = null
                }
            ],
            cancellationToken: cancellationToken);

        await _collections.TimeEntriesCollection.InsertManyAsync(
            [
                Entry(EntryFeb20Id, IvanovId, Project001Id, Utc(2026, 2, 20), 8m, createdAt),
                Entry(EntryMar05IvanovId, IvanovId, Project001Id, Utc(2026, 3, 5), 8m, createdAt),
                Entry(EntryMar05PetrovaId, PetrovaId, Project001Id, Utc(2026, 3, 5), 4m, createdAt),
                Entry(EntryMar06PetrovaId, PetrovaId, Project002Id, Utc(2026, 3, 6), 10m, createdAt)
            ],
            cancellationToken: cancellationToken);

        Console.WriteLine("База наполнена тестовыми данными из задания.");
    }

    private static DateTime Utc(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    private static TimeEntryDocument Entry(
        Guid id,
        Guid employeeId,
        Guid projectId,
        DateTime date,
        decimal hours,
        DateTime createdAt) =>
        new()
        {
            Id = id,
            EmployeeId = employeeId,
            ProjectId = projectId,
            Date = date,
            Hours = hours,
            Version = 1,
            CreatedAt = createdAt
        };
}
