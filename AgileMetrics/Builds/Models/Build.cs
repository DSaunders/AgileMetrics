namespace AgileMetrics.Core.Builds.Models;

public class Build
{
    public int Id { get; set; }
    public BuildResult Result { get; set; }
    public DateTime QueuedAt { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime FinishedAt { get; set; }
}