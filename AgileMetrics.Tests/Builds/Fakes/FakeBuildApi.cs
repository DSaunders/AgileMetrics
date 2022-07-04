using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AgileMetrics.Core.Builds;
using AgileMetrics.Core.Builds.Models;

namespace AgileMetricsTests.Builds.Fakes;

public class FakeBuildApi : IBuildApi
{
    public DateTime? To { get; private set; }
    public DateTime? From { get; private set; }
    
    private readonly List<Build> _builds = new();

    public void AddBuild(Build buildToAdd) => _builds.Add(buildToAdd);

    public Task<IReadOnlyCollection<Build>> GetBuilds(DateTime? from = null, DateTime? to = null)
    {
        From = from;
        To = to;
        return Task.FromResult<IReadOnlyCollection<Build>>(
            _builds.AsReadOnly()
        );
    }
}