

  /* BTC Chart */
  var total = {
    chart: {
      type: "area",
      height: 130,
      sparkline: {
        enabled: true,
      },
      dropShadow: {
        enabled: true,
        enabledOnSeries: undefined,
        top: 0,
        left: 0,
        blur: 1,
        color: "#fff",
        opacity: 0.05,
      },
    },
    stroke: {
      show: true,
      curve: "smooth",
      lineCap: "butt",
      colors: undefined,
      width: 2,
      dashArray: 0,
    },
    fill: {
      gradient: {
        enabled: false,
      },
    },
    grid: {
      padding: {
        bottom: 10,
      },
    },
    series: [
      {
        name: "Value",
        data: [54, 38, 56, 35, 65, 43, 53, 45, 62, 80, 35, 48],
      },
    ],
    yaxis: {
      min: 0,
      show: false,
    },
    xaxis: {
      axisBorder: {
        show: false,
      },
    },
    yaxis: {
      axisBorder: {
        show: false,
      },
    },
    colors: ["rgba(98, 89, 202,0.7)"],
  };
  document.getElementById('bitcoinchart').innerHTML = ''
  var total = new ApexCharts(document.querySelector("#bitcoinchart"), total);
  total.render();
  
  function bitcoinchart() {
    total.updateOptions({
      colors: ["rgb(" + myVarVal + ")", "rgba(" + myVarVal + ", 0.3)"],
    })
  }
  // bitcoinchart()
  
  
  /* BTC Chart */

  /* ETH Chart */
  var total1 = {
    chart: {
      type: "area",
      height: 130,
      sparkline: {
        enabled: true,
      },
      dropShadow: {
        enabled: true,
        enabledOnSeries: undefined,
        top: 0,
        left: 0,
        blur: 1,
        color: "#fff",
        opacity: 0.05,
      },
    },
    stroke: {
      show: true,
      curve: "smooth",
      lineCap: "butt",
      colors: undefined,
      width: 2,
      dashArray: 0,
    },
    fill: {
      gradient: {
        enabled: false,
      },
    },
    series: [
      {
        name: "Value",
        data: [48, 35, 80, 62, 45, 53, 43, 65, 35, 56, 38, 54],
      },
    ],
    yaxis: {
      min: 0,
      show: false,
    },
    xaxis: {
      axisBorder: {
        show: false,
      },
    },
    yaxis: {
      axisBorder: {
        show: false,
      },
    },
    grid: {
      padding: {
        bottom: 10,
      },
    },
    colors: ["rgba(98, 89, 202,0.7)"],
    // tooltip: {
    //   enabled: false,
    // },
  };
  document.getElementById('ethereumchart').innerHTML = ''
  var total1 = new ApexCharts(document.querySelector("#ethereumchart"), total1);
  total1.render();
  
  function ethereumchart() {
    total1.updateOptions({
      colors: ["rgb(" + myVarVal + ")", "rgba(" + myVarVal + ", 0.3)"],
    })
  }
  /* ETH Chart */

  /* Dash Chart */
  var total2 = {
    chart: {
      type: "area",
      height: 130,
      sparkline: {
        enabled: true,
      },
      dropShadow: {
        enabled: true,
        enabledOnSeries: undefined,
        top: 0,
        left: 0,
        blur: 1,
        color: "#fff",
        opacity: 0.05,
      },
    },
    stroke: {
      show: true,
      curve: "smooth",
      lineCap: "butt",
      colors: undefined,
      width: 2,
      dashArray: 0,
    },
    grid: {
      padding: {
        bottom: 10,
      },
    },
    fill: {
      gradient: {
        enabled: false,
      },
    },
    series: [
      {
        name: "Value",
        data: [20, 50, 15, 35, 65, 43, 53, 45, 62, 22, 22, 55],
      },
    ],
    yaxis: {
      min: 0,
      show: false,
    },
    xaxis: {
      axisBorder: {
        show: false,
      },
    },
    yaxis: {
      axisBorder: {
        show: false,
      },
    },
    colors: ["rgba(98, 89, 202,0.7)"],
    // tooltip: {
    //   enabled: false,
    // },
  };
  document.getElementById('ripplechart').innerHTML = ''
  var total2 = new ApexCharts(document.querySelector("#ripplechart"), total2);
  total2.render();
  
  function ripplechart() {
    total2.updateOptions({
      colors: ["rgb(" + myVarVal + ")", "rgba(" + myVarVal + ", 0.3)"],
    })
  }
  /* Dash Chart */

  /* LTC Chart */
  var total3 = {
    chart: {
      type: "area",
      height: 130,
      sparkline: {
        enabled: true,
      },
      dropShadow: {
        enabled: true,
        enabledOnSeries: undefined,
        top: 0,
        left: 0,
        blur: 1,
        color: "#fff",
        opacity: 0.05,
      },
    },
    stroke: {
      show: true,
      curve: "smooth",
      lineCap: "butt",
      colors: undefined,
      width: 2,
      ltcArray: 0,
    },
    grid: {
      padding: {
        bottom: 10,
      },
    },
    fill: {
      gradient: {
        enabled: false,
      },
    },
    series: [
      {
        name: "Value",
        data: [80, 38, 56, 22, 45, 43, 62, 45, 62, 35, 35, 25],
      },
    ],
    yaxis: {
      min: 0,
      show: false,
    },
    xaxis: {
      axisBorder: {
        show: false,
      },
    },
    yaxis: {
      axisBorder: {
        show: false,
      },
    },
    colors: ["rgba(98, 89, 202,0.7)"],
    // tooltip: {
    //   enabled: false,
    // },
  };
  document.getElementById('dashchart').innerHTML = ''
  var total3 = new ApexCharts(document.querySelector("#dashchart"), total3);
  total3.render();
  
  function dashchart() {
    total3.updateOptions({
      colors: ["rgb(" + myVarVal + ")", "rgba(" + myVarVal + ", 0.3)"],
    })
  }
  /* LTC Chart */

  /* XRS Chart */
  var total4 = {
    chart: {
      type: "area",
      height: 130,
      sparkline: {
        enabled: true,
      },
      dropShadow: {
        enabled: true,
        enabledOnSeries: undefined,
        top: 0,
        left: 0,
        blur: 1,
        color: "#fff",
        opacity: 0.05,
      },
    },
    stroke: {
      show: true,
      curve: "smooth",
      lineCap: "butt",
      colors: undefined,
      width: 2,
      ltcArray: 0,
    },
    fill: {
      gradient: {
        enabled: false,
      },
    },
    series: [
      {
        name: "Value",
        data: [54, 38, 56, 35, 65, 43, 53, 45, 62, 80, 35, 48],
      },
    ],
    yaxis: {
      min: 0,
      show: false,
    },
    xaxis: {
      axisBorder: {
        show: false,
      },
    },
    yaxis: {
      axisBorder: {
        show: false,
      },
    },
    grid: {
      padding: {
        bottom: 10,
      },
    },
    colors: ["rgba(98, 89, 202,0.7)"],
    // tooltip: {
    //   enabled: false,
    // },
  };
  document.getElementById('neochart').innerHTML = ''
  var total4 = new ApexCharts(document.querySelector("#neochart"), total4);
  total4.render();
  
  function neochart() {
    total4.updateOptions({
      colors: ["rgb(" + myVarVal + ")", "rgba(" + myVarVal + ", 0.3)"],
    })
  }
  /* XRS Chart */

  /* GLM Chart */
  var total5 = {
    chart: {
      type: "area",
      height: 130,
      sparkline: {
        enabled: true,
      },
      dropShadow: {
        enabled: true,
        enabledOnSeries: undefined,
        top: 0,
        left: 0,
        blur: 1,
        color: "#fff",
        opacity: 0.05,
      },
    },
    stroke: {
      show: true,
      curve: "smooth",
      lineCap: "butt",
      colors: undefined,
      width: 2,
      ltcArray: 0,
    },
    fill: {
      gradient: {
        enabled: false,
      },
    },
    series: [
      {
        name: "Value",
        data: [10, 56, 35, 35, 65, 32, 53, 45, 48, 35, 56, 20],
      },
    ],
    yaxis: {
      min: 0,
      show: false,
    },
    grid: {
      padding: {
        bottom: 10,
      },
    },
    xaxis: {
      axisBorder: {
        show: false,
      },
    },
    yaxis: {
      axisBorder: {
        show: false,
      },
    },
    colors: ["rgba(98, 89, 202,0.7)"],
    // tooltip: {
    //   enabled: false,
    // },
  };
  document.getElementById('litecoinchart').innerHTML = ''
  var total5 = new ApexCharts(document.querySelector("#litecoinchart"), total5);
  total5.render();
  
  function litecoinchart() {
    total5.updateOptions({
      colors: ["rgb(" + myVarVal + ")", "rgba(" + myVarVal + ", 0.3)"],
    })
  }
  /* GLM Chart */
