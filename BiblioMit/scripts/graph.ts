//out vars
var lang = $("html").attr("lang");
var esp = lang === 'es';
var choiceOps:any = {
    maxItemCount: 50,
    removeItemButton: true,
    duplicateItemsAllowed: false,
    paste: false,
    searchResultLimit: 10,
    shouldSort: false,
    placeholderValue: esp ? 'Seleccione áreas' : 'Select areas',
    fuseOptions: {
        include: 'score'
    }
};
if (esp) {
    choiceOps.searchPlaceholderValue = 'Buscar';
    choiceOps.loadingText = 'Cargando...';
    choiceOps.noResultsText = 'Sin resultados';
    choiceOps.noChoicesText = 'Sin opciones a elegir';
    choiceOps.itemSelectText = 'Presione para seleccionar';
    choiceOps.maxItemText = (maxItemCount: number) => `Máximo ${maxItemCount} valores`;
}
const etl = document.getElementById('tl');
choiceOps.placeholderValue = esp ? 'Variables semáforo' : 'Semaforo Variables';
const tl = new Choices(etl, choiceOps);
const semaforo = !document.getElementById('semaforo').classList.contains('d-none');
var logged = document.getElementById('logoutForm') !== null;
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
$("#legenddiv").bind('DOMSubtreeModified', function (_e) {
    document.getElementById("legenddiv").style.height = chart.legend.contentHeight + "px";
});
am4core.ready(function () {
    am4core.useTheme(am4themes_kelly);
    if (esp) chart.language.locale = am4lang_es_ES;
    var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
    dateAxis.dataFields.category = 'date';
    //var sd = $('#start').val();
    //var ed = $('#end').val();
    //var dataSource = chart.dataSource;
    //dataSource.url = `/ambiental/graphdata?area=1&var=t&start=${sd}&end=${ed}`;
    //var ser = chart.series.push(new am4charts.LineSeries());
    //ser.dataFields.valueY = "value";
    //chart.dataSource.parser = am4core.JSONParser;
    chart.scrollbarX = new am4core.Scrollbar();
    //var dateslider = am4core.create("sliderdiv", am4core.Container);
    //chart.scrollbarX.parent = dateslider;
    //chart.scrollbarY = new am4core.Scrollbar();
    //chart.scrollbarY.parent = chart.leftAxesContainer;
    //dateAxis.dateFormats.setKey("day", "MM");
    //dateAxis.dateFormats.setKey("month", "MMM");
    dateAxis.dateFormats.setKey('year', 'yy');
    dateAxis.periodChangeDateFormats.setKey('month', 'MMM yy');
    dateAxis.tooltipDateFormat = 'dd MMM, yyyy';
    dateAxis.renderer.minGridDistance = 40;
    chart.yAxes.push(new am4charts.ValueAxis());
    chart.legend = new am4charts.Legend();
    var legendContainer = am4core.create("legenddiv", am4core.Container);
    legendContainer.width = am4core.percent(100);
    legendContainer.height = am4core.percent(100);
    chart.legend.parent = legendContainer;
    chart.cursor = new am4charts.XYCursor();
    chart.exporting.menu = new am4core.ExportMenu();
    if (!logged) {
        chart.exporting.formatOptions.getKey("png").disabled = true;
        chart.exporting.formatOptions.getKey("svg").disabled = true;
        chart.exporting.formatOptions.getKey("pdf").disabled = true;
        chart.exporting.formatOptions.getKey("json").disabled = true;
        chart.exporting.formatOptions.getKey("csv").disabled = true;
        chart.exporting.formatOptions.getKey("xlsx").disabled = true;
        chart.exporting.formatOptions.getKey("html").disabled = true;
        chart.exporting.formatOptions.getKey("pdfdata").disabled = true;
    }
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
            var serie = chart.series.push(new am4charts.LineSeries());
            serie.dataFields.valueY = tag;
            serie.dataFields.dateX = 'date';
            serie.name = name;
            serie.tooltipText = '{name}: [bold]{valueY}[/]';
            serie.showOnInit = false;
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
        x = [event.value, null, event.label, null, (event.groupValue === "Fitoplancton" || event.groupValue === "Phytoplankton") ? "fito" : "graph"];
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
const eall = document.querySelector('select.choice');
const epsmb = document.getElementById('psmb');
epsmb.addEventListener('addItem', (event: any) => {
    loadData(variables.getValue(), event.detail, true);
    clickMap(event);
}, supportsPassiveOption ? { passive: true } : false);
epsmb.addEventListener('removeItem', (event: any) => {
    variables.getValue(true).forEach((e:any) => {
        var name = `${e}_${event.detail.value}`;
        chart.series._values.forEach((v: any, i: number) => {
            if (v.dataFields.valueY === name)
                chart.series.removeIndex(i).dispose();
        });
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
        chart.series._values.forEach((v: any, i: number) => {
            if (v.dataFields.valueY === name)
                chart.series.removeIndex(i).dispose();
        });
    });
}, supportsPassiveOption ? { passive: true } : false);
choiceOps.placeholderValue = esp ? 'Seleccione variables' : 'Select Variables';
const variables = new Choices(evariable, choiceOps);
variables.setChoices(async () => await getList('variable'));
//SEMAFORO select
function getParam(a: string) {
    switch (a) {
        case '11':
            return 't';
        case '12':
            return 'l';
        case '13':
        case '16':
            return 's';
        case '15':
            return 'rs';
        default:
            return '';
    };
}
function callDatas(a: any, sd: any, ed: any, psmbs: any, sps: any, tls: any, groupId:number)
{
    var nogroup = groupId === 0;
    var promises = nogroup ?
        psmbs.map((psmb: any) =>
        sps.map((sp: any) => {
            var tag = [a.value, psmb.value, sp.value].join('_');
            if (!chart.series._values.some((v: any) => tag === v.dataFields.valueY)) {
                var name = [a.label, psmb.label, sp.label.replace("<i>","[bold font-style: italic]").replace("</i>","[/]")].join(' ');
                var url = `/ambiental/tldata?a=${a.value}&psmb=${psmb.value}&sp=${sp.value}&start=${sd}&end=${ed}`;
                return fetchData(url, tag, name);
            }
        })) :
        tls.filter((x: any) => x.groupId === groupId).map((x: any) =>
            psmbs.map((psmb: any) =>
                sps.map((sp: any) => {
                    var tag = [a.value, psmb.value, sp.value, x.value].join('_');
                    if (!chart.series._values.some((v: any) => tag === v.dataFields.valueY)) {
                        var name = [a.label, psmb.label, sp.label.replace("<i>", "[bold font-style: italic]").replace("</i>", "[/]"), x.label].join(' ');
                        var m = getParam(a.value);
                        console.log('hi');
                        var url = `/ambiental/tldata?a=${a.value}&psmb=${psmb.value}&sp=${sp.value}&${m}=${x.value}&start=${sd}&end=${ed}`;
                        return fetchData(url, tag, name);
                    }
                })));
    return Promise.all(promises).then(r => r);
}
if (semaforo) {
    etl.addEventListener('addItem', (_e: any) => {
        var tls = tl.getValue();
        var analyses = tls.filter((x: any) => x.groupId === 1);
        if (analyses.length !== 0) {
            var psmbs = tls.filter((x: any) => x.groupId === 2);
            var sps = tls.filter((x: any) => x.groupId === 3);
            if (psmbs.length !== 0 && sps.length !== 0) {
                psmb.disable();
                variables.disable();
                tl.disable();
                var sd = $('#start').val();
                var ed = $('#end').val();
                analyses.forEach((a: any) => {
                    switch (a.value) {
                        case '14':
                        case '17':
                            return callDatas(a, sd, ed, psmbs, sps, null, 0);
                        case '11':
                            return callDatas(a, sd, ed, psmbs, sps, tls, 4);
                        case '12':
                            return callDatas(a, sd, ed, psmbs, sps, tls, 5);
                        case '15':
                            return callDatas(a, sd, ed, psmbs, sps, tls, 6);
                        case '13':
                        case '16':
                            return callDatas(a, sd, ed, psmbs, sps, tls, 7);
                        default:
                            return;
                    }
                });
                chart.invalidateData();
                psmb.enable();
                variables.enable();
                tl.enable();
            }
        }
    }, supportsPassiveOption ? { passive: true } : false);
    etl.addEventListener('removeItem', (event: any) => {
        var id = event.detail.value;
        chart.series._values.forEach((v: any, i: number) => {
            var k = v.dataFields.valueY;
            if (k.match(/^[1-7][0-8]_[1-7][0-8]_[1-7][0-8](_[1-7][0-8])?$/g) && k.includes(id))
            chart.series.removeIndex(i).dispose();
        });
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
    maxWidth: 500
});
var polygons: any = {};
var table: any = [];
var titles = esp ?
    ["Código", "Comuna", "Provincia", "Región", "Área", "Fuentes"] :
    ["Code", "Commune", "Province", "Region", "Area", "Sources"];
function showInfo(_e: any) {
    var id = this.zIndex;
    var content =
        `<h4>${table[id].name}</h4><table class="table"><tr><th scope="row">${titles[0]}</th><td align="right">${table[id].code}</td></tr>`;
    if (table[id].comuna !== null)
        content +=
            `<tr><th scope="row">${titles[1]}</th><td align="right">${table[id].comuna}</td></tr>`;
    if (table[id].provincia !== null)
        content +=
            `<tr><th scope="row">${titles[2]}</th><td align="right">${table[id].provincia}</td></tr>`;
    content +=
        `<tr><th scope="row">${titles[3]}</th><td align="right">Los Lagos</td>
</tr><tr><th scope="row">${titles[4]} (ha)</th>
<td align="right">${Area(polygons[id].getPath().getArray())}</td>
</tr>
<tr><th scope="row">${titles[5]}</th><td></td></tr>
<tr><td>Sernapesca</td>
<td align="right">
<a target="_blank" href="https://www.sernapesca.cl">
<img src="../images/ico/sernapesca.svg" height="20" /></a></td></tr>
<tr><td>PER Mitílidos</td>
<td align="right">
<a target="_blank" href="https://www.mejillondechile.cl">
<img src="../images/ico/mejillondechile.min.png" height="20" /></a></td></tr>
<tr><td>Subpesca</td>
<td align="right">
<a target="_blank" href="https://www.subpesca.cl">
<img src="../images/ico/subpesca.png" height="20" /></a></td></tr>`;
    infowindow.setContent(content);
    infowindow.open(map, this);
}
window.onload = async function initMap() {
    var bnds: google.maps.LatLngBounds = new google.maps.LatLngBounds();
    await fetch('/ambiental/mapdata')
        .then(r => r.json())
        .then(data => data.map((dato:any) => {
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
                title: `${dato.name} ${dato.id}`,
                zIndex: dato.id
            });
            table[dato.id] = {
                name: dato.name,
                comuna: dato.comuna,
                provincia: dato.provincia,
                code: dato.code
            }
            marker.addListener('click', showInfo);
            return marker;
            })).then(m => new MarkerClusterer(map, m, { imagePath: '/images/markers/m' })
        ).then(_ => {
            map.fitBounds(bnds);
            map.setCenter(bnds.getCenter());
            loadDates();
            variables.setChoiceByValue('t');
            psmb.setChoiceByValue('1');
            if (logged) chart.exporting.menu = new am4core.ExportMenu();
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
    if (semaforo) {
        var tls = tl.getValue(true);
        tl.removeActiveItems();
    }
    chart.data = [];
    loadDates();
    vars.forEach((v: any) => variables.setChoiceByValue(v));
    if (semaforo) {
        tls.forEach((v: any) => tl.setChoiceByValue(v));
    }
});