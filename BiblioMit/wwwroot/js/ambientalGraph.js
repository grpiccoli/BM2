var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var loaderStart = () => {
    document.getElementById('preloader-background').style.display = "block";
};
var loaderStop = () => {
    document.getElementById('preloader-background').style.display = "none";
};
loaderStart();
var Area = function (path) {
    return (google.maps.geometry.spherical.computeArea(path) / 10000).toFixed(2);
};
var flatten = function (items) {
    const flat = [];
    items.forEach(item => {
        if (Array.isArray(item)) {
            flat.push(...flatten(item));
        }
        else {
            flat.push(item);
        }
    });
    return flat;
};
var getBounds = function (positions) {
    var bounds = new google.maps.LatLngBounds();
    flatten(positions).forEach(p => bounds.extend(p));
    return bounds;
};
var selected = 'red';
var lang = $("html").attr("lang");
var esp = lang === 'es';
var choiceOps = {
    maxItemCount: 50,
    removeItemButton: true,
    duplicateItemsAllowed: false,
    paste: false,
    searchResultLimit: 10,
    shouldSort: false,
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
    choiceOps.maxItemText = (maxItemCount) => `Máximo ${maxItemCount} valores`;
}
var semaforo = !document.getElementById('semaforo').classList.contains('d-none');
var epsmb = document.getElementById('psmb');
var psmb = new Choices(epsmb, choiceOps);
var evariable = document.getElementById('variable');
var variables = new Choices(evariable, choiceOps);
var etl = document.getElementById('tl');
var tl = new Choices(etl, choiceOps);
var customvar = document.getElementById('variables');
var customvars = new Choices(customvar, choiceOps);
var map = new google.maps.Map(document.getElementById('map'), {
    mapTypeId: 'terrain'
});
var infowindow = new google.maps.InfoWindow({
    maxWidth: 500
});
var tableInfo = [];
var polygons = {};
var stats = {};
var circles = {};
var bnds = new google.maps.LatLngBounds();
var markers = [];
var clusters;
var showInfo = function (_e) {
    var id = this.zIndex;
    fetch(`/ambiental/getcontent?Name=${tableInfo[id].name}&Code=${tableInfo[id].code}&Commune=${tableInfo[id].comuna}&Province=${tableInfo[id].provincia}&Area=${Area(polygons[id].getPath().getArray())}`, {
        credentials: "same-origin"
    })
        .then(r => r.text())
        .then(t => {
        infowindow.setContent(t);
        infowindow.open(map, this);
    });
};
var addListenerOnPolygon = function (e) {
    if ($.isEmptyObject(e)) {
        psmb.getValue(true).includes(this.zIndex) ?
            this.setOptions({ fillColor: selected, strokeColor: selected }) :
            this.setOptions({ fillColor: undefined, strokeColor: undefined });
    }
    else {
        if (psmb.getValue(true).includes(this.zIndex)) {
            this.setOptions({ fillColor: undefined, strokeColor: undefined });
            psmb.removeActiveItemsByValue(this.zIndex);
        }
        else {
            this.setOptions({ fillColor: selected, strokeColor: selected });
            psmb.setChoiceByValue(this.zIndex);
        }
    }
};
var processMapData = function (dato) {
    var consessionPolygon = new google.maps.Polygon({
        paths: dato.position,
        zIndex: dato.id
    });
    consessionPolygon.setMap(map);
    consessionPolygon.addListener('click', addListenerOnPolygon);
    polygons[dato.id] = consessionPolygon;
    var center = getBounds(dato.position).getCenter();
    if (dato.id < 4)
        bnds.extend(center);
    var marker = new google.maps.Marker({
        position: center,
        title: `${dato.name} ${dato.id}`,
        zIndex: dato.id
    });
    tableInfo[dato.id] = {
        name: dato.name,
        comuna: dato.comuna,
        provincia: dato.provincia,
        code: dato.code
    };
    marker.addListener('click', showInfo);
    return marker;
};
var chart = am4core.create("chartdiv", am4charts.XYChart);
var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
dateAxis.dataFields.category = 'date';
var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
var chartloaded = false;
var loadChart = function (_) {
    am4core.useTheme(am4themes_kelly);
    chart.language.locale = esp ? am4lang_es_ES : am4lang_en_US;
    chart.scrollbarY = new am4core.Scrollbar();
    chart.scrollbarX = new am4core.Scrollbar();
    chart.zoomOutButton.align = "left";
    chart.zoomOutButton.valign = "bottom";
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
    chartloaded = true;
    chart.events.off('validated', loadChart);
};
chart.events.on('validated', loadChart);
var loadDates = function () {
    var sd = $('#start').val();
    var ed = $('#end').val();
    var current = moment(sd);
    var max = moment(ed);
    while (current <= max) {
        chart.data.push({ date: current.format('yyyy-MM-DD') });
        current.add(1, 'days');
    }
    return chart.data;
};
var fetchData = function (url, tag, name) {
    return __awaiter(this, void 0, void 0, function* () {
        if (!(tag in chart.exporting.dataFields)) {
            var parts = tag.split('_');
            var psmbid = parts.splice(1, 1)[0];
            var vartag = parts.join('_');
            chart.exporting.dataFields[tag] = name;
            return yield fetch(url)
                .then(data => data.json())
                .then(j => {
                var counter = 0;
                chart.data.forEach((c) => {
                    if (counter < j.length && c.date === j[counter].date) {
                        c[tag] = j[counter].value;
                        counter++;
                        if (counter == j.length) {
                            if (stats[vartag] == null)
                                stats[vartag] = {};
                            stats[vartag][psmbid] = j[counter - 1].value;
                        }
                    }
                });
                var serie = chart.series.push(new am4charts.LineSeries());
                serie.dataFields.valueY = tag;
                serie.dataFields.dateX = 'date';
                serie.name = name;
                serie.tooltipText = '{name}: [bold]{valueY}[/]';
                serie.showOnInit = false;
            }).catch((e) => {
                delete chart.exporting.dataFields[tag];
            });
        }
    });
};
var generatefetchData = function (v, p, sd, ed) {
    return __awaiter(this, void 0, void 0, function* () {
        var tag = `${v.value}_${p.value}`;
        var name = `${v.label} ${p.label}`;
        var url = `/ambiental/data?area=${p.value}&type=${v.value.charAt(0)}&id=${v.value.substring(1)}&start=${sd}&end=${ed}`;
        return yield fetchData(url, tag, name);
    });
};
var green = [0, 128, 0];
var red = [255, 0, 0];
var circlemax = 20000;
var circlemin = 5000;
var loadData = function (e, isPsmb) {
    return __awaiter(this, void 0, void 0, function* () {
        var arr = isPsmb ? variables.getValue().concat(customvars.getValue()) : psmb.getValue();
        if (arr.length === 0)
            return;
        loaderStart();
        var sd = $('#start').val(), ed = $('#end').val();
        var promises = isPsmb ?
            arr.map((v) => generatefetchData(v, e.detail, sd, ed)) :
            arr.map((p) => generatefetchData(e.detail, p, sd, ed));
        return Promise.all(promises).then(_ => {
            chart.invalidateData();
        });
    });
};
var redrawCircles = function () {
    removeCircles();
    var variable = variables.getValue(true).slice(-1)[0];
    var selectedpsmbs = psmb.getValue(true);
    if (variable != undefined && selectedpsmbs.length && Object.keys(stats).length) {
        var psmbStats = stats[variable];
        var min;
        var max;
        var selectpsmbStats = {};
        if (psmbStats != undefined) {
            selectedpsmbs.forEach((p) => {
                min = (min === undefined || psmbStats[p] < min) ? psmbStats[p] : min;
                max = (max === undefined || psmbStats[p] > max) ? psmbStats[p] : max;
                selectpsmbStats[p] = psmbStats[p];
            });
            Object.entries(selectpsmbStats).forEach(([k, v]) => {
                var marker = markers.find((m) => m.zIndex == k);
                var weight = min == max ? 1 : (v - min) / (max - min);
                var color = pickHex(red, green, weight);
                var rad = circlemin + (circlemax - circlemin) * weight;
                circles[k] = new google.maps.Circle({
                    strokeColor: "#" + color,
                    strokeOpacity: 0.8,
                    strokeWeight: 0.1,
                    fillColor: "#" + color,
                    fillOpacity: 0.35,
                    map: map,
                    center: marker.position,
                    radius: rad
                });
                google.maps.event.addListener(circles[k], 'mouseover', function () {
                    this.getMap().getDiv().setAttribute('title', `${v}`);
                });
                google.maps.event.addListener(circles[k], 'mouseout', function () {
                    this.getMap().getDiv().removeAttribute('title');
                });
            });
        }
    }
};
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
}
catch (e) { }
var passive = supportsPassiveOption ? { passive: true } : false;
var removeCircle = function (i) {
    var circle = circles[i];
    google.maps.event.clearListeners(circle, 'click_handler_name');
    google.maps.event.clearListeners(circle, 'drag_handler_name');
    google.maps.event.clearListeners(circle, 'mouseover');
    google.maps.event.clearListeners(circle, 'mouseout');
    circle.setRadius(0);
    circle.setMap(null);
    delete circles[i];
};
var removeCircles = () => {
    Object.keys(circles).forEach((k) => removeCircle(k));
};
var removeSeries = function (tag) {
    chart.series.values.forEach((v, i) => {
        if (v.dataFields.valueY === tag) {
            delete chart.exporting.dataFields[tag];
            chart.series.removeIndex(i).dispose();
        }
    });
};
customvar.addEventListener('addItem', (e) => __awaiter(this, void 0, void 0, function* () {
    var arr = psmb.getValue();
    if (arr.length === 0)
        return;
    loaderStart();
    var sd = $('#start').val(), ed = $('#end').val();
    var promises = arr.map((p) => __awaiter(this, void 0, void 0, function* () {
        var tag = `${e.detail.value}_${p.value}`;
        var name = `${e.detail.label} ${p.label}`;
        var url = `/ambiental/customdata?area=${p.value}&typeid=${e.detail.value}&start=${sd}&end=${ed}`;
        return yield fetchData(url, tag, name);
    }));
    Promise.all(promises).then(_ => {
        chart.invalidateData();
        redrawCircles();
    });
}), passive);
customvar.addEventListener('removeItem', (event) => {
    psmb.getValue(true).forEach((e) => {
        var tag = `${event.detail.value}_${e}`;
        removeSeries(tag);
    });
    redrawCircles();
}, passive);
evariable.addEventListener('addItem', (e) => __awaiter(this, void 0, void 0, function* () {
    yield loadData(e, false);
    redrawCircles();
}), passive);
evariable.addEventListener('removeItem', (event) => {
    psmb.getValue(true).forEach((e) => {
        var tag = `${event.detail.value}_${e}`;
        removeSeries(tag);
    });
    redrawCircles();
}, passive);
var clickMap = function (e) {
    if (e !== undefined && polygons[e.detail.value] !== undefined)
        google.maps.event.trigger(polygons[e.detail.value], 'click', {});
};
epsmb.addEventListener('addItem', (e) => __awaiter(this, void 0, void 0, function* () {
    yield loadData(e, true);
    clickMap(e);
    redrawCircles();
}), passive);
epsmb.addEventListener('removeItem', (event) => {
    variables.getValue(true).forEach((e) => {
        var tag = `${e}_${event.detail.value}`;
        removeSeries(tag);
    });
    clickMap(event);
    redrawCircles();
}, passive);
var getList = function (name) {
    return __awaiter(this, void 0, void 0, function* () {
        return yield fetch(`/api/static/json/${lang}/${name}list.json`)
            .then(r => r.json())
            .catch(e => console.error(e, name));
    });
};
var loaderStopped = function () {
    return new Promise((resolve, _) => {
        (function wait() {
            if (document.getElementById('preloader-background').style.display === "none")
                return resolve();
            setTimeout(wait, 400);
        })();
    });
};
var chartLoaded = function () {
    return new Promise((resolve, _) => {
        (function wait() {
            if (chartloaded)
                return resolve();
            setTimeout(wait, 400);
        })();
    });
};
var init = function () {
    return __awaiter(this, void 0, void 0, function* () {
        loadDates();
        var cuencadata = fetch(`/api/static/json/${lang}/cuencadata.json`)
            .then(r => r.json())
            .then(data => data.map(processMapData))
            .then(m => {
            markers = m;
            map.fitBounds(bnds);
            map.setCenter(bnds.getCenter());
        });
        var oceanvarlist = getList('oceanvar');
        var cuencalist = getList('cuenca');
        var comunadata = fetch(`/api/static/json/${lang}/comunadata.json`)
            .then(r => r.json())
            .then(data => data.map(processMapData))
            .then(m => markers = markers.concat(m));
        var comunalist = getList('comuna');
        var groupvarlist = getList('groupvar');
        var customvarlist = getList('customvar');
        var variablechoicesInit = Promise.all([oceanvarlist]).then(r => { variables.setChoices([r[0]]); return true; });
        var psmbchoicesInit = Promise.all([cuencalist]).then(r => { psmb.setChoices([r[0]]); return true; });
        var customvarInit = Promise.all([customvarlist]).then(r => { customvars.setChoices([r[0]]); return true; });
        let buildchart = Promise.all([variablechoicesInit, psmbchoicesInit, cuencadata]).then(r => {
            variables.setChoiceByValue('v0');
            psmb.setChoiceByValue(1);
            return chartLoaded();
        });
        if (semaforo) {
            var psmbdata = fetch(`/api/static/json/${lang}/psmbdata.json`)
                .then(r => r.json())
                .then(data => data.map(processMapData))
                .then(m => markers = markers.concat(m));
            var psmblist = getList('psmb');
            var genusvarlist = getList('genusvar');
            var speciesvarlist = getList('speciesvar');
            var tllist = getList('tl');
            var psmbchoices = Promise.all([psmbchoicesInit, comunalist, psmblist]).then(r => {
                psmb.setChoices(r[1].concat(r[2]));
                return true;
            });
            var variablechoices = Promise.all([variablechoicesInit, groupvarlist, genusvarlist, speciesvarlist]).then(r => {
                variables.setChoices([r[1], r[2], r[3]]);
                return true;
            });
            var tlchoices = Promise.all([tllist]).then(r => { tl.setChoices(r[0]); return true; });
            clusters = Promise.all([cuencadata, comunadata, psmbdata]).then(_ => new MarkerClusterer(map, markers, { imagePath: '/images/markers/m' }));
            var callDatas = function (a, sd, ed, psmbs, sps, tls, groupId) {
                var nogroup = groupId === 0;
                var promises = nogroup ?
                    psmbs.map((psmb) => sps.map((sp) => {
                        var tag = [a.value, psmb.value, sp.value].join('_');
                        if (!chart.series._values.some((v) => tag === v.dataFields.valueY)) {
                            var name = [a.label, psmb.label, sp.label.replace("<i>", "[bold font-style: italic]").replace("</i>", "[/]")].join(' ');
                            var url = `/ambiental/tldata?a=${a.value}&psmb=${psmb.value}&sp=${sp.value}&start=${sd}&end=${ed}`;
                            return fetchData(url, tag, name);
                        }
                    })) :
                    tls.filter((x) => Math.floor(x.value / 10) === groupId).map((x) => psmbs.map((psmb) => sps.map((sp) => {
                        var tag = [a.value, psmb.value, sp.value, x.value].join('_');
                        if (!chart.series._values.some((v) => tag === v.dataFields.valueY)) {
                            var name = [a.label, psmb.label, sp.label.replace("<i>", "[bold font-style: italic]").replace("</i>", "[/]"), x.label].join(' ');
                            var url = `/ambiental/tldata?a=${a.value}&psmb=${psmb.value}&sp=${sp.value}&v=${x.value}&start=${sd}&end=${ed}`;
                            return fetchData(url, tag, name);
                        }
                    })));
                return Promise.all(promises).then(r => r);
            };
            etl.addEventListener('addItem', (_e) => {
                var tls = tl.getValue();
                var analyses = tls.filter((x) => Math.floor(x.value / 10) === 1);
                if (analyses.length !== 0) {
                    var psmbs = tls.filter((x) => Math.floor(x.value / 10) === 2);
                    var sps = tls.filter((x) => Math.floor(x.value / 10) === 3);
                    if (psmbs.length !== 0 && sps.length !== 0) {
                        loaderStart();
                        var sd = $('#start').val();
                        var ed = $('#end').val();
                        analyses.forEach((a) => {
                            switch (a.value) {
                                case 14:
                                case 17:
                                    return callDatas(a, sd, ed, psmbs, sps, null, 0);
                                case 11:
                                    return callDatas(a, sd, ed, psmbs, sps, tls, 4);
                                case 12:
                                    return callDatas(a, sd, ed, psmbs, sps, tls, 5);
                                case 15:
                                    return callDatas(a, sd, ed, psmbs, sps, tls, 6);
                                case 13:
                                case 16:
                                    return callDatas(a, sd, ed, psmbs, sps, tls, 7);
                                default:
                                    return;
                            }
                        });
                        chart.invalidateData();
                    }
                }
            }, passive);
            etl.addEventListener('removeItem', (event) => {
                if (chart.series.values.length === 0)
                    return;
                var tags = chart.series.values.map((v) => v.dataFields.valueY);
                var id = event.detail.value;
                var removed = 0;
                tags.forEach((k, i) => {
                    if (k.match(/^[1-7][0-8]_[1-7][0-8]_[1-7][0-8](_[1-7][0-8])?$/g) && k.includes(id)) {
                        delete chart.exporting.dataFields[k];
                        chart.series.removeIndex(i - removed).dispose();
                        removed++;
                    }
                });
            }, passive);
            Promise.all([buildchart, psmbchoices, variablechoices, tlchoices, clusters, customvarInit]).then(_ => {
                chart.events.on('validated', loaderStop);
                loaderStop();
            });
        }
        else {
            var psmbchoices = Promise.all([psmbchoicesInit, comunalist]).then(r => {
                psmb.setChoices(r[1]);
                return true;
            });
            var variablechoices = Promise.all([variablechoicesInit, groupvarlist]).then(r => {
                variables.setChoices([r[1]]);
                return true;
            });
            clusters = Promise.all([cuencadata, comunadata]).then(_ => new MarkerClusterer(map, markers, { imagePath: '/images/markers/m' }));
            Promise.all([buildchart, psmbchoices, variablechoices, clusters, customvarInit]).then(_ => {
                chart.events.on('validated', loaderStop);
                loaderStop();
            });
        }
    });
};
init();
$('.input-daterange').datepicker({
    inputs: Array.from($('.actual_range')),
    format: 'yyyy-mm-dd',
    language: lang,
    startDate: $('#start').val().toString(),
    endDate: $('#end').val().toString()
}).on('changeDate', (_) => {
    var vars = variables.getValue(true);
    variables.removeActiveItems();
    if (semaforo) {
        var tls = tl.getValue(true);
        tl.removeActiveItems();
    }
    chart.data = [];
    loadDates();
    vars.forEach((v) => variables.setChoiceByValue(v));
    if (semaforo) {
        tls.forEach((v) => tl.setChoiceByValue(v));
    }
});
var CreateTableFromJSON = function (json) {
    var col = [];
    for (var i = 0; i < json.length; i++) {
        for (var key in json[i]) {
            if (col.indexOf(key) === -1) {
                col.push(key);
            }
        }
    }
    var table = document.createElement("table");
    table.className = "table";
    table.id = "newTable";
    var tr = table.insertRow(-1);
    for (var i = 0; i < col.length; i++) {
        var th = document.createElement("th");
        th.innerHTML = col[i];
        tr.appendChild(th);
    }
    for (var i = 0; i < json.length; i++) {
        tr = table.insertRow(-1);
        for (var j = 0; j < col.length; j++) {
            var tabCell = tr.insertCell(-1);
            if (json[i][col[j]])
                tabCell.innerHTML = json[i][col[j]];
        }
    }
    var divContainer = document.getElementById("results");
    divContainer.appendChild(table);
};
var fetchPlankton = function () {
    var sd = $('#start').val();
    var ed = $('#end').val();
    var promises = psmb.getValue(true).map((p) => fetch('/ambiental/BuscarInformes?' + `id=${p}&start=${sd}&end=${ed}`).then(r => r.json()));
    Promise.all(promises).then(j => {
        var divContainer = document.getElementById("results");
        divContainer.innerHTML = "";
        j.forEach(i => CreateTableFromJSON(i));
    });
    $("#table2excel").show();
};
var tableToExcel = (function () {
    var uri = 'data:application/vnd.ms-excel;base64,', template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table>{table}</table></body></html>', base64 = function (s) { return window.btoa(unescape(encodeURIComponent(s))); }, format = function (s, c) { return s.replace(/{(\w+)}/g, function (_, p) { return c[p]; }); };
    return function (table, name) {
        if (!table.nodeType)
            table = document.getElementById(table);
        var ctx = { worksheet: name || 'Worksheet', table: table.innerHTML };
        window.location.href = uri + base64(format(template, ctx));
    };
})();
$('#fetchPlankton').click(fetchPlankton);
$('#table2excel').click(function () {
    tableToExcel('newTable', 'Ensayos');
});
document.getElementById('legenddiv').addEventListener('DOMSubtreeModified', function (_e) {
    document.getElementById("legenddiv").style.height = chart.legend.contentHeight + "px";
});
document.getElementById("pin-switch").addEventListener('change', function () {
    return __awaiter(this, void 0, void 0, function* () {
        if (this.checked) {
            (yield clusters).addMarkers(markers);
        }
        else {
            (yield clusters).clearMarkers();
            Object.values(markers).forEach((m) => {
                m.setMap(null);
            });
        }
    });
});
document.getElementById("polygon-switch").addEventListener('change', function () {
    return __awaiter(this, void 0, void 0, function* () {
        if (this.checked) {
            Object.values(polygons).forEach((m) => {
                m.setMap(map);
            });
        }
        else {
            Object.values(polygons).forEach((m) => {
                m.setMap(null);
            });
        }
    });
});
document.getElementById("stat-switch").addEventListener('change', function () {
    return __awaiter(this, void 0, void 0, function* () {
        if (this.checked) {
            Object.values(circles).forEach((m) => {
                m.setMap(map);
            });
        }
        else {
            Object.values(circles).forEach((m) => {
                m.setMap(null);
            });
        }
    });
});
//# sourceMappingURL=ambientalGraph.js.map