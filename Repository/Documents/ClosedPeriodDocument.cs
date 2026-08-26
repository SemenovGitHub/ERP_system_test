using MongoDB.Bson.Serialization.Attributes;

namespace Repository.Documents;

[BsonIgnoreExtraElements]
public sealed class ClosedPeriodDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }
}
