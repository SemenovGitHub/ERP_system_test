using MongoDB.Bson.Serialization.Attributes;

namespace Repository.Documents;

[BsonIgnoreExtraElements]
public sealed class TimeEntryDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }

    public Guid ProjectId { get; set; }

    public DateTime Date { get; set; }

    public decimal Hours { get; set; }

    public string? Comment { get; set; }

    public int Version { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
