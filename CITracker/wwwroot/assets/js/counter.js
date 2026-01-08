/* counter js */

window.addEventListener("load", function () {
    var counters = document.querySelectorAll(".count");
    var countersQuantity = counters.length;
    var counter = [];

    for (var i = 0; i < countersQuantity; i++) {
        counter[i] = parseInt(counters[i].innerHTML, 10);
    }

    var count = function (start, value, id) {
        var localStart = start;

        var intervalId = setInterval(function () {
            if (localStart < value) {
                localStart++;
                counters[id].innerHTML = localStart;
            } else {
                clearInterval(intervalId);
            }
        }, 5);
    };
    for (var j = 0; j < countersQuantity; j++) {
        count(0, counter[j], j);
    }
});

window.addEventListener("resize", function () {
});

/* counter js */



