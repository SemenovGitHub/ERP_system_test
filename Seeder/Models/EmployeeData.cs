using System.Text.Json.Serialization;

namespace Seeder.Models;

public sealed class EmployeeData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonPropertyName("department")]
    public string Department { get; set; } = string.Empty;

    [JsonPropertyName("rates")]
    public List<RateData> Rates { get; set; } = [];
}

public sealed class RateData
{
    [JsonPropertyName("from")]
    public DateTime From { get; set; }

    [JsonPropertyName("value")]
    public decimal Value { get; set; }
}