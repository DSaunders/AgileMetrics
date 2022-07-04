# AgileMetrics

Displays metrics from Azure DevOps that can help to measure your organisation's performance.

The metrics are based on those recommended in the books [Measuring Continutous Delivery](https://www.amazon.co.uk/gp/product/B08LYZDPMK/ref=ppx_yo_dt_b_search_asin_title?ie=UTF8&psc=1) and [Accelerate](https://www.amazon.co.uk/Accelerate-Software-Performing-Technology-Organizations-ebook/dp/B07B9F83WM/ref=sr_1_1?crid=1FNSJVL7673IT&keywords=accelerate+book&qid=1656938480&s=digital-text&sprefix=accellerate+book%2Cdigital-text%2C75&sr=1-1).

> Note: This is a spike. The code is.. uhh.. 'not optimal'.. and the UI is ugly. It works though, so feel free to use it while I clean it up 😊


![image](https://user-images.githubusercontent.com/4059030/177156417-255348db-03c2-4c10-9bd7-834e8aefd90b.png)

----

## Supported Metrics

Currently, only build metrics are supported.

Ticket flow metrics (cycle time, lead time etc.) will come later, but the build-in widgets in Azure DevOps are pretty good already.

### Build Count

Displays the total number of passing and failing builds for the period:

![image](https://user-images.githubusercontent.com/4059030/177157433-ea734be1-6150-4703-82e3-bb93a7ccab5f.png)


### Build Time

Displays the total time to produce a working build (the time your pipeline takes to execute, from start to finish):

![image](https://user-images.githubusercontent.com/4059030/177157766-fe7d6d53-077b-4206-a599-ee96a73709e0.png)

The solid line displays the average (the mean) of all build times for the period.

The light-green area represents the standard deviation - in other words, the variance in the build times. 

The smaller the green area, the more consistent the build times are..

### Build recovery time

Shows the number of working hours taken to fix a broken build.

![image](https://user-images.githubusercontent.com/4059030/177158379-917f3546-00a9-4a25-8525-c63690a14b5f.png)

This chart will also show the standard deviation of the 'fix time' in a light red colour (not shown in the example above).

Note: working hours are assumed to be 9am-5pm Monday-Friday. This will be configurable later.

### 'Blocked Time'

This is the total amount of time that the build was in failure for during the period.

If a build is in failure, no further production deployments can occur until it is fixed. That's what we're measuring here:

![image](https://user-images.githubusercontent.com/4059030/177158615-4a003a8a-9b85-4015-bfc0-79d1a46015ca.png)

Note: Again, working hours are assumed to be 9am-5pm Monday-Friday for now.



## Configuration

Add an appsettings.json to the root of the web project, with a collection of build definitions you would like to measure:

```
"BuildDefinitions": [
  {
    "DisplayName": "Example 1",
    "DevOpsAccessToken": "sometoken",
    "Organisation": "myOrg",
    "Project": "My Project Name",
    "BuildDefinition": 123
  },
  {
    "DisplayName": "Example 2,
    "DevOpsAccessToken": "sometoken",
    "Organisation": "myOrg",
    "Project": "Another Project Name",
    "BuildDefinition": 456
  },
]
 ```
 
| Property    | Description |
| ----------- | ----------- |
| DisplayName | The name shown in the UI for this build definition |
| DevOpsAccessToken | The [Personal Access Token](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate?view=azure-devops&tabs=Windows) that can be used to read the build definitions from the  Azure DevOps API |
| Organisation | Usually at the start of your Azure DevOps URL.. e.g. `myOrg.visualstudio.com` |
| Project | The name of the project that this build definition belongs to |
| BuildDefinition | The integer ID of the build definition. This can be found by viewing the build in the browser and inspecting the end of the URL. E.g. `_build?definitionId=123` |


## Things to do

This is a work in progress, the next items to work on are:

- Ability to filter build definitions by branch. Currently only builds to `refs/heads/master` are measured
- Dynamically choose a time period. Currently the dashboard is fixed to show the last 6 weeks
- Improve the API call code. Currently a single API call is made on load, and cached for the duration. In this time, the UI makes multiple calls to get the value from this cache. This could be improved
