//out vars
var series: any = new Object();
var choiceOps = {
    maxItemCount: 10,
    removeItemButton: true,
    duplicateItemsAllowed: false,
    paste: false,
    searchResultLimit: 10,
    shouldSort: false,
    placeholderValue: 'Seleccione áreas',
    searchPlaceholderValue: 'Buscar datos',
    loadingText: 'Cargando...',
    noResultsText: 'Sin resultados',
    noChoicesText: 'Sin opciones a elegir',
    itemSelectText: 'Presione para seleccionar',
    maxItemText: (maxItemCount: number) => {
        return `Máximo ${maxItemCount} valores`;
    },
    fuseOptions: {
        include: 'score'
    }
};
const etl = document.getElementById('tl');
choiceOps.placeholderValue = 'Variables semáforo';
const tl = new Choices(etl, choiceOps);
const semaforo = !document.getElementById('semaforo').classList.contains('d-none');
//passive support
var supportsPassiveOption = false;
try {
    var opts = Object.defineProperty({}, 'passive', {
        get: function () {
            supportsPassiveOption = true;
            return;
        }
    });
    var noop = function () { };
    window.addEventListener('testPassiveEventSupport', noop, opts);
    window.removeEventListener('testPassiveEventSupport', noop, opts);
} catch (e) { }
//CHART
var chart: any = am4core.create("chartdiv", am4charts.XYChart);
am4core.ready(async function () {
    am4core.useTheme(am4themes_kelly);
    //chart.scrollbarX = new am4core.Scrollbar();
    //chart.scrollbarY = new am4core.Scrollbar();
    chart.language.locale = am4lang_es_ES;
    var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
    dateAxis.dataFields.category = 'date';
    //dateAxis.dateFormats.setKey("day", "MM");
    //dateAxis.dateFormats.setKey("month", "MMM");
    dateAxis.dateFormats.setKey('year', 'yy');
    dateAxis.periodChangeDateFormats.setKey('month', 'MMM yy');
    dateAxis.tooltipDateFormat = 'dd MMM, yyyy';
    dateAxis.renderer.minGridDistance = 40;
    var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
    chart.legend = new am4charts.Legend();
    chart.cursor = new am4charts.XYCursor();
    chart.exporting.menu = new am4core.ExportMenu();
    await fetch('/ambiental/exportmenu').then(j => j.json())
        .then(menu => chart.exporting.menu.items = menu);
});
//LOAD DATA
async function fetchData(url:string, tag:string, name:string) {
    return await fetch(url)
        .then(data => data.json())
        .then(j => {
            var counter = 0;
            chart.data.map((c: any) => {
                if (counter < j.length && c.date == j[counter].date) {
                    c[tag] = j[counter].value;
                    counter++;
                }
            });
            series[tag] = chart.series.push(new am4charts.LineSeries());
            series[tag].dataFields.valueY = tag;
            series[tag].dataFields.dateX = 'date';
            series[tag].name = name;
            series[tag].tooltipText = '{name}: [bold]{valueY}[/]';
            series[tag].showOnInit = false;
        });
}
async function loadData(values: any, event: any, boolpsmb: boolean) {
    psmb.disable();
    variables.disable();
    if (semaforo) tl.disable();
    var sd = $('#start').val(), ed = $('#end').val();
    var x:any = [], st = 0, rd = 0, grp = true;
    if (boolpsmb) {
        //var tag, area tag, var name, area name, group value
        x = [null, event.value, null, event.label, null];
        rd = st + 4;
    } else {
        x = [event.value, null, event.label, null, event.groupValue === "Fitoplancton" ? "fito" : "graph"];
        st++;
        grp = false;
    }
    var nd = st + 2;
    var promises = values.map((e:any) => {
        x[st] = e.value;
        x[nd] = e.label;
        if (grp) x[rd] = e.groupId === 2 ? "fito" : "graph";
        var tag = `${x[0]}_${x[1]}`;
        var name = `${x[2]} ${x[3]}`;
        var url = `/ambiental/${x[4]}data?area=${x[1]}&var=${x[0]}&start=${sd}&end=${ed}`;
        return fetchData(url, tag, name);
    });
    Promise.all(promises).then(_ => {
        psmb.enable();
        variables.enable();
        if (semaforo) tl.enable();
    });
}
function clickMap(e:any) {
    if (e !== undefined && polygons[e.detail.value] !== undefined)
        google.maps.event.trigger(polygons[e.detail.value], 'click', {});
}
//PSMB SELECT
const epsmb = document.getElementById('psmb');
epsmb.addEventListener('addItem', (event: any) => {
    loadData(variables.getValue(), event.detail, true);
    clickMap(event);
}, supportsPassiveOption ? { passive: true } : false);
epsmb.addEventListener('removeItem', (event: any) => {
    variables.getValue(true).forEach((e:any) => {
        var name = `${e}_${event.detail.value}`;
        chart.series.removeIndex(chart.series.indexOf(series[name])).dispose();
    });
    clickMap(event);
}, supportsPassiveOption ? { passive: true } : false);

async function getList(name:string) {
    return await fetch(`/ambiental/${name}list`)
        .then(r => r.json())
        .catch(e => console.error(e));
}
const psmb = new Choices(epsmb, choiceOps);
psmb.setChoices(async () => await getList('psmb'));
//VARIABLE SELECT
const evariable = document.getElementById('variable');
evariable.addEventListener('addItem', (event:any) =>
    loadData(psmb.getValue(), event.detail, false)
    , supportsPassiveOption ? { passive: true } : false);
evariable.addEventListener('removeItem', (event: any) => {
    psmb.getValue(true).forEach((e:any) => {
        var name = `${event.detail.value}_${e}`;
        chart.series.removeIndex(chart.series.indexOf(series[name])).dispose();
    });
}, supportsPassiveOption ? { passive: true } : false);
choiceOps.placeholderValue = 'Seleccione variables';
const variables = new Choices(evariable, choiceOps);
variables.setChoices(async () => await getList('variable'));
//SEMAFORO select
function getParam(a:string, x:any) {
    switch (a) {
        case '11':
            return `&t=${x.value.slice(1, 2)}`;
        case '12':
            return `&l=${x.value}`;
        case '13':
        case '16':
            return `&s=${x.value}`;
        case '15':
            return `&rs=${x.value}`;
        default:
            return '';
    };
}
if (semaforo) {
    etl.addEventListener('addItem', (event:any) => {
        if (event.detail.groupValue === 'Análisis') {
            var a = event.detail.value;
            var tls = tl.getValue();
            var psmbs = tls.filter((x:any) => x.groupId === 2);
            var sps = tls.filter((x: any) => x.groupId === 3);
            if (psmbs.length !== 0 && sps.length !== 0) {
                var id = event.detail.id;
                var nm = event.detail.groupValue;
                console.log(event.detail);
                var xs = id == 0 ?
                    { value: '', name: '' }
                    : tls.filter((x: any) => x.groupId === id);
                var xs = tls.filter((x: any) => x.groupId === id);
                if (xs.length !== 0) {
                    psmb.disable();
                    variables.disable();
                    tl.disable();
                    var sd = $('#start').val();
                    var ed = $('#end').val();
                    var promises = psmbs.map((psmb: any) =>
                        sps.map((sp: any) =>
                            xs.map((x: any) => {
                                var tag = [a, psmb.value, sp.value, x.value].join('_');
                                var name = [event.detail.name, psmb.name, sp.name, x.name].join(' ');
                                var url = `/ambiental/tldata?a=${a}&psmb=${psmb.value}&sp=${sp.value}${getParam(a, x)}&start=${sd}&end=${ed}`;
                                return fetchData(url, tag, name);
                            })));
                    Promise.all(promises).then(r => {
                        chart.invalidateData();
                        psmb.enable();
                        variables.enable();
                        tl.enable();
                    });
                } else {
                    alert(`${nm} no seleccionada, seleccione requerimientos y luego análisis`);
                }
            } else {
                tl.removeActiveItemsByValue(a);
                alert('PSMB y especie no seleccionados, seleccione requerimientos y luego análisis');
            }
        }
    }, supportsPassiveOption ? { passive: true } : false);
    etl.addEventListener('removeItem', (event: any) => {
        if (event.detail.groupValue === 'Análisis') {
            var a = event.detail.value;
            var tls = tl.getValue();
            var psmbs = tls.filter((x: any) => x.groupId === 2);
            var sps = tls.filter((x: any) => x.groupId === 3);
            if (psmbs.length !== 0 && sps.length !== 0) {
                psmb.disable();
                variables.disable();
                tl.disable();
                var id = a - 7;
                var xs = tls.filter((x: any) => x.groupId === id);
                if (xs.length !== 0) {
                    psmbs.forEach((psmb:any) =>
                        sps.forEach((sp:any) =>
                            xs.forEach((x: any) => {
                                var name = [a, psmb.value, sp.value, x.value].join('_');
                                chart.series.removeIndex(chart.series.indexOf(series[name])).dispose();
                            })));
                }
            }
        }
    }, supportsPassiveOption ? { passive: true } : false);
    tl.setChoices(async () => await getList('tl'));
}
//MAP
function Area(path: google.maps.LatLng[] | google.maps.MVCArray<google.maps.LatLng>) {
    return (google.maps.geometry.spherical.computeArea(path) / 10000).toFixed(2);
}
const selected = 'red';
function addListenerOnPolygon(e:any) {
    if ($.isEmptyObject(e)) {
        psmb.getValue(true).includes(this.zIndex.toString()) ?
            this.setOptions({ fillColor: selected, strokeColor: selected }) :
            this.setOptions({ fillColor: undefined, strokeColor: undefined });
    } else {
        if (psmb.getValue(true).includes(this.zIndex.toString())) {
            this.setOptions({ fillColor: undefined, strokeColor: undefined });
            psmb.removeActiveItemsByValue(this.zIndex.toString());
        } else {
            this.setOptions({ fillColor: selected, strokeColor: selected });
            psmb.setChoiceByValue(this.zIndex.toString());
        }
    }
};
function flatten(items:any[]) {
    const flat:any[] = [];
    items.forEach(item => {
        if (Array.isArray(item)) {
            flat.push(...flatten(item));
        } else {
            flat.push(item);
        }
    });
    return flat;
}
function getBounds(positions:any[]) {
    var bounds = new google.maps.LatLngBounds();
    flatten(positions).forEach(p => bounds.extend(p));
    return bounds;
}
var map = new google.maps.Map(document.getElementById('map'), {
    mapTypeId: 'terrain'
});
var infowindow = new google.maps.InfoWindow({
    maxWidth: 150
});
var polygons:any = {};
window.onload = async function initMap() {
    var bnds: google.maps.LatLngBounds = new google.maps.LatLngBounds();
    await fetch('/ambiental/mapdata')
        .then(r => r.json())
        .then(data => {
            return data.map((dato:any) => {
                var consessionPolygon = new google.maps.Polygon({
                    paths: dato.position,
                    zIndex: dato.id
                });
                consessionPolygon.setMap(map);
                consessionPolygon.addListener('click', addListenerOnPolygon);
                polygons[dato.id] = consessionPolygon;
                var center = getBounds(dato.position).getCenter();
                if(dato.id < 4) bnds.extend(center);
                var marker = new google.maps.Marker({
                    position: center,
                    title: `${dato.name} ${dato.id}`
                });
                marker.addListener('click', (marker =>
                    () => {
                        var content =
                            `<h4>${dato.name}</h4><table><tr><th>Código</th><td align="right">${dato.id}</td></tr>`;
                        if (dato.comuna !== null)
                            content +=
                                `<tr><th>Comuna</th><td align="right">${dato.comuna}</td></tr>`;
                        if (dato.provincia !== null)
                            content +=
                                `<tr><th>Provincia</th><td align="right">${dato.provincia}</td></tr>`;
                        content +=
                            `<tr><th>Región</th><td align="right">${dato.region}</td>
</tr><tr><th>Área (ha)</th>
<td align="right">${Area(consessionPolygon.getPath().getArray())}</td>
</tr>
<tr><th>Fuentes</th><td></td></tr>
<tr><td>Sernapesca</td>
<td align="right">
<a target="_blank" href="https://www.sernapesca.cl">
<img src="../images/ico/sernapesca.svg" height="30" /></a></td></tr>
<tr><td>PER Mitilidos</td>
<td align="right">
<a target="_blank" href="https://www.mejillondechile.cl">
<img src="../images/ico/mejillondechile.min.png" height="30" /></a></td></tr>
<tr><td>Subpesca</td>
<td align="right">
<a target="_blank" href="https://www.subpesca.cl">
<img src="../images/ico/subpesca.png" height="30" /></a></td></tr>`;
                        infowindow.setContent(content);
                        infowindow.open(map, marker);
                        //map.setCenter(marker.getPosition());
                    })(marker));
                return marker;
            });
        }).then(m =>
            new MarkerClusterer(map, m, { imagePath: '/images/markers/m' })
        ).then(_ => {
            map.fitBounds(bnds);
            map.setCenter(bnds.getCenter());
            loadDates();
            variables.setChoiceByValue('t');
            psmb.setChoiceByValue('1');
        });
};
function loadDates() {
    var sd = $('#start').val();
    var ed = $('#end').val();
    var current = moment(sd);
    var max = moment(ed);
    while (current <= max) {
        chart.data.push({ date: current.format('yyyy-MM-DD') });
        current.add(1, 'days');
    }
}
//DATES
$('.input-daterange').datepicker({
    inputs: $('.actual_range'),
    format: 'yyyy-mm-dd',
    language: 'es',
    startDate: $('#start').val().toString(),
    endDate: $('#end').val().toString()
}).on('changeDate', (_:any) => {
    var vars = variables.getValue(true);
    variables.removeActiveItems();
    chart.data = [];
    loadDates();
    vars.forEach((v:any) => variables.setChoiceByValue(v));
});