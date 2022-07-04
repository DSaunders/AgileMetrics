namespace AgileMetrics.Core.Builds.Models;

public class TimeStatistics
{
    public TimeSpan MeanTime { get; set; }
    public TimeSpan StdDevTime { get; set; }

    public TimeSpan FastestTime { get; set; }
    public TimeSpan SlowestTime { get; set; }
}