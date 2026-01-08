/* Start:: Crypto Currencies BTC chart */
var btc = {
  chart: {
      type: 'area',
      height: 40,
      width: 50,
      sparkline: {
          enabled: true
      }
  },
  stroke: {
      show: true,
      curve: 'smooth',
      lineCap: 'butt',
      colors: undefined,
      width: 2,
      dashArray: 0,
  },
  fill: {
      type: 'gradient',
      gradient: {
          opacityFrom: 0.8,
          opacityTo: 0.05,
          stops: [0, 98],
      }
  },
  series: [{
      name: 'Value',
      data: [20, 14, 19, 10, 23, 20, 22, 9, 12]
  }],
  yaxis: {
      min: 0,
      show: false,
      axisBorder: {
          show: false
      },
  },
  xaxis: {
      show: false,
      axisBorder: {
          show: false
      },
  },
//   tooltip: {
//       enabled: false,
//   },
  colors: ["rgb(98, 89, 202)"],
}
document.getElementById('btcmarket').innerHTML = ''
var btc = new ApexCharts(document.querySelector("#btcmarket"), btc);
btc.render();

function btcmarket() {
	btc.updateOptions({
		colors: ["rgb(" + myVarVal + ")", "rgba(" + myVarVal + ", 0.3)"],
	})
}

/* Start:: Crypto Currencies ETH chart */
var eth = {
  chart: {
      type: 'area',
      height: 40,
      width: 50,
      sparkline: {
          enabled: true
      }
  },
  stroke: {
      show: true,
      curve: 'smooth',
      lineCap: 'butt',
      colors: undefined,
      width: 2,
      dashArray: 0,
  },
  fill: {
      type: 'gradient',
      gradient: {
          opacityFrom: 0.8,
          opacityTo: 0.1,
          stops: [0, 98],
      }
  },
  series: [{
      name: 'Value',
      data: [20, 20, 22, 9, 14, 19, 10, 25, 12]
  }],
  yaxis: {
      min: 0,
      show: false,
      axisBorder: {
          show: false
      },
  },
  xaxis: {
      show: false,
      axisBorder: {
          show: false
      },
  },
//   tooltip: {
//       enabled: false,
//   },
  colors: ["rgb(98, 89, 202)"],

}
document.getElementById('ethmarket').innerHTML = ''
var eth = new ApexCharts(document.querySelector("#ethmarket"), eth);
eth.render();

function ethmarket() {
	eth.updateOptions({
		colors: ["rgb(" + myVarVal + ")", "rgba(" + myVarVal + ", 0.3)"],
	})
}

/* Start:: Crypto Currencies XRP chart */
var xrp = {
  chart: {
      type: 'area',
      height: 40,
      width: 50,
      sparkline: {
          enabled: true
      }
  },
  stroke: {
      show: true,
      curve: 'smooth',
      lineCap: 'butt',
      colors: undefined,
      width: 2,
      dashArray: 0,
  },
  fill: {
      type: 'gradient',
      gradient: {
          opacityFrom: 0.8,
          opacityTo: 0.1,
          stops: [0, 98],
      }
  },
  series: [{
      name: 'Value',
      data: [20, 14, 20, 22, 9, 12, 19, 10, 25]
  }],
  yaxis: {
      min: 0,
      show: false,
      axisBorder: {
          show: false
      },
  },
  xaxis: {
      show: false,
      axisBorder: {
          show: false
      },
  },
//   tooltip: {
//       enabled: false,
//   },
  colors: ["rgb(98, 89, 202)"],

}
document.getElementById('xrpmarket').innerHTML = ''
var xrp = new ApexCharts(document.querySelector("#xrpmarket"), xrp);
xrp.render();

function xrpmarket() {
	xrp.updateOptions({
		colors: ["rgb(" + myVarVal + ")", "rgba(" + myVarVal + ", 0.3)"],
	})
}

/* Start:: Crypto Currencies LTC chart */
var ltc = {
  chart: {
      type: 'area',
      height: 40,
      width: 50,
      sparkline: {
          enabled: true
      }
  },
  stroke: {
      show: true,
      curve: 'smooth',
      lineCap: 'butt',
      colors: undefined,
      width: 2,
      dashArray: 0,
  },
  fill: {
      type: 'gradient',
      gradient: {
          opacityFrom: 0.8,
          opacityTo: 0.1,
          stops: [0, 98],
      }
  },
  series: [{
      name: 'Value',
      data: [20, 20, 22, 9, 12, 14, 19, 10, 25]
  }],
  yaxis: {
      min: 0,
      show: false,
      axisBorder: {
          show: false
      },
  },
  xaxis: {
      show: false,
      axisBorder: {
          show: false
      },
  },
//   tooltip: {
//       enabled: false,
//   },
  colors: ["rgb(98, 89, 202)"],

}
document.getElementById('ltcmarket').innerHTML = ''
var ltc = new ApexCharts(document.querySelector("#ltcmarket"), ltc);
ltc.render();

function ltcmarket() {
	ltc.updateOptions({
		colors: ["rgb(" + myVarVal + ")", "rgba(" + myVarVal + ", 0.3)"],
	})
}