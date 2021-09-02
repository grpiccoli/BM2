document.addEventListener("DOMContentLoaded", function () {
    function numberThousands(x: number,sep:string) {
        return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, sep);
    }
    var counter = document.getElementById("counter")
    if (counter) {
        var cnt = 0;
        var interval = setInterval(function () {
            cnt++;
            counter.innerHTML = cnt.toString();
        }, 10);
        fetch("/home/getanalyticsdata")
            .then(function (response) {
                return response.text();
            })
            .then(function (body) {
                var num = parseInt(body)
                if (!isNaN(num)) {
                    var sep = document.querySelector("html").lang == "es" ? "." : ",";
                    counter.innerHTML = numberThousands(num, sep);
                    clearInterval(interval);
                } else {
                    throw Error("not a number");
                }
            }).catch(function () {
                counter.innerHTML = ">3000";
                clearInterval(interval);
            });
    }
    /*document.getElementById('slim-toggler').addEventListener('click', () => {
        Array.from(document.querySelectorAll('#mdb-sidenav span[data-mdb-slim]')).forEach((e) => {
            e.classList.toggle("d-none");
        });
    });*/
});