export async function getBuildMetricsForPeriod(buildId, numberOfPeriods, lengthOfPeriod) {

  const results = []

  var now = new Date();
  now.setHours(23, 59, 59, 0);

  for (let i = 0; i < numberOfPeriods; i++) {

    var from = new Date(now);
    from.setDate(from.getDate() - ((i+1) * lengthOfPeriod));

    var to = new Date(from);
    to.setDate(to.getDate() + lengthOfPeriod);

    const response = await fetch(`api/builds/${buildId}/metrics?from=${from.toISOString()}&to=${to.toISOString()}`);
    const data = await response.json();

    console.log(data.failureRecoveryTimes);

    let thisResult = {
      from: from,
      to: to,
      failedBuilds: data.failedBuilds,
      successfulBuilds: data.successfulBuilds,
    };

    thisResult.successfulBuildTimeMean = minutesFromTimeSpan(data.successfulBuildTimes?.meanTime);
    thisResult.successfulBuildTimeStdDev = minutesFromTimeSpan(data.successfulBuildTimes?.stdDevTime);

    thisResult.failureRecoveryTimeMean = hoursFromTimeSpan(data.failureRecoveryTimes?.meanTime);
    thisResult.failureRecoveryTimeStdDev = hoursFromTimeSpan(data.failureRecoveryTimes?.stdDevTime);

    thisResult.timeUnableToDeploy = hoursFromTimeSpan(data.totalFailedTime);

    results[i] = thisResult;
  }

  console.log(results)

  return results.reverse();
}

function minutesFromTimeSpan(timespan) {
  if (!timespan)
    return 0;

  let components = timespan.split(":");
  
  var withDays = components[0].split(".");
  if (withDays.length <= 1)
    return parseInt((components[0] * 60) + (components[1] * 1));
  
  return parseInt((withDays[0] * 24 * 60) + (withDays[1] * 60) + (+components[1]));
}



function hoursFromTimeSpan(timespan) {
  if (!timespan)
    return 0;

  let components = timespan.split(":");

  var withDays = components[0].split(".");
  if (withDays.length <= 1)
    return parseInt(components[0]);

  return parseInt((withDays[0] * 24) + (+withDays[1]));
}