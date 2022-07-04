using System.Net.Http.Headers;
using System.Text.Json;
using AgileMetrics.Core.APIs.AzureDevOps.Models;
using AgileMetrics.Core.Builds;
using AgileMetrics.Core.Builds.Models;

namespace AgileMetrics.Core.APIs.AzureDevOps;

public class AzureDevopsApi : IBuildApi
{
    private readonly AzureDevopsApiConfig _config;
    private List<Build>? _buildCache;

    public AzureDevopsApi(AzureDevopsApiConfig config)
    {
        _config = config;
    }

    public async Task<IReadOnlyCollection<Build>> GetBuilds(DateTime? from = null, DateTime? to = null)
    {
        if (_buildCache is not null) 
            return ReturnFromCache(from, to);

        var url = $"https://dev.azure.com/" +
                  $"{_config.Organisation}/" +
                  $"{_config.Project}/" +
                  $"_apis/build/builds?definitions=" +
                  _config.BuildDefinition +
                  $"&branchname=refs/heads/master&$top=200";

        var builds = await CallApi<AzureDevOpsBuildResult>(url);

        if (builds is null)
            return new List<Build>();

        _buildCache = builds
            .Value
            .OrderBy(b => b.QueueTime)
            .Select(b => new Build
            {
                Id = b.Id,
                QueuedAt = b.QueueTime,
                StartedAt = b.StartTime,
                FinishedAt = b.FinishTime,
                Result = GetResultForBuild(b),
            })
            .ToList();

        return ReturnFromCache(from, to);
    }

    private IReadOnlyCollection<Build> ReturnFromCache(DateTime? from, DateTime? to)
    {
        return _buildCache
            .Where(b => (!from.HasValue || b.QueuedAt >= from.Value) && (!to.HasValue || b.QueuedAt < to.Value))
            .ToList();
    }

    private async Task<T?> CallApi<T>(string url)
    {
        using var client = new HttpClient();

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Basic",
            Convert.ToBase64String(
                System.Text.Encoding.ASCII.GetBytes($":{_config.DevOpsAccessToken}")
            )
        );

        using var response = await client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();

        var resultModel = JsonSerializer.Deserialize<T>(responseBody);

        return resultModel ?? default(T);
    }

    private BuildResult GetResultForBuild(AzureDevopsBuild build)
    {
        if (_config.TagToOverrideSuccessfulBuild != null &&
            build.Tags.Contains(_config.TagToOverrideSuccessfulBuild))
        {
            return BuildResult.Succeeded;
        }

        return build.Result switch
        {
            "failed" => BuildResult.Failed,
            "partiallySucceeded" => BuildResult.PartiallySucceeded,
            "inProgress" => BuildResult.InProgress,
            _ => BuildResult.Succeeded
        };
    }
}