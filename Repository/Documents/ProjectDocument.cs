using MongoDB.Bson.Serialization.Attributes;

namespace Repository.Documents;

[BsonIgnoreExtraElements]
public sealed class ProjectDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public string Code { get; set; } = null!;

    public string Name { get; set; } = null!;

    public decimal Budget { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime? EndDate { get; set; }
}
