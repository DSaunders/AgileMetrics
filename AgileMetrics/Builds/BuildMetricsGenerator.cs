using AgileMetrics.Core.Builds.Models;

namespace AgileMetrics.Core.Builds;

public class BuildMetricsGenerator
{
    private readonly IBuildApi _buildApi;

    public BuildMetricsGenerator(IBuildApi buildApi)
    {
        _buildApi = buildApi;
    }

    public async Task<BuildMetrics> GetBuildMetrics(DateTime? from = null, DateTime? to = null)
    {
        var buildResults = await _buildApi.GetBuilds();

        return PopulateBuildMetrics(buildResults, from, to);
    }

    private bool IsPassingBuild(Build build) =>
        build.Result is BuildResult.Succeeded or BuildResult.PartiallySucceeded &&
        build.FinishedAt != DateTime.MinValue;

    private bool IsFailingBuild(Build build) => build.Result is BuildResult.Failed;

    private TimeSpan HorriblyInefficientlyCalculateWorkingTime(DateTime from, DateTime to)
    {
        var elapsedMinutes = 0;

        var counterTime = from;

        while (counterTime < to)
        {
            counterTime = counterTime.AddMinutes(1);

            var isWeekend = counterTime.DayOfWeek == DayOfWeek.Saturday || counterTime.DayOfWeek == DayOfWeek.Sunday;
            var isInHours = counterTime.Hour >= 9 && counterTime.Hour < 17;

            if (!isWeekend && isInHours)
                elapsedMinutes++;
        }

        return TimeSpan.FromMinutes(elapsedMinutes);
    }
    
    private BuildMetrics PopulateBuildMetrics(IEnumerable<Build> builds, DateTime? from, DateTime? to)
    {
        var result = new BuildMetrics();

        var materialisedBuilds = builds.ToList();

        // Super slow and inefficient, but much easier to reason about.. loop 
        // through the list every time, for every metric

        CountBuilds(from, to, materialisedBuilds, result);
        TimeSuccessfulBuilds(from, to, materialisedBuilds, result);
        CalculateFailureRecoveryTimes(from, to, materialisedBuilds, result);
        CalculateTotalTimeInFailure(from, to, result, materialisedBuilds);

        return result;
    }

    private void CalculateTotalTimeInFailure(DateTime? from, DateTime? to, BuildMetrics result, List<Build> materialisedBuilds)
    {
        var inFailure = false;
        DateTime? lastFailure = null;
        result.TotalFailedTime = new TimeSpan();
        for (var i = 0; i < materialisedBuilds.Count; i++)
        {
            var build = materialisedBuilds[i];

            // Hit the end of our range
            if (build.QueuedAt > (to ?? DateTime.Now))
                break;

            // Build switched from passed -> failed
            if (IsFailingBuild(build) && !lastFailure.HasValue)
                lastFailure = build.FinishedAt;

            // Build switched from failed -> passed
            if (IsPassingBuild(build) && lastFailure != null)
            {
                if (IsWithinDateRange(from, to, build))
                {
                    if (from.HasValue && lastFailure.Value < from)
                        lastFailure = from;

                    var workingTimeLost = HorriblyInefficientlyCalculateWorkingTime(
                        lastFailure.Value,
                        build.StartedAt
                    );
                    result.TotalFailedTime += workingTimeLost;
                }

                lastFailure = null;
            }
        }

        if (lastFailure.HasValue)
        {
            if (from.HasValue && lastFailure.Value < from)
                lastFailure = from;

            var toEnd = to ?? DateTime.Now;

            var workingTimeLost = HorriblyInefficientlyCalculateWorkingTime(
                lastFailure.Value,
                toEnd
            );
            result.TotalFailedTime += workingTimeLost;
        }
    }

    private void CalculateFailureRecoveryTimes(DateTime? from, DateTime? to, List<Build> materialisedBuilds, BuildMetrics result)
    {
        var failureRecoveryTimes = new List<(DateTime? failedAt, long downTime)>();
        DateTime? lastFailure = null;
        for (var i = 0; i < materialisedBuilds.Count; i++)
        {
            var build = materialisedBuilds[i];

            // Build switched from passed -> failed
            if (IsFailingBuild(build) && !lastFailure.HasValue)
                lastFailure = build.FinishedAt;

            // Build switched from failed -> passed
            if (IsPassingBuild(build) && lastFailure != null)
            {
                var workingTimeLost = HorriblyInefficientlyCalculateWorkingTime(
                    lastFailure.Value,
                    build.StartedAt
                );
                failureRecoveryTimes.Add((lastFailure, workingTimeLost.Ticks));
                lastFailure = null;
            }
        }

        if (lastFailure.HasValue)
        {
            var workingTimeLost = HorriblyInefficientlyCalculateWorkingTime(
                lastFailure.Value,
                DateTime.Now
            );
            failureRecoveryTimes.Add((lastFailure, workingTimeLost.Ticks));
        }

        var failuresInTimeRange = failureRecoveryTimes
            .Where(f => (!from.HasValue || f.failedAt > from) &&
                        (!to.HasValue || f.failedAt < to.Value))
            .Select(f => f.downTime)
            .ToList();

        result.FailureRecoveryTimes = GenerateTimeStats(failuresInTimeRange);
    }

    private void TimeSuccessfulBuilds(DateTime? from, DateTime? to, List<Build> materialisedBuilds, BuildMetrics result)
    {
        var successfulBuildTimesTicks = new List<long>();
        for (var i = 0; i < materialisedBuilds.Count; i++)
        {
            var build = materialisedBuilds[i];

            if (!IsWithinDateRange(from, to, build))
                continue;

            if (IsFailingBuild(build))
                continue;
            
            if (!IsPassingBuild(build))
                continue;

            successfulBuildTimesTicks.Add((build.FinishedAt - build.StartedAt).Ticks);
        }

        result.SuccessfulBuildTimes = GenerateTimeStats(successfulBuildTimesTicks);
    }

    private void CountBuilds(DateTime? from, DateTime? to, List<Build> materialisedBuilds, BuildMetrics result)
    {
        foreach (var build in materialisedBuilds)
        {
            if (!IsWithinDateRange(from, to, build))
                continue;

            if (IsPassingBuild(build))
                result.SuccessfulBuilds++;

            if (IsFailingBuild(build))
                result.FailedBuilds++;
        }
    }

    private static bool IsWithinDateRange(DateTime? from, DateTime? to, Build build)
    {
        if (from.HasValue && build.StartedAt <= from)
            return false;

        if (to.HasValue && build.StartedAt > to)
            return false;

        return true;
    }

    private TimeStatistics GenerateTimeStats(List<long> tickTimes)
    {
        if (!tickTimes.Any())
            return new TimeStatistics();

        var stats = new TimeStatistics();

        stats.MeanTime = TimeSpan.FromTicks(
            (long)tickTimes.Average()
        );

        stats.StdDevTime = TimeSpan.FromTicks(
            CalculateStandardDeviation(tickTimes)
        );

        stats.FastestTime = TimeSpan.FromTicks(
            tickTimes.Min()
        );

        stats.SlowestTime = TimeSpan.FromTicks(
            tickTimes.Max()
        );

        return stats;
    }

    private long CalculateStandardDeviation(IEnumerable<long> values)
    {
        var avg = values.Average();
        return (long)Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
    }
}