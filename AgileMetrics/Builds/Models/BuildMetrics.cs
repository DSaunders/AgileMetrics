namespace AgileMetrics.Core.Builds.Models;

public class BuildMetrics
{
    public int SuccessfulBuilds { get; set; }
    public TimeStatistics SuccessfulBuildTimes { get; set; }
    
    public int FailedBuilds { get; set; }
    public TimeSpan TotalFailedTime { get; set; }
    public TimeStatistics FailureRecoveryTimes { get; set; }
}