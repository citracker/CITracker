/* revenuechart */
var options = {
	series: [{
		name: "Order",
		data: [20, 60, 38, 72, 45, 63, 43, 76]
	}, {
		name: "Sale",
		data: [15, 45, 75, 38, 85, 35, 62, 40]
	}],
	chart: {
		height: 265,
		type: 'line',
		zoom: {
			enabled: false
		},
		dropShadow: {
			enabled: true,
			enabledOnSeries: undefined,
			top: 5,
			left: 0,
			blur: 3,
			color: '#000',
			opacity: 0.1
		},
	},
	dataLabels: {
		enabled: false
	},
	legend: {
		position: "top",
		horizontalAlign: "center",
		offsetX: -15,
		fontWeight: "bold",
	},
	stroke: {
		curve: 'smooth',
		width: '3',
		dashArray: [0, 5],
	},
	grid: {
		borderColor: '#f2f6f7',
	},
	colors: ["rgb(98, 89, 202)", "rgba(98, 89, 202, 0.3)"],
	yaxis: {
		title: {
			// text: 'Statistics',
			style: {
				color: '#adb5be',
				fontSize: '14px',
				fontFamily: 'poppins, sans-serif',
				fontWeight: 600,
				cssClass: 'apexcharts-yaxis-label',
			},
		},
	},
	xaxis: {
		type: 'month',
		categories: ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug'],
		axisBorder: {
			show: true,
			color: 'rgba(119, 119, 142, 0.05)',
			offsetX: 0,
			offsetY: 0,
		},
		axisTicks: {
			show: true,
			borderType: 'solid',
			color: 'rgba(119, 119, 142, 0.05)',
			width: 6,
			offsetX: 0,
			offsetY: 0
		},
		labels: {
			rotate: -90
		}
	}
};
document.getElementById('revenue-chart').innerHTML = ''
var chart3 = new ApexCharts(document.querySelector("#revenue-chart"), options);
chart3.render();

function revenueChart() {
	chart3.updateOptions({
		colors: ["rgb(" + myVarVal + ")", "rgba(" + myVarVal + ", 0.3)"],
	})
}

/* stroked circular gauge */
var options = {
    series: [83],
    chart: {
        height: 285,
        type: 'radialBar',
        offsetY: -10
    },
    colors: ["#6259ca"],
    plotOptions: {
        radialBar: {
            startAngle: -135,
            endAngle: 135,
            dataLabels: {
                name: {
                    fontSize: '16px',
                    color: undefined,
                    offsetY: 10
                },
                value: {
                    offsetY: 0,
                    fontSize: '22px',
                    color: undefined,
                    formatter: function (val) {
                        return val + "%";
                    }
                }
            }
        }
    },
    fill: {
        type: 'gradient',
        gradient: {
            shade: 'dark',
            shadeIntensity: 0.15,
            inverseColors: false,
            opacityFrom: 1,
            opacityTo: 1,
            stops: [0, 50, 65, 91]
        },
    },
    stroke: {
        dashArray: 4
    },
    labels: [''],
};
document.getElementById('recentorders').innerHTML = ''
var chart2 = new ApexCharts(document.querySelector("#recentorders"), options);
chart2.render();
function recentorders() {
	chart2.updateOptions({
		colors: ["rgb(" + myVarVal + ")"],
	})
}

/* country wise sales */
(function () {

    // Users by country map
    var markers = [{
        name: 'Russia',
        coords: [61, 105],
        style: {
            fill: '#6259ca'
        }
    },
    {
        name: 'Geenland',
        coords: [72, -42],
        style: {
            fill: '#6259ca'
        }
    },
    {
        name: 'Canada',
        coords: [56, -106],
        style: {
            fill: '#6259ca'
        }
    },
    {
        name: 'Palestine',
        coords: [31.5, 34.8],
        style: {
            fill: '#6259ca'
        }
    },
    {
        name: 'Brazil',
        coords: [-14.2350, -51.9253],
        style: {
            fill: '#6259ca'
        }
    },
    ];
    var map = new jsVectorMap({
        map: 'world_merc',
        selector: '#users-map',
        markersSelectable: true,

        onMarkerSelected(index, isSelected, selectedMarkers) {
            console.log(index, isSelected, selectedMarkers);
        },

        // -------- Labels --------
        labels: {
            markers: {
                render: function (marker) {
                    return marker.name
                },
            },
        },

        // -------- Marker and label style --------
        markers: markers,
        markerStyle: {
            hover: {
                stroke: "#DDD",
                strokeWidth: 3,
                fill: '#5b51cd'
            },
            selected: {
                fill: '#ff525d'
            }
        },
        markerLabelStyle: {
            initial: {
                fontFamily: 'Poppins',
                fontSize: 13,
                fontWeight: 500,
                fill: '#35373e',
            },
        },
    })

})();