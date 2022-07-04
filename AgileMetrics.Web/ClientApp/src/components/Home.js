import React, {useEffect, useState} from 'react'
import {Chart as ChartJS, registerables} from 'chart.js';
import BuildCounter from "./BuildCounter";
import BuildSpeed from "./BuildSpeed";
import FailureRecoveryTime from "./FailureRecoveryTime";
import TimeBlockedFromDeploying from "./TimeBlockedFromDeploying";

ChartJS.register(...registerables);

export function Home() {

  const [isLoaded, setIsLoaded] = useState(false);
  const [builds, setBuilds] = useState([]);
  
  useEffect(() => {
    (async () => {
      const response = await fetch(`api/builds/`);
      const data = await response.json();
      
      setBuilds(data);
      
      setIsLoaded(true);      
    })();
  }, [])
  
  const timePeriodDays = 7;
  const numberOfPeriods = 6;

  if (!isLoaded)
    return <p>Loading...</p>
  
  return <>
    <p>Last {numberOfPeriods} weeks:</p>

    {builds.map(build =>
        <div className={"chart-container"}>
          <h2>{build.displayName}</h2>
          <BuildCounter buildId={build.buildDefinition} numberOfPeriods={numberOfPeriods} lengthOfPeriod={timePeriodDays}/>
          <BuildSpeed buildId={build.buildDefinition} numberOfPeriods={numberOfPeriods} lengthOfPeriod={timePeriodDays}/>
          <FailureRecoveryTime buildId={build.buildDefinition} numberOfPeriods={numberOfPeriods} lengthOfPeriod={timePeriodDays}/>
          <TimeBlockedFromDeploying buildId={build.buildDefinition} numberOfPeriods={numberOfPeriods} lengthOfPeriod={timePeriodDays}/>
        </div>
    )}
  </>
}
