using MongoDB.Driver;
using Repository.Documents;

namespace Repository.Mongo;

public sealed class MongoCollections
{
    public const string Employees = "employees";
    public const string Projects = "projects";
    public const string TimeEntries = "time_entries";
    public const string ClosedPeriods = "closed_periods";

    public MongoCollections(IMongoDatabase database)
    {
        MongoSerializers.Register();
        EmployeesCollection = database.GetCollection<EmployeeDocument>(Employees);
        ProjectsCollection = database.GetCollection<ProjectDocument>(Projects);
        TimeEntriesCollection = database.GetCollection<TimeEntryDocument>(TimeEntries);
        ClosedPeriodsCollection = database.GetCollection<ClosedPeriodDocument>(ClosedPeriods);
    }

    public IMongoCollection<EmployeeDocument> EmployeesCollection { get; }

    public IMongoCollection<ProjectDocument> ProjectsCollection { get; }

    public IMongoCollection<TimeEntryDocument> TimeEntriesCollection { get; }

    public IMongoCollection<ClosedPeriodDocument> ClosedPeriodsCollection { get; }
}
