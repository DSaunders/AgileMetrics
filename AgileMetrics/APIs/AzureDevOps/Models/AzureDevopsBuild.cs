using System.Text.Json.Serialization;

namespace AgileMetrics.Core.APIs.AzureDevOps.Models;

internal class AzureDevopsBuild
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("result")]
    public string Result { get; set; }
    [JsonPropertyName("queueTime")]
    public DateTime QueueTime { get; set; }
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; set; }
    [JsonPropertyName("finishTime")]
    public DateTime FinishTime { get; set; }
    [JsonPropertyName("tags")]
    public IEnumerable<string> Tags { get; set; }
}