import React, {useEffect, useState} from 'react'
import {Chart as ChartJS, registerables} from 'chart.js';
import {Line} from 'react-chartjs-2'
import {getBuildMetricsForPeriod} from "../metrics";

ChartJS.register(...registerables);

export default function BuildSpeed({buildId, numberOfPeriods, lengthOfPeriod}) {

  const [chartData, setChartData] = useState([]);

  useEffect(async () => {
    getBuildMetricsForPeriod(buildId, numberOfPeriods, lengthOfPeriod)
        .then(result => setChartData(result));
  }, []);

  if (chartData.length <= 0)
    return <></>;

  const data = {
    labels: chartData.map(c => `${c.from.getDate()}/${c.from.getMonth() + 1}`),
    datasets: [
      {
        label: "Average (mean)",
        type: "line",
        borderColor: "#84cb8a",
        backgroundColor: "#6ED77B",        
        hoverBorderColor: "rgb(175, 192, 192)",
        fill: false,
        tension: 0,
        data: chartData.map(c => c.successfulBuildTimeMean)
      },
      {
        label: "+ 1 σ",
        type: "line",
        backgroundColor: "#d8ecd9",
        borderColor: "transparent",
        pointRadius: 0,
        fill: 0,
        tension: 0,
        data: chartData.map(c => c.successfulBuildTimeMean + (+c.successfulBuildTimeStdDev))
      },
      {
        label: "-1 σ",
        type: "line",
        backgroundColor: "#d8ecd9",
        borderColor: "transparent",
        pointRadius: 0,
        fill: 0,
        tension: 0,
        data: chartData.map(c => c.successfulBuildTimeMean - (+c.successfulBuildTimeStdDev))
      },
    ]
  }

  const options = {plugins: {
      legend: {
        display: false
      },
    },
    scales: {
      yAxis: {
        position: "left",
        min: 0,
        max: 90,
        title: {
          display: true,
          text: "Minutes",
        },
        ticks: {
          stepSize: 5
        }
      },
      x: {
        offset: 0,
        title: {
          display: true,
          text: "Date",
        },
        ticks: {
          stepSize: 1,
        }
      }
    }
  };

  return (
      <div>
        <h2>Build Time</h2>
        <p>Time to produce a successful build</p>
        <Line data={data} options={options}/>
      </div>
  );
}
