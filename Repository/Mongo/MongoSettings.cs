namespace Repository.Mongo;

public sealed class MongoSettings
{
    public const string SectionName = "Mongo";

    public string ConnectionString { get; set; } = null!;

    public string DatabaseName { get; set; } = null!;
}
