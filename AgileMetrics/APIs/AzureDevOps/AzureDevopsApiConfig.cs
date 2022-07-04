namespace AgileMetrics.Core.APIs.AzureDevOps;

public class AzureDevopsApiConfig
{
    public string DisplayName { get; set; }
    public string DevOpsAccessToken { get; set; }
    public string Organisation { get;  set;}
    public string Project { get;  set;}
    public int BuildDefinition { get;  set;}
    public string? TagToOverrideSuccessfulBuild { get;  set;}

    public AzureDevopsApiConfig()
    {
        
    }
}