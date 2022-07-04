using AgileMetrics.Core.Builds.Models;

namespace AgileMetrics.Core.Builds;

public interface IBuildApi
{
    Task<IReadOnlyCollection<Build>> GetBuilds(DateTime? from = null, DateTime? to = null);
}
