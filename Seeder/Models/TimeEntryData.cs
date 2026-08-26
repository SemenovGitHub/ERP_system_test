using System.Text.Json.Serialization;

namespace Seeder.Models;

public sealed class TimeEntryData
{
    [JsonPropertyName("employeeId")]
    public string EmployeeId { get; set; } = string.Empty;

    [JsonPropertyName("projectId")]
    public string ProjectId { get; set; } = string.Empty;

    [JsonPropertyName("date")]
    public DateTime Date { get; set; }

    [JsonPropertyName("hours")]
    public decimal Hours { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;
}