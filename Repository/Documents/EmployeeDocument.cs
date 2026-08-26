using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Repository.Documents;

[BsonIgnoreExtraElements]
public sealed class EmployeeDocument
{
    [BsonId]
    public Guid Id { get; set; }

    public string FullName { get; set; } = null!;

    public string Department { get; set; } = null!;

    public List<RateDocument> Rates { get; set; } = [];
}
