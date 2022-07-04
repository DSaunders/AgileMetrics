import React, {useEffect, useState} from 'react'
import {Chart as ChartJS, registerables} from 'chart.js';
import {Line} from 'react-chartjs-2'
import {getBuildMetricsForPeriod} from "../metrics";

ChartJS.register(...registerables);

export default function BuildCounter({buildId, numberOfPeriods, lengthOfPeriod}) {

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
        label: "Failed",
        type: "line",
        borderColor: "#d3b0aa",
        backgroundColor: "#AC442D",
        hoverBorderColor: "rgb(175, 192, 192)",
        fill: false,
        tension: 0,
        data: chartData.map(c => c.failedBuilds)
      },
      {
        label: "Succeeded",
        type: "line",
        borderColor: "#c9eccc",
        backgroundColor: "#6ED77B",
        hoverBorderColor: "rgb(175, 192, 192)",
        fill: false,
        tension: 0,
        data: chartData.map(c => c.successfulBuilds)
      }
    ]
  }

  const options = {
    scales: {
      yAxis: {
        type: 'linear',
        position: "left",
        min: 0,
        max: 20,
        title: {
          display: true,
          text: "Number of Builds",
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
        <h2>Build Count</h2>
        <p>Number of builds performed</p>
        <Line data={data} options={options}/>
      </div>
  );
}
