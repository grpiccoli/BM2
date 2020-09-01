var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
var loaderStart = function () {
    document.getElementById('preloader-background').style.display = "block";
};
var loaderStop = function () {
    document.getElementById('preloader-background').style.display = "none";
};
loaderStart();
var Area = function (path) {
    return (google.maps.geometry.spherical.computeArea(path) / 10000).toFixed(2);
};
var flatten = function (items) {
    var flat = [];
    items.forEach(function (item) {
        if (Array.isArray(item)) {
            flat.push.apply(flat, flatten(item));
        }
        else {
            flat.push(item);
        }
    });
    return flat;
};
var getBounds = function (positions) {
    var bounds = new google.maps.LatLngBounds();
    flatten(positions).forEach(function (p) { return bounds.extend(p); });
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
    choiceOps.maxItemText = function (maxItemCount) { return "M\u00E1ximo " + maxItemCount + " valores"; };
}
var semaforo = !document.getElementById('semaforo').classList.contains('d-none');
var epsmb = document.getElementById('psmb');
var evariable = document.getElementById('variable');
var etl = document.getElementById('tl');
choiceOps.placeholderValue = esp ? 'Seleccione áreas' : 'Select areas';
var psmb = new Choices(epsmb, choiceOps);
choiceOps.placeholderValue = esp ? 'Seleccione variables' : 'Select Variables';
var variables = new Choices(evariable, choiceOps);
choiceOps.placeholderValue = esp ? 'Variables semáforo' : 'Semaforo Variables';
var tl = new Choices(etl, choiceOps);
var map = new google.maps.Map(document.getElementById('map'), {
    mapTypeId: 'terrain'
});
var infowindow = new google.maps.InfoWindow({
    maxWidth: 500
});
var tableInfo = [];
var polygons = {};
var bnds = new google.maps.LatLngBounds();
var markers = [];
var titles = esp ?
    ["Código", "Comuna", "Provincia", "Región", "Área", "Fuentes"] :
    ["Code", "Commune", "Province", "Region", "Area", "Sources"];
var showInfo = function (_e) {
    var id = this.zIndex;
    var content = "<h4>" + tableInfo[id].name + "</h4><table class=\"table\"><tr><th scope=\"row\">" + titles[0] + "</th><td align=\"right\">" + tableInfo[id].code + "</td></tr>";
    if (tableInfo[id].comuna !== null)
        content +=
            "<tr><th scope=\"row\">" + titles[1] + "</th><td align=\"right\">" + tableInfo[id].comuna + "</td></tr>";
    if (tableInfo[id].provincia !== null)
        content +=
            "<tr><th scope=\"row\">" + titles[2] + "</th><td align=\"right\">" + tableInfo[id].provincia + "</td></tr>";
    content +=
        "<tr><th scope=\"row\">" + titles[3] + "</th><td align=\"right\">Los Lagos</td>\n</tr><tr><th scope=\"row\">" + titles[4] + " (ha)</th>\n<td align=\"right\">" + Area(polygons[id].getPath().getArray()) + "</td>\n</tr>\n<tr><th scope=\"row\">" + titles[5] + "</th><td></td></tr>\n<tr><td>Sernapesca</td>\n<td align=\"right\">\n<a target=\"_blank\" href=\"https://www.sernapesca.cl\" rel=\"noopener\">\n<img src=\"../images/ico/sernapesca.svg\" height=\"20\" /></a></td></tr>\n<tr><td>PER Mit\u00EDlidos</td>\n<td align=\"right\">\n<a target=\"_blank\" href=\"https://www.mejillondechile.cl\" rel=\"noopener\">\n<img src=\"../images/ico/mejillondechile.min.png\" height=\"20\" /></a></td></tr>\n<tr><td>Subpesca</td>\n<td align=\"right\">\n<a target=\"_blank\" href=\"https://www.subpesca.cl\" rel=\"noopener\">\n<img src=\"../images/ico/subpesca.png\" height=\"20\" /></a></td></tr>";
    infowindow.setContent(content);
    infowindow.open(map, this);
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
        title: dato.name + " " + dato.id,
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
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    if (!!(tag in chart.exporting.dataFields)) return [3, 2];
                    chart.exporting.dataFields[tag] = name;
                    return [4, fetch(url)
                            .then(function (data) { return data.json(); })
                            .then(function (j) {
                            var counter = 0;
                            chart.data.map(function (c) {
                                if (counter < j.length && c.date === j[counter].date) {
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
                        }).catch(function (e) {
                            delete chart.exporting.dataFields[tag];
                        })];
                case 1: return [2, _a.sent()];
                case 2: return [2];
            }
        });
    });
};
var generatefetchData = function (v, p, sd, ed) {
    return __awaiter(this, void 0, void 0, function () {
        var tag, name, url;
        return __generator(this, function (_a) {
            tag = v.value + "_" + p.value;
            name = v.label + " " + p.label;
            url = "/ambiental/data?area=" + p.value + "&type=" + v.value.charAt(0) + "&id=" + v.value.substring(1) + "&start=" + sd + "&end=" + ed;
            return [2, fetchData(url, tag, name)];
        });
    });
};
var loadData = function (e, isPsmb) {
    return __awaiter(this, void 0, void 0, function () {
        var arr, sd, ed, promises;
        return __generator(this, function (_a) {
            arr = isPsmb ? variables.getValue() : psmb.getValue();
            if (arr.length === 0)
                return [2];
            loaderStart();
            sd = $('#start').val(), ed = $('#end').val();
            promises = isPsmb ?
                arr.map(function (v) { return generatefetchData(v, e.detail, sd, ed); }) :
                arr.map(function (p) { return generatefetchData(e.detail, p, sd, ed); });
            Promise.all(promises).then(function (_) {
                chart.invalidateData();
            });
            return [2];
        });
    });
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
evariable.addEventListener('addItem', function (e) { return loadData(e, false); }, passive);
evariable.addEventListener('removeItem', function (event) {
    psmb.getValue(true).forEach(function (e) {
        var tag = event.detail.value + "_" + e;
        chart.series.values.forEach(function (v, i) {
            if (v.dataFields.valueY === tag) {
                delete chart.exporting.dataFields[tag];
                chart.series.removeIndex(i).dispose();
            }
        });
    });
}, passive);
var clickMap = function (e) {
    if (e !== undefined && polygons[e.detail.value] !== undefined)
        google.maps.event.trigger(polygons[e.detail.value], 'click', {});
};
epsmb.addEventListener('addItem', function (e) {
    loadData(e, true);
    clickMap(e);
}, passive);
epsmb.addEventListener('removeItem', function (event) {
    variables.getValue(true).forEach(function (e) {
        var tag = e + "_" + event.detail.value;
        chart.series.values.forEach(function (v, i) {
            if (v.dataFields.valueY === tag) {
                delete chart.exporting.dataFields[tag];
                chart.series.removeIndex(i).dispose();
            }
        });
    });
    clickMap(event);
}, passive);
var getList = function (name) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4, fetch("/json/" + name + "list.json")
                        .then(function (r) { return r.json(); })
                        .catch(function (e) { return console.error(e, name); })];
                case 1: return [2, _a.sent()];
            }
        });
    });
};
var loaderStopped = function () {
    return new Promise(function (resolve, _) {
        (function wait() {
            if (document.getElementById('preloader-background').style.display === "none")
                return resolve();
            setTimeout(wait, 400);
        })();
    });
};
var chartLoaded = function () {
    return new Promise(function (resolve, _) {
        (function wait() {
            if (chartloaded)
                return resolve();
            setTimeout(wait, 400);
        })();
    });
};
var init = function () {
    return __awaiter(this, void 0, void 0, function () {
        var cuencadata, oceanvarlist, cuencalist, comunadata, comunalist, groupvarlist, variablechoicesInit, psmbchoicesInit, buildchart, psmbdata, psmblist, genusvarlist, speciesvarlist, tllist, psmbchoices, variablechoices, tlchoices, clusters, callDatas, psmbchoices, variablechoices, clusters;
        return __generator(this, function (_a) {
            loadDates();
            cuencadata = fetch('/json/cuencadata.json')
                .then(function (r) { return r.json(); })
                .then(function (data) { return data.map(processMapData); })
                .then(function (m) {
                markers = m;
                map.fitBounds(bnds);
                map.setCenter(bnds.getCenter());
            });
            oceanvarlist = getList('oceanvar');
            cuencalist = getList('cuenca');
            comunadata = fetch('/json/comunadata.json')
                .then(function (r) { return r.json(); })
                .then(function (data) { return data.map(processMapData); })
                .then(function (m) { return markers = markers.concat(m); });
            comunalist = getList('comuna');
            groupvarlist = getList('groupvar');
            variablechoicesInit = Promise.all([oceanvarlist]).then(function (r) { variables.setChoices([r[0]]); return true; });
            psmbchoicesInit = Promise.all([cuencalist]).then(function (r) { psmb.setChoices([r[0]]); return true; });
            buildchart = Promise.all([variablechoicesInit, psmbchoicesInit, cuencadata]).then(function (r) {
                variables.setChoiceByValue('v0');
                psmb.setChoiceByValue(1);
                return chartLoaded();
            });
            if (semaforo) {
                psmbdata = fetch('/json/psmbdata.json')
                    .then(function (r) { return r.json(); })
                    .then(function (data) { return data.map(processMapData); })
                    .then(function (m) { return markers = markers.concat(m); });
                psmblist = getList('psmb');
                genusvarlist = getList('genusvar');
                speciesvarlist = getList('speciesvar');
                tllist = getList('tl');
                psmbchoices = Promise.all([psmbchoicesInit, comunalist, psmblist]).then(function (r) {
                    psmb.setChoices(r[1].concat(r[2]));
                    return true;
                });
                variablechoices = Promise.all([variablechoicesInit, groupvarlist, genusvarlist, speciesvarlist]).then(function (r) {
                    variables.setChoices([r[1], r[2], r[3]]);
                    return true;
                });
                tlchoices = Promise.all([tllist]).then(function (r) { tl.setChoices(r[0]); return true; });
                clusters = Promise.all([cuencadata, comunadata, psmbdata]).then(function (_) { return new MarkerClusterer(map, markers, { imagePath: '/images/markers/m' }); });
                callDatas = function (a, sd, ed, psmbs, sps, tls, groupId) {
                    var nogroup = groupId === 0;
                    var promises = nogroup ?
                        psmbs.map(function (psmb) {
                            return sps.map(function (sp) {
                                var tag = [a.value, psmb.value, sp.value].join('_');
                                if (!chart.series._values.some(function (v) { return tag === v.dataFields.valueY; })) {
                                    var name = [a.label, psmb.label, sp.label.replace("<i>", "[bold font-style: italic]").replace("</i>", "[/]")].join(' ');
                                    var url = "/ambiental/tldata?a=" + a.value + "&psmb=" + psmb.value + "&sp=" + sp.value + "&start=" + sd + "&end=" + ed;
                                    return fetchData(url, tag, name);
                                }
                            });
                        }) :
                        tls.filter(function (x) { return Math.floor(x.value / 10) === groupId; }).map(function (x) {
                            return psmbs.map(function (psmb) {
                                return sps.map(function (sp) {
                                    var tag = [a.value, psmb.value, sp.value, x.value].join('_');
                                    if (!chart.series._values.some(function (v) { return tag === v.dataFields.valueY; })) {
                                        var name = [a.label, psmb.label, sp.label.replace("<i>", "[bold font-style: italic]").replace("</i>", "[/]"), x.label].join(' ');
                                        var url = "/ambiental/tldata?a=" + a.value + "&psmb=" + psmb.value + "&sp=" + sp.value + "&v=" + x.value + "&start=" + sd + "&end=" + ed;
                                        return fetchData(url, tag, name);
                                    }
                                });
                            });
                        });
                    return Promise.all(promises).then(function (r) { return r; });
                };
                etl.addEventListener('addItem', function (_e) {
                    var tls = tl.getValue();
                    var analyses = tls.filter(function (x) { return Math.floor(x.value / 10) === 1; });
                    if (analyses.length !== 0) {
                        var psmbs = tls.filter(function (x) { return Math.floor(x.value / 10) === 2; });
                        var sps = tls.filter(function (x) { return Math.floor(x.value / 10) === 3; });
                        if (psmbs.length !== 0 && sps.length !== 0) {
                            loaderStart();
                            var sd = $('#start').val();
                            var ed = $('#end').val();
                            analyses.forEach(function (a) {
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
                etl.addEventListener('removeItem', function (event) {
                    if (chart.series.values.length === 0)
                        return;
                    var tags = chart.series.values.map(function (v) { return v.dataFields.valueY; });
                    var id = event.detail.value;
                    var removed = 0;
                    tags.forEach(function (k, i) {
                        if (k.match(/^[1-7][0-8]_[1-7][0-8]_[1-7][0-8](_[1-7][0-8])?$/g) && k.includes(id)) {
                            delete chart.exporting.dataFields[k];
                            chart.series.removeIndex(i - removed).dispose();
                            removed++;
                        }
                    });
                }, passive);
                Promise.all([buildchart, psmbchoices, variablechoices, tlchoices, clusters]).then(function (_) {
                    chart.events.on('validated', loaderStop);
                    loaderStop();
                });
            }
            else {
                psmbchoices = Promise.all([psmbchoicesInit, comunalist]).then(function (r) {
                    psmb.setChoices(r[1]);
                    return true;
                });
                variablechoices = Promise.all([variablechoicesInit, groupvarlist]).then(function (r) {
                    variables.setChoices([r[1]]);
                    return true;
                });
                clusters = Promise.all([cuencadata, comunadata]).then(function (_) { return new MarkerClusterer(map, markers, { imagePath: '/images/markers/m' }); });
                Promise.all([buildchart, psmbchoices, variablechoices, clusters]).then(function (_) {
                    chart.events.on('validated', loaderStop);
                    loaderStop();
                });
            }
            return [2];
        });
    });
};
init();
$('.input-daterange').datepicker({
    inputs: $('.actual_range'),
    format: 'yyyy-mm-dd',
    language: lang,
    startDate: $('#start').val().toString(),
    endDate: $('#end').val().toString()
}).on('changeDate', function (_) {
    var vars = variables.getValue(true);
    variables.removeActiveItems();
    if (semaforo) {
        var tls = tl.getValue(true);
        tl.removeActiveItems();
    }
    chart.data = [];
    loadDates();
    vars.forEach(function (v) { return variables.setChoiceByValue(v); });
    if (semaforo) {
        tls.forEach(function (v) { return tl.setChoiceByValue(v); });
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
            tabCell.innerHTML = json[i][col[j]];
        }
    }
    var divContainer = document.getElementById("results");
    divContainer.appendChild(table);
};
var fetchPlankton = function () {
    var sd = $('#start').val();
    var ed = $('#end').val();
    var promises = psmb.getValue(true).map(function (p) { return fetch('/ambiental/BuscarInformes', {
        method: 'POST',
        headers: new Headers({ 'Content-Type': 'application/x-www-form-urlencoded;charset=UTF-8' }),
        body: "id=" + p + "&start=" + sd + "&end=" + ed
    }).then(function (r) { return r.json(); }); });
    Promise.all(promises).then(function (j) {
        var divContainer = document.getElementById("results");
        divContainer.innerHTML = "";
        j.forEach(function (i) { return CreateTableFromJSON(i); });
    });
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
document.getElementById('legenddiv').addEventListener('DOMSubtreeModified', function (_e) {
    document.getElementById("legenddiv").style.height = chart.legend.contentHeight + "px";
});
//# sourceMappingURL=graph.js.map