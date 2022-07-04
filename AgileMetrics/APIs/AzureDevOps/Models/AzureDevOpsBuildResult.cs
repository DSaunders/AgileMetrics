using System.Text.Json.Serialization;

namespace AgileMetrics.Core.APIs.AzureDevOps.Models;

internal class AzureDevOpsBuildResult
{
    [JsonPropertyName("value")]
    public IEnumerable<AzureDevopsBuild> Value { get; set; }
}