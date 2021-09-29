//loader
var loaderStart = () => {
    (<HTMLElement>document.getElementById('preloader-background')).style.display = "block";
}
var loaderStop = () => {
    (<HTMLElement>document.getElementById('preloader-background')).style.display = "none";
}
loaderStart();
//maps
var Area = function (path: google.maps.LatLng[] | google.maps.MVCArray<google.maps.LatLng>) {
    return (google.maps.geometry.spherical.computeArea(path) / 10000).toFixed(2);
}
//flatten array
var flatten = function (items: any[]) {
    const flat: any[] = [];
    items.forEach(item => {
        if (Array.isArray(item)) {
            flat.push(...flatten(item));
        } else {
            flat.push(item);
        }
    });
    return flat;
}
//get bounderies from array of arrays
var getBounds = function (positions: any[]) {
    var bounds = new google.maps.LatLngBounds();
    flatten(positions).forEach(p => bounds.extend(p));
    return bounds;
}
var selected = 'red';
//get language
var lang = $("html").attr("lang");
var esp = lang === 'es';
//choice options
var choiceOps: any = {
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
    choiceOps.maxItemText = (maxItemCount: number) => `Máximo ${maxItemCount} valores`;
}
//is semaforo?
var semaforo = !document.getElementById('semaforo').classList.contains('d-none');
//choice elements//
//psmb load choices
var epsmb = document.getElementById('psmb');
var psmb = new Choices(epsmb, choiceOps);
//variable load choices
var evariable = document.getElementById('variable');
var variables = new Choices(evariable, choiceOps);
//semaforo
var etl = document.getElementById('tl');
var tl = new Choices(etl, choiceOps);
//custom variables
var customvar = document.getElementById('variables');
var customvars = new Choices(customvar, choiceOps);
//define info
var map = new google.maps.Map(document.getElementById('map'), {
    mapTypeId: 'terrain'
});
var infowindow = new google.maps.InfoWindow({
    maxWidth: 500
});
var tableInfo: any = [];
var polygons: any = {};
var stats: any = {};
var circles: any = {};
var bnds: google.maps.LatLngBounds = new google.maps.LatLngBounds();
var markers: any = [];
var clusters: any;
var showInfo = function (_e: any) {
    var id = this.zIndex;
    fetch(`/ambiental/getcontent?Name=${tableInfo[id].name
        }&Code=${tableInfo[id].code}&Commune=${tableInfo[id].comuna
        }&Province=${tableInfo[id].provincia}&Area=${Area(polygons[id].getPath().getArray())}`,
        {
            credentials: "same-origin"
        })
        .then(r => r.text())
        .then(t => {
            infowindow.setContent(t);
            infowindow.open(map, this);
        });
}
var addListenerOnPolygon = function(e: any) {
    if ($.isEmptyObject(e)) {
        psmb.getValue(true).includes(this.zIndex) ?
            this.setOptions({ fillColor: selected, strokeColor: selected }) :
            this.setOptions({ fillColor: undefined, strokeColor: undefined });
    } else {
        if (psmb.getValue(true).includes(this.zIndex)) {
            this.setOptions({ fillColor: undefined, strokeColor: undefined });
            psmb.removeActiveItemsByValue(this.zIndex);
        } else {
            this.setOptions({ fillColor: selected, strokeColor: selected });
            psmb.setChoiceByValue(this.zIndex);
        }
    }
}
//define map
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
    tableInfo[dato.id] = {
        name: dato.name,
        comuna: dato.comuna,
        provincia: dato.provincia,
        code: dato.code
    }
    marker.addListener('click', showInfo);
    return marker;
}
//define chart
var chart: any = am4core.create("chartdiv", am4charts.XYChart);
var dateAxis = chart.xAxes.push(new am4charts.DateAxis());
dateAxis.dataFields.category = 'date';
var valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
//"beforedatavalidated beforedisposed beforevalidated blur childadded childremoved dataitemsvalidated datarangechanged datavalidated disabled doublehit down drag dragged dragstart dragstop enabled endchange focus globalscalechanged hidden hit hold inited input keydown keypress keyup layoutvalidated maxsizechanged out over parentset positionchanged propertychanged rangechangeended rangechangestarted ready resize rightclick shown sizechanged startchanged startedchanged swipe swipeleft swiperight toggled track transformed transitionended up validated visibilitychanged wheel wheeldown wheelleft wheelright wheelup zIndexChanged"
//log all events
//chart.events.on('appeared', (e: any) => { console.log(e); }); chart.events.on('beforedatavalidated', (e: any) => { console.log(e) }); chart.events.on('beforedisposed', (e: any) => { console.log(e) }); chart.events.on('beforevalidated', (e: any) => { console.log(e) }); chart.events.on('blur', (e: any) => { console.log(e) }); chart.events.on('childadded', (e: any) => { console.log(e) }); chart.events.on('childremoved', (e: any) => { console.log(e) }); chart.events.on('dataitemsvalidated', (e: any) => { console.log(e) }); chart.events.on('datarangechanged', (e: any) => { console.log(e) }); chart.events.on('datavalidated', (e: any) => { console.log(e) }); chart.events.on('disabled', (e: any) => { console.log(e) }); chart.events.on('doublehit', (e: any) => { console.log(e) }); chart.events.on('down', (e: any) => { console.log(e) }); chart.events.on('drag', (e: any) => { console.log(e) }); chart.events.on('dragged', (e: any) => { console.log(e) }); chart.events.on('dragstart', (e: any) => { console.log(e) }); chart.events.on('dragstop', (e: any) => { console.log(e) }); chart.events.on('enabled', (e: any) => { console.log(e) }); chart.events.on('endchange', (e: any) => { console.log(e) }); chart.events.on('focus', (e: any) => { console.log(e) }); chart.events.on('globalscalechanged', (e: any) => { console.log(e) }); chart.events.on('hidden', (e: any) => { console.log(e) }); chart.events.on('hit', (e: any) => { console.log(e) }); chart.events.on('hold', (e: any) => { console.log(e) }); chart.events.on('inited', (e: any) => { console.log(e) }); chart.events.on('input', (e: any) => { console.log(e) }); chart.events.on('keydown', (e: any) => { console.log(e) }); chart.events.on('keypress', (e: any) => { console.log(e) }); chart.events.on('keyup', (e: any) => { console.log(e) }); chart.events.on('layoutvalidated', (e: any) => { console.log(e) }); chart.events.on('maxsizechanged', (e: any) => { console.log(e) }); chart.events.on('out', (e: any) => { console.log(e) }); chart.events.on('over', (e: any) => { console.log(e) }); chart.events.on('parentset', (e: any) => { console.log(e) }); chart.events.on('positionchanged', (e: any) => { console.log(e) }); chart.events.on('propertychanged', (e: any) => { console.log(e) }); chart.events.on('rangechangeended', (e: any) => { console.log(e) }); chart.events.on('rangechangestarted', (e: any) => { console.log(e) }); chart.events.on('ready', (e: any) => { console.log(e) }); chart.events.on('resize', (e: any) => { console.log(e) }); chart.events.on('rightclick', (e: any) => { console.log(e) }); chart.events.on('shown', (e: any) => { console.log(e) }); chart.events.on('sizechanged', (e: any) => { console.log(e) }); chart.events.on('startchanged', (e: any) => { console.log(e) }); chart.events.on('startedchanged', (e: any) => { console.log(e) }); chart.events.on('swipe', (e: any) => { console.log(e) }); chart.events.on('swipeleft', (e: any) => { console.log(e) }); chart.events.on('swiperight', (e: any) => { console.log(e) }); chart.events.on('toggled', (e: any) => { console.log(e) }); chart.events.on('track', (e: any) => { console.log(e) }); chart.events.on('transformed', (e: any) => { console.log(e) }); chart.events.on('transitionended', (e: any) => { console.log(e) }); chart.events.on('up', (e: any) => { console.log(e) }); chart.events.on('validated', (e: any) => { console.log(e) }); chart.events.on('visibilitychanged', (e: any) => { console.log(e) }); chart.events.on('wheel', (e: any) => { console.log(e) }); chart.events.on('wheeldown', (e: any) => { console.log(e) }); chart.events.on('wheelleft', (e: any) => { console.log(e) }); chart.events.on('wheelright', (e: any) => { console.log(e) }); chart.events.on('wheelup', (e: any) => { console.log(e) }); chart.events.on('zIndexChanged', (e: any) => { console.log(e); });

var chartloaded = false;
var loadChart = function (_: any) {
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
}
chart.events.on('validated', loadChart);
//define load dates function
var loadDates = function() {
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
//fetch each series from server and loaded into graph
var fetchData = async function (url: string, tag: string, name: string) {
    if (!(tag in chart.exporting.dataFields)) {
        var parts = tag.split('_');
        var psmbid = parts.splice(1, 1)[0];
        var vartag = parts.join('_')
        chart.exporting.dataFields[tag] = name;
        return await fetch(url)
            .then(data => data.json())
            .then(j => {
                var counter = 0;
                chart.data.forEach((c: any) => {
                    if (counter < j.length && c.date === j[counter].date) {
                        c[tag] = j[counter].value;
                        counter++;
                        if (counter == j.length) {
                            if (stats[vartag] == null) stats[vartag] = {};
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
            }).catch((e: any) => {
                delete chart.exporting.dataFields[tag];
            });
    }
}
var generatefetchData = async function(v: any, p: any, sd: any, ed: any) {
    var tag = `${v.value}_${p.value}`;
    var name = `${v.label} ${p.label}`;
    var url = `/ambiental/data?area=${p.value}&type=${v.value.charAt(0)}&id=${v.value.substring(1)}&start=${sd}&end=${ed}`;
    return await fetchData(url, tag, name);
}
var green = [0, 128, 0];
var red = [255, 0, 0];
//interface to fetch collections to server and load them into graph
//between 5000 and 30000
var circlemax = 20000;
var circlemin = 5000;
var loadData = async function (e: any, isPsmb: boolean) {
    var arr = isPsmb ? variables.getValue().concat(customvars.getValue()) : psmb.getValue();
    if (arr.length === 0) return;
    loaderStart();
    var sd = $('#start').val(), ed = $('#end').val();
    var promises = isPsmb ?
        arr.map((v: any) => generatefetchData(v, e.detail, sd, ed)) :
        arr.map((p: any) => generatefetchData(e.detail, p, sd, ed));
    return Promise.all(promises).then(_ => {
        chart.invalidateData();
    });
}
var redrawCircles = function () {
    removeCircles();
    var variable = variables.getValue(true).slice(-1)[0];
    var selectedpsmbs = psmb.getValue(true);
    if (variable != undefined && selectedpsmbs.length && Object.keys(stats).length) {
        var psmbStats = stats[variable];
        var min: any;
        var max: any;
        var selectpsmbStats: any = {};
        if (psmbStats != undefined) {
            selectedpsmbs.forEach((p: any) => {
                min = (min === undefined || psmbStats[p] < min) ? psmbStats[p] : min;
                max = (max === undefined || psmbStats[p] > max) ? psmbStats[p] : max;
                selectpsmbStats[p] = psmbStats[p];
            });
            Object.entries(selectpsmbStats).forEach(([k, v]: any) => {
                var marker = markers.find((m: any) => m.zIndex == k);
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
}
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
var passive = supportsPassiveOption ? { passive: true } : false;
//remove circle
var removeCircle = function(i: string) {
    var circle = circles[i];
    // remove event listers
    google.maps.event.clearListeners(circle, 'click_handler_name');
    google.maps.event.clearListeners(circle, 'drag_handler_name');
    google.maps.event.clearListeners(circle, 'mouseover');
    google.maps.event.clearListeners(circle, 'mouseout');
    circle.setRadius(0);
    // if polygon:
    // polygon_shape.setPath([]); 
    circle.setMap(null);
    delete circles[i];
}
var removeCircles = () => {
    Object.keys(circles).forEach((k: any) => removeCircle(k));
}
var removeSeries = function (tag:string) {
    chart.series.values.forEach((v: any, i: number) => {
        if (v.dataFields.valueY === tag) {
            delete chart.exporting.dataFields[tag];
            chart.series.removeIndex(i).dispose();
        }
    });
}
//custom variable choice listeners
customvar.addEventListener('addItem', async (e: any) => {
    var arr = psmb.getValue();
    if (arr.length === 0) return;
    loaderStart();
    var sd = $('#start').val(), ed = $('#end').val();
    var promises = arr.map(async (p: any) => {
        var tag = `${e.detail.value}_${p.value}`;
        var name = `${e.detail.label} ${p.label}`;
        var url = `/ambiental/customdata?area=${p.value}&typeid=${e.detail.value}&start=${sd}&end=${ed}`;
        return await fetchData(url, tag, name);
    });
    Promise.all(promises).then(_ => {
        chart.invalidateData();
        redrawCircles();
    });
}, passive);
customvar.addEventListener('removeItem', (event: any) => {
    psmb.getValue(true).forEach((e: any) => {
        var tag = `${event.detail.value}_${e}`;
        removeSeries(tag);
    });
    redrawCircles();
}, passive);
//variable choice listeners
evariable.addEventListener('addItem', async (e: any) => {
    await loadData(e, false);
    redrawCircles();
}, passive);
evariable.addEventListener('removeItem', (event: any) => {
    psmb.getValue(true).forEach((e: any) => {
        var tag = `${event.detail.value}_${e}`;
        removeSeries(tag);
    });
    redrawCircles();
}, passive);
//trigger map click on selection
var clickMap = function(e: any) {
    if (e !== undefined && polygons[e.detail.value] !== undefined)
        google.maps.event.trigger(polygons[e.detail.value], 'click', {});
}
//psmb choice listeners
epsmb.addEventListener('addItem', async (e: any) => {
    await loadData(e, true);
    clickMap(e);
    redrawCircles();
}, passive);
epsmb.addEventListener('removeItem', (event: any) => {
    variables.getValue(true).forEach((e: any) => {
        var tag = `${e}_${event.detail.value}`;
        removeSeries(tag);
    });
    clickMap(event);
    redrawCircles();
}, passive);
//get lists of choices
var getList = async function(name: string) {
    return await fetch(`/api/static/json/${lang}/${name}list.json`)
        .then(r => r.json())
        .catch(e => console.error(e, name));
}
//wait until
var loaderStopped = function() {
    return new Promise((resolve: any, _) => {
        (function wait() {
            if ((<HTMLElement>document.getElementById('preloader-background')).style.display === "none") return resolve();
            setTimeout(wait, 400);
        })();
    });
}
var chartLoaded = function() {
    return new Promise((resolve: any, _) => {
        (function wait() {
            if (chartloaded) return resolve();
            setTimeout(wait, 400);
        })();
    });
}
//init function
var init = async function () {
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
        var callDatas = function (a: any, sd: any, ed: any, psmbs: any, sps: any, tls: any, groupId: number) {
            var nogroup = groupId === 0;
            var promises = nogroup ?
                psmbs.map((psmb: any) =>
                    sps.map((sp: any) => {
                        var tag = [a.value, psmb.value, sp.value].join('_');
                        if (!chart.series._values.some((v: any) => tag === v.dataFields.valueY)) {
                            var name = [a.label, psmb.label, sp.label.replace("<i>", "[bold font-style: italic]").replace("</i>", "[/]")].join(' ');
                            var url = `/ambiental/tldata?a=${a.value}&psmb=${psmb.value}&sp=${sp.value}&start=${sd}&end=${ed}`;
                            return fetchData(url, tag, name);
                        }
                    })) :
                tls.filter((x: any) => Math.floor(x.value / 10) === groupId).map((x: any) =>
                    psmbs.map((psmb: any) =>
                        sps.map((sp: any) => {
                            var tag = [a.value, psmb.value, sp.value, x.value].join('_');
                            if (!chart.series._values.some((v: any) => tag === v.dataFields.valueY)) {
                                var name = [a.label, psmb.label, sp.label.replace("<i>", "[bold font-style: italic]").replace("</i>", "[/]"), x.label].join(' ');
                                var url = `/ambiental/tldata?a=${a.value}&psmb=${psmb.value}&sp=${sp.value}&v=${x.value}&start=${sd}&end=${ed}`;
                                return fetchData(url, tag, name);
                            }
                        })));
            return Promise.all(promises).then(r => r);
        }
        etl.addEventListener('addItem', (_e: any) => {
            var tls = tl.getValue();
            var analyses = tls.filter((x: any) => Math.floor(x.value / 10) === 1);
            if (analyses.length !== 0) {
                var psmbs = tls.filter((x: any) => Math.floor(x.value / 10) === 2);
                var sps = tls.filter((x: any) => Math.floor(x.value / 10) === 3);
                if (psmbs.length !== 0 && sps.length !== 0) {
                    loaderStart();
                    var sd = $('#start').val();
                    var ed = $('#end').val();
                    analyses.forEach((a: any) => {
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
        etl.addEventListener('removeItem', (event: any) => {
            if (chart.series.values.length === 0) return;
            var tags = chart.series.values.map((v: any) => v.dataFields.valueY);
            var id = event.detail.value;
            var removed = 0;
            tags.forEach((k: any, i:number) => {
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
    } else {
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
}
//init and load marker clusters after
init();
//DATES
$('.input-daterange').datepicker({
    inputs: Array.from($('.actual_range')),
    format: 'yyyy-mm-dd',
    language: lang,
    startDate: $('#start').val().toString(),
    endDate: $('#end').val().toString()
}).on('changeDate', (_: any) => {
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
var CreateTableFromJSON = function(json: any) {
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
            if (json[i][col[j]]) tabCell.innerHTML = json[i][col[j]];
        }
    }
    // FINALLY ADD THE NEWLY CREATED TABLE WITH JSON DATA TO A CONTAINER.
    var divContainer = document.getElementById("results");
    divContainer.appendChild(table);
}
var fetchPlankton = function() {
    var sd = $('#start').val();
    var ed = $('#end').val();
    var promises = psmb.getValue(true).map((p: any) => fetch('/ambiental/BuscarInformes?' + `id=${p}&start=${sd}&end=${ed}`).then(r => r.json()));
    Promise.all(promises).then(j => {
        var divContainer = document.getElementById("results");
        divContainer.innerHTML = "";
        j.forEach(i => CreateTableFromJSON(i));
    });
    $("#table2excel").show();
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
$('#fetchPlankton').click(fetchPlankton);
$('#table2excel').click(function () {
    tableToExcel('newTable', 'Ensayos');
});
//overflow scroll legend if to many elements added
document.getElementById('legenddiv').addEventListener('DOMSubtreeModified', function (_e) {
    document.getElementById("legenddiv").style.height = chart.legend.contentHeight + "px";
});
document.getElementById("pin-switch").addEventListener('change', async function () {
    if ((<HTMLInputElement>this).checked) {
        (await clusters).addMarkers(markers);
    } else {
        (await clusters).clearMarkers();
        Object.values(markers).forEach((m: any) => {
            m.setMap(null);
        });
    }
});
document.getElementById("polygon-switch").addEventListener('change', async function () {
    if ((<HTMLInputElement>this).checked) {
        Object.values(polygons).forEach((m: any) => {
            m.setMap(map);
        });
    } else {
        Object.values(polygons).forEach((m: any) => {
            m.setMap(null);
        });
    }
});
document.getElementById("stat-switch").addEventListener('change', async function () {
    if ((<HTMLInputElement>this).checked) {
        Object.values(circles).forEach((m: any) => {
            m.setMap(map);
        });
    } else {
        Object.values(circles).forEach((m: any) => {
            m.setMap(null);
        });
    }
});