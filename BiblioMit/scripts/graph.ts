//out vars
const loaderStart = () => {
    (<HTMLElement>document.getElementById('preloader-background')).style.display = "block";
}

const loaderStop = () => {
    (<HTMLElement>document.getElementById('preloader-background')).style.display = "none";
}
loaderStart();
var build = new Event("buildChart");
var cnt = 0;
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
//var logged = document.getElementById('logoutForm') !== null;
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
var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
dateAxis.dataFields.category = 'date';
var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
etl.addEventListener("buildChart", function () {
    am4core.useTheme(am4themes_kelly);
    chart.language.locale = esp ? am4lang_es_ES : am4lang_en_US;
    chart.scrollbarY = new am4core.Scrollbar();
    chart.scrollbarX = new am4core.Scrollbar();
    chart.zoomOutButton.align = "left";
    chart.zoomOutButton.valign = "bottom";
    //dateAxis.dateFormats.setKey("day", "MM");
    //dateAxis.dateFormats.setKey("month", "MMM");
    dateAxis.dateFormats.setKey('year', 'yy');
    dateAxis.periodChangeDateFormats.setKey('month', 'MMM yy');
    dateAxis.tooltipDateFormat = 'dd MMM, yyyy';
    dateAxis.renderer.minGridDistance = 40;
    chart.legend = new am4charts.Legend();
    var legendContainer = am4core.create("legenddiv", am4core.Container);
    legendContainer.width = am4core.percent(100);
    legendContainer.height = am4core.percent(100);
    chart.legend.parent = legendContainer;
    chart.cursor = new am4charts.XYCursor();
    chart.exporting.menu = new am4core.ExportMenu();
    if (!semaforo) {
        chart.exporting.formatOptions.getKey("png").disabled = true;
        chart.exporting.formatOptions.getKey("svg").disabled = true;
        chart.exporting.formatOptions.getKey("pdf").disabled = true;
        chart.exporting.formatOptions.getKey("json").disabled = true;
        chart.exporting.formatOptions.getKey("csv").disabled = true;
        chart.exporting.formatOptions.getKey("xlsx").disabled = true;
        chart.exporting.formatOptions.getKey("html").disabled = true;
        chart.exporting.formatOptions.getKey("pdfdata").disabled = true;
    }
}, false);
$("#legenddiv").bind('DOMSubtreeModified', function (_e) {
    document.getElementById("legenddiv").style.height = chart.legend.contentHeight + "px";
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
            chart.exporting.dataFields[tag] = name;
        });
}
async function loadData(values: any, event: any, boolpsmb: boolean) {
    loaderStart();
    //psmb.disable();
    //variables.disable();
    //if (semaforo) tl.disable();
    var sd = $('#start').val(), ed = $('#end').val();
    var x:any = [], st = 0;
    if (boolpsmb) {
        //var tag, area tag, var name, area name, group value
        x = [null, event.value, null, event.label];
    } else {
        x = [event.value, null, event.label, null];
        st++;
    }
    var nd = st + 2;
    var promises = values.map((e:any) => {
        x[st] = e.value;
        x[nd] = e.label;
        //if (boolpsmb) x[rd] = e.groupId;
        var tag = `${x[0]}_${x[1]}`;
        var name = `${x[2]} ${x[3]}`;
        var url = `/ambiental/data?area=${x[1]}&var=${x[0]}&start=${sd}&end=${ed}`;
        return fetchData(url, tag, name);
    });
    Promise.all(promises).then(_ => {
        //psmb.enable();
        //variables.enable();
        //if (semaforo) tl.enable();
        if (cnt === 0 && boolpsmb) {
            etl.dispatchEvent(build);
            cnt++;
            //loaderStop();
        } else {
            loaderStop();
        }
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
        .catch(e => console.error(e, name));
}
const psmb = new Choices(epsmb, choiceOps);
//psmb.setChoices(async () => await getList('psmbs'));
psmb.setChoices(async () => {
    var one = [await getList('cuenca')];
    var two = getList('comuna');
    if (semaforo) {
        var three = getList('psmb');
        return one.concat(await two).concat(await three)
    }
    return one.concat(await two);
});
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
        tls.filter((x: any) => x.id % 10 === groupId).map((x: any) =>
            psmbs.map((psmb: any) =>
                sps.map((sp: any) => {
                    var tag = [a.value, psmb.value, sp.value, x.value].join('_');
                    if (!chart.series._values.some((v: any) => tag === v.dataFields.valueY)) {
                        var name = [a.label, psmb.label, sp.label.replace("<i>", "[bold font-style: italic]").replace("</i>", "[/]"), x.label].join(' ');
                        var m = getParam(a.value);
                        var url = `/ambiental/tldata?a=${a.value}&psmb=${psmb.value}&sp=${sp.value}&${m}=${x.value}&start=${sd}&end=${ed}`;
                        return fetchData(url, tag, name);
                    }
                })));
    return Promise.all(promises).then(r => r);
}
if (semaforo) {
    etl.addEventListener('addItem', (_e: any) => {
        var tls = tl.getValue();
        var analyses = tls.filter((x: any) => x.id % 10 === 1);
        if (analyses.length !== 0) {
            var psmbs = tls.filter((x: any) => x.id % 10 === 2);
            var sps = tls.filter((x: any) => x.id % 10 === 3);
            if (psmbs.length !== 0 && sps.length !== 0) {
                loaderStart();
                //psmb.disable();
                //variables.disable();
                //tl.disable();
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
                loaderStop();
                //psmb.enable();
                //variables.enable();
                //tl.enable();
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
var Area = function (path: google.maps.LatLng[] | google.maps.MVCArray<google.maps.LatLng>) {
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
var flatten = function (items:any[]) {
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
var getBounds = function (positions:any[]) {
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
var showInfo = function (_e: any) {
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
var bnds: google.maps.LatLngBounds = new google.maps.LatLngBounds();
var markers :any[]= [];
var processMapData = function (dato: any) {
    var consessionPolygon = new google.maps.Polygon({
        paths: dato.position,
        zIndex: dato.id
    });
    consessionPolygon.setMap(map);
    consessionPolygon.addListener('click', addListenerOnPolygon);
    polygons[dato.id] = consessionPolygon;
    var center = getBounds(dato.position).getCenter();
    if (dato.id < 4) bnds.extend(center);
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
}
window.onload = function initMap() {
    fetch('/ambiental/cuencadata')
        .then(r => r.json())
        .then(data => data.map(processMapData))
        .then(m => {
            markers = m;
            map.fitBounds(bnds);
            map.setCenter(bnds.getCenter());
        }).then(async _ => {
            await fetch('/ambiental/comunadata')
                .then(r => r.json())
                .then(data => data.map(processMapData))
                .then(m => markers = markers.concat(m));
            if (semaforo)
                await fetch('/ambiental/psmbdata')
                    .then(r => r.json())
                    .then(data => data.map(processMapData))
                    .then(m => markers = markers.concat(m));
        }).then(_ => {
            new MarkerClusterer(map, markers, { imagePath: '/images/markers/m' });
            loadDates();
            variables.setChoiceByValue('v0');
            psmb.setChoiceByValue('1');
            loaderStop();
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
    return chart.data;
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
function CreateTableFromJSON(json: any) {
    // EXTRACT VALUE FOR HTML HEADER.
    var col = [];
    for (var i = 0; i < json.length; i++) {
        for (var key in json[i]) {
            if (col.indexOf(key) === -1) {
                col.push(key);
            }
        }
    }
    // CREATE DYNAMIC TABLE.
    var table = document.createElement("table");
    table.className = "table";
    table.id = "newTable";
    // CREATE HTML TABLE HEADER ROW USING THE EXTRACTED HEADERS ABOVE.
    var tr = table.insertRow(-1);                   // TABLE ROW.

    for (var i = 0; i < col.length; i++) {
        var th = document.createElement("th");      // TABLE HEADER.
        th.innerHTML = col[i];
        tr.appendChild(th);
    }
    // ADD JSON DATA TO THE TABLE AS ROWS.
    for (var i = 0; i < json.length; i++) {
        tr = table.insertRow(-1);
        for (var j = 0; j < col.length; j++) {
            var tabCell = tr.insertCell(-1);
            tabCell.innerHTML = json[i][col[j]];
        }
    }
    // FINALLY ADD THE NEWLY CREATED TABLE WITH JSON DATA TO A CONTAINER.
    var divContainer = document.getElementById("results");
    divContainer.appendChild(table);
}
function fetchPlankton() {
    var sd = $('#start').val();
    var ed = $('#end').val();
    var promises = psmb.getValue(true).map((p:any) => fetch('/ambiental/BuscarInformes', {
        method: 'POST',
        headers: new Headers({ 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' }),
        body: `id=${p}&start=${sd}&end=${ed}`
    }).then(r => r.json()));
    Promise.all(promises).then(j => {
        var divContainer = document.getElementById("results");
        divContainer.innerHTML = "";
        j.forEach(i => CreateTableFromJSON(i));
    });
}
var tableToExcel = (function () {
    var uri = 'data:application/vnd.ms-excel;base64,'
        , template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table>{table}</table></body></html>'
        , base64 = function (s:any) { return window.btoa(unescape(encodeURIComponent(s))) }
        , format = function (s:any, c:any) { return s.replace(/{(\w+)}/g, function (_:any, p:any) { return c[p]; }) }
    return function (table:any, name:any) {
        if (!table.nodeType) table = document.getElementById(table)
        var ctx = { worksheet: name || 'Worksheet', table: table.innerHTML }
        window.location.href = uri + base64(format(template, ctx))
    }
})();