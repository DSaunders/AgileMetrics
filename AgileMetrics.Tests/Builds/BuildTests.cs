using System;
using System.Threading.Tasks;
using AgileMetrics.Core.Builds;
using AgileMetrics.Core.Builds.Models;
using AgileMetricsTests.Builds.Fakes;
using Shouldly;
using Xunit;

namespace AgileMetricsTests.Builds;

public class BuildTests
{
    private readonly BuildMetricsGenerator _metrics;
    private readonly FakeBuildApi _fakeBuilds;

    public BuildTests()
    {
        _fakeBuilds = new FakeBuildApi();
        _metrics = new BuildMetricsGenerator(_fakeBuilds);
    }

    
    [Fact]
    public async Task Returns_Total_Successful_Build_Count()
    {
        PassingBuild();
        FailingBuild();
        PassingBuild();

        var result = await _metrics.GetBuildMetrics();

        result.SuccessfulBuilds.ShouldBe(2);
    }

    [Fact]
    public async Task Considers_Partially_Successful_Builds_As_Successful_In_Metrics()
    {
        PassingBuild();
        PartialBuild();

        var result = await _metrics.GetBuildMetrics();

        result.SuccessfulBuilds.ShouldBe(2);
    }

    [Fact]
    public async Task Returns_Failed_Build_Count()
    {
        PassingBuild();
        FailingBuild();
        PassingBuild();
        FailingBuild();

        var result = await _metrics.GetBuildMetrics();
        
        result.FailedBuilds.ShouldBe(2);
    }

    [Fact]
    public async Task Counts_Repeatedly_Failing_Builds()
    {
        PassingBuild();
        FailingBuild();
        FailingBuild();
        PassingBuild();
        FailingBuild();

        var result = await _metrics.GetBuildMetrics();

        result.FailedBuilds.ShouldBe(3);
    }

    [Fact]
    public async Task Returns_Total_Time_Spent_In_Failed_Builds()
    {
        var t = DateTime.Now;

        FailingBuild(t.AddMinutes(1), t.AddMinutes(2)); // Failed at t+2
        FailingBuild(t.AddMinutes(3), t.AddMinutes(4));
        PassingBuild(t.AddMinutes(5)); // Passed again at t+5
        FailingBuild(t.AddMinutes(5), t.AddMinutes(6)); // Failed at t+6
        PassingBuild(t.AddMinutes(7)); // Passes at t+7

        var result = await _metrics.GetBuildMetrics();
        
        result.TotalFailedTime
            .ShouldBe(TimeSpan.FromMinutes(4));
    }

    [Fact]
    public async Task Returns_Mean_Build_Failure_Recover_Time()
    {
        var t = DateTime.Now;

        FailingBuild(t.AddMinutes(1), t.AddMinutes(2)); // Failed at t+2
        FailingBuild(t.AddMinutes(3), t.AddMinutes(4));
        PassingBuild(t.AddMinutes(6)); // Passed again at t+6 (down for 4 mins)
        FailingBuild(t.AddMinutes(5), t.AddMinutes(6)); // Failed at t+6
        PassingBuild(t.AddMinutes(8)); // Passes at t+7 (down for 2 mins)

        var result = await _metrics.GetBuildMetrics();
        
        result.FailureRecoveryTimes.MeanTime
            .ShouldBe(TimeSpan.FromMinutes(3));
    }

    [Fact]
    public async Task Returns_Standard_Deviation_Build_Failure_Recover_Time()
    {
        var t = DateTime.Now;

        PassingBuild(t);
        FailingBuild(t.AddMinutes(1), t.AddMinutes(2));
        PassingBuild(t.AddMinutes(3)); // Build fixed (down for 1 minute);
        FailingBuild(t.AddMinutes(3), t.AddMinutes(4));
        PassingBuild(t.AddMinutes(6)); // Build fixed (down for 2 minutes);
        FailingBuild(t.AddMinutes(7), t.AddMinutes(8));
        FailingBuild(t.AddMinutes(9), t.AddMinutes(11));
        PassingBuild(t.AddMinutes(12)); // Build fixed (down for 4 minutes);

        var result = await _metrics.GetBuildMetrics();
        
        Math.Round(result.FailureRecoveryTimes.StdDevTime.TotalMinutes, 2)
            .ShouldBe(1.25);
    }

    [Fact]
    public async Task Returns_Fastest_Build_Failure_Recover_Time()
    {
        var t = DateTime.Now;

        FailingBuild(t.AddMinutes(1), t.AddMinutes(2));
        PassingBuild(t.AddMinutes(3)); // Build fixed (down for 1 minute);
        FailingBuild(t.AddMinutes(3), t.AddMinutes(4));
        PassingBuild(t.AddMinutes(6)); // Build fixed (down for 2 minutes);

        var result = await _metrics.GetBuildMetrics();
        
        result.FailureRecoveryTimes.FastestTime
            .ShouldBe(TimeSpan.FromMinutes(1));
    }

    [Fact]
    public async Task Returns_Slowest_Build_Failure_Recover_Time()
    {
        var t = DateTime.Now;

        FailingBuild(t.AddMinutes(1), t.AddMinutes(2));
        PassingBuild(t.AddMinutes(3)); // Build fixed (down for 1 minute);
        FailingBuild(t.AddMinutes(3), t.AddMinutes(4));
        PassingBuild(t.AddMinutes(6)); // Build fixed (down for 2 minutes);

        var result = await _metrics.GetBuildMetrics();
        
        result.FailureRecoveryTimes.SlowestTime
            .ShouldBe(TimeSpan.FromMinutes(2));
    }

    [Fact]
    public async Task Ignores_In_Progress_Builds()
    {
        PassingBuild();
        InProgressBuild();

        var result = await _metrics.GetBuildMetrics();
        
        result.SuccessfulBuilds.ShouldBe(1);
    }

    [Fact]
    public async Task Calculates_Mean_Successful_Build_Times()
    {
        var referenceTime = DateTime.Now;

        PassingBuild(referenceTime.AddMinutes(1), referenceTime.AddMinutes(2));
        FailingBuild(referenceTime.AddMinutes(2), referenceTime.AddMinutes(3));
        PassingBuild(referenceTime.AddMinutes(3), referenceTime.AddMinutes(6));

        var result = await _metrics.GetBuildMetrics();
        
        result.SuccessfulBuildTimes.MeanTime.ShouldBe(TimeSpan.FromMinutes(2));
    }

    [Fact]
    public async Task Calculates_Standard_Deviation_Of_Successful_Build_Times()
    {
        var referenceTime = DateTime.Now;

        PassingBuild(referenceTime.AddMinutes(1), referenceTime.AddMinutes(2));
        FailingBuild(referenceTime.AddMinutes(2), referenceTime.AddMinutes(3));
        PassingBuild(referenceTime.AddMinutes(3), referenceTime.AddMinutes(6));
        PassingBuild(referenceTime.AddMinutes(6), referenceTime.AddMinutes(11));

        var result = await _metrics.GetBuildMetrics();
        
        Math.Round(result.SuccessfulBuildTimes.StdDevTime.TotalSeconds, 2)
            .ShouldBe(97.98);
    }

    [Fact]
    public async Task Returns_Slowest_Successful_Build_Time()
    {
        var t = DateTime.Now;

        PassingBuild(t.AddMinutes(1), t.AddMinutes(2));
        PassingBuild(t.AddMinutes(1), t.AddMinutes(3));

        var result = await _metrics.GetBuildMetrics();
        
        result.SuccessfulBuildTimes.SlowestTime.ShouldBe(
            TimeSpan.FromMinutes(2)
        );
    }

    [Fact]
    public async Task Returns_Quickest_Successful_Build_Time()
    {
        var t = DateTime.Now;

        PassingBuild(t.AddMinutes(1), t.AddMinutes(2));
        PassingBuild(t.AddMinutes(1), t.AddMinutes(3));

        var result = await _metrics.GetBuildMetrics();
        
        result.SuccessfulBuildTimes.FastestTime.ShouldBe(
            TimeSpan.FromMinutes(1)
        );
    }

    private void PassingBuild(DateTime? startTime = null, DateTime? endTime = null)
    {
        _fakeBuilds.AddBuild(new Build
        {
            Result = BuildResult.Succeeded,
            StartedAt = startTime ?? DateTime.Now,
            FinishedAt = endTime ?? DateTime.Now,
        });
    }

    private void PartialBuild(DateTime? startTime = null, DateTime? endTime = null)
    {
        _fakeBuilds.AddBuild(new Build
        {
            Result = BuildResult.PartiallySucceeded,
            StartedAt = startTime ?? DateTime.Now,
            FinishedAt = endTime ?? DateTime.Now,
        });
    }

    private void InProgressBuild(DateTime? startTime = null, DateTime? endTime = null)
    {
        _fakeBuilds.AddBuild(new Build
        {
            Result = BuildResult.InProgress,
            StartedAt = startTime ?? DateTime.Now,
            FinishedAt = endTime ?? DateTime.Now,
        });
    }

    private void FailingBuild(DateTime? startTime = null, DateTime? endTime = null)
    {
        _fakeBuilds.AddBuild(new Build
        {
            Result = BuildResult.Failed,
            StartedAt = startTime ?? DateTime.Now,
            FinishedAt = endTime ?? DateTime.Now
        });
    }
}