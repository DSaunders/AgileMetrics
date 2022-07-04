import React, {useEffect, useState} from 'react'
import {Chart as ChartJS, registerables} from 'chart.js';
import {Line} from 'react-chartjs-2'
import {getBuildMetricsForPeriod} from "../metrics";

ChartJS.register(...registerables);

export default function TimeBlockedFromDeploying({buildId, numberOfPeriods, lengthOfPeriod}) {

  const [chartData, setChartData] = useState([]);

  useEffect(async () => {
    getBuildMetricsForPeriod(buildId, numberOfPeriods, lengthOfPeriod)
        .then(result => setChartData(result));
  }, []);

  if (chartData.length <= 0)
    return <></>;

  const data = {
    labels: chartData.map(c => `${c.from.getDate()}/${c.from.getMonth()+1}`),
    datasets: [
      {
        label: "Time",
        type: "line",
        borderColor: "#d3b0aa",
        backgroundColor: "#AC442D",
        hoverBorderColor: "rgb(175, 192, 192)",
        fill: false,
        tension: 0,
        data: chartData.map(c => c.timeUnableToDeploy)
      }
    ]
  }

  const options = {plugins: {
      legend: {
        display: false
      },
    },
    scales: {
      yAxis: {
        type: 'linear',
        position: "left",
        min: 0,
        max: 10,
        title: {
          display: true,
          text: "Working Hours",
        },
        ticks: {
          stepSize: 1
        }
      },
      xAxis: {
        title: {
          display: true,
          text: "Date",
        },
      }
    }
  };

  return (
      <div>
        <h2>Blocked Time</h2>
        <p>The amount of time we couldn't deploy to production due to a failing build</p>
        <Line data={data} options={options}/>
      </div>
  );
}
