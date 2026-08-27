using MongoDB.Driver;
using Repository.Documents;
using Repository.Mongo;
using Seeder.Models;
using System.Text.Json;

namespace Seeder;

public sealed class DatabaseSeeder
{
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

        var createdAt = DateTime.UtcNow;

        var employees = await LoadEmployeesAsync(cancellationToken);
        var projects = await LoadProjectsAsync(cancellationToken);
        var timeEntries = await LoadTimeEntriesAsync(cancellationToken);

        var employeeDocuments = employees.Select(e => new EmployeeDocument
        {
            Id = Guid.Parse(e.Id),
            FullName = e.FullName,
            Department = e.Department,
            Rates = e.Rates.Select(r => new RateDocument
            {
                From = DateTime.SpecifyKind(r.From, DateTimeKind.Utc),
                Value = r.Value
            }).ToList()
        }).ToList();

        await _collections.EmployeesCollection.InsertManyAsync(
            employeeDocuments,
            cancellationToken: cancellationToken);

        var projectDocuments = projects.Select(p => new ProjectDocument
        {
            Id = Guid.Parse(p.Id),
            Code = p.Code,
            Name = p.Name,
            Budget = p.Budget,
            StartDate = DateTime.SpecifyKind(p.StartDate, DateTimeKind.Utc),
            EndDate = p.EndDate is { } end ? DateTime.SpecifyKind(end, DateTimeKind.Utc) : null
        }).ToList();

        await _collections.ProjectsCollection.InsertManyAsync(
            projectDocuments,
            cancellationToken: cancellationToken);

        var timeEntryDocuments = timeEntries.Select(te => new TimeEntryDocument
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.Parse(te.EmployeeId),
            ProjectId = Guid.Parse(te.ProjectId),
            Date = DateTime.SpecifyKind(te.Date, DateTimeKind.Utc),
            Hours = te.Hours,
            Comment = te.Description,
            Version = 1,
            CreatedAt = createdAt
        }).ToList();

        await _collections.TimeEntriesCollection.InsertManyAsync(
            timeEntryDocuments,
            cancellationToken: cancellationToken);
    }

    private async Task<List<EmployeeData>> LoadEmployeesAsync(CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync("Data/employees.json", cancellationToken);
        return JsonSerializer.Deserialize<List<EmployeeData>>(json) ?? [];
    }

    private async Task<List<ProjectData>> LoadProjectsAsync(CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync("Data/projects.json", cancellationToken);
        return JsonSerializer.Deserialize<List<ProjectData>>(json) ?? [];
    }

    private async Task<List<TimeEntryData>> LoadTimeEntriesAsync(CancellationToken cancellationToken)
    {
        var json = await File.ReadAllTextAsync("Data/timeEntries.json", cancellationToken);
        return JsonSerializer.Deserialize<List<TimeEntryData>>(json) ?? [];
    }
}
