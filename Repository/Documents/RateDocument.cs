using MongoDB.Bson.Serialization.Attributes;

namespace Repository.Documents;

public sealed class RateDocument
{
    public DateTime From { get; set; }

    public decimal Value { get; set; }
}
