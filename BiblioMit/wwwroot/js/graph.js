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
var _this = this;
var series = new Object();
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
    maxItemText: function (maxItemCount) {
        return "M\u00E1ximo " + maxItemCount + " valores";
    },
    fuseOptions: {
        include: 'score'
    }
};
var etl = document.getElementById('tl');
choiceOps.placeholderValue = 'Variables semáforo';
var tl = new Choices(etl, choiceOps);
var semaforo = !document.getElementById('semaforo').classList.contains('d-none');
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
var chart = am4core.create("chartdiv", am4charts.XYChart);
am4core.ready(function () {
    return __awaiter(this, void 0, void 0, function () {
        var dateAxis, valueAxis;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    am4core.useTheme(am4themes_kelly);
                    chart.language.locale = am4lang_es_ES;
                    dateAxis = chart.xAxes.push(new am4charts.DateAxis());
                    dateAxis.dataFields.category = 'date';
                    dateAxis.dateFormats.setKey('year', 'yy');
                    dateAxis.periodChangeDateFormats.setKey('month', 'MMM yy');
                    dateAxis.tooltipDateFormat = 'dd MMM, yyyy';
                    dateAxis.renderer.minGridDistance = 40;
                    valueAxis = chart.yAxes.push(new am4charts.ValueAxis());
                    chart.legend = new am4charts.Legend();
                    chart.cursor = new am4charts.XYCursor();
                    chart.exporting.menu = new am4core.ExportMenu();
                    return [4, fetch('/ambiental/exportmenu').then(function (j) { return j.json(); })
                            .then(function (menu) { return chart.exporting.menu.items = menu; })];
                case 1:
                    _a.sent();
                    return [2];
            }
        });
    });
});
function fetchData(url, tag, name) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4, fetch(url)
                        .then(function (data) { return data.json(); })
                        .then(function (j) {
                        var counter = 0;
                        chart.data.map(function (c) {
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
                    })];
                case 1: return [2, _a.sent()];
            }
        });
    });
}
function loadData(values, event, boolpsmb) {
    return __awaiter(this, void 0, void 0, function () {
        var sd, ed, x, st, rd, grp, nd, promises;
        return __generator(this, function (_a) {
            psmb.disable();
            variables.disable();
            if (semaforo)
                tl.disable();
            sd = $('#start').val(), ed = $('#end').val();
            x = [], st = 0, rd = 0, grp = true;
            if (boolpsmb) {
                x = [null, event.value, null, event.label, null];
                rd = st + 4;
            }
            else {
                x = [event.value, null, event.label, null, event.groupValue === "Fitoplancton" ? "fito" : "graph"];
                st++;
                grp = false;
            }
            nd = st + 2;
            promises = values.map(function (e) {
                x[st] = e.value;
                x[nd] = e.label;
                if (grp)
                    x[rd] = e.groupId === 2 ? "fito" : "graph";
                var tag = x[0] + "_" + x[1];
                var name = x[2] + " " + x[3];
                var url = "/ambiental/" + x[4] + "data?area=" + x[1] + "&var=" + x[0] + "&start=" + sd + "&end=" + ed;
                return fetchData(url, tag, name);
            });
            Promise.all(promises).then(function (_) {
                psmb.enable();
                variables.enable();
                if (semaforo)
                    tl.enable();
            });
            return [2];
        });
    });
}
function clickMap(e) {
    if (e !== undefined && polygons[e.detail.value] !== undefined)
        google.maps.event.trigger(polygons[e.detail.value], 'click', {});
}
var epsmb = document.getElementById('psmb');
epsmb.addEventListener('addItem', function (event) {
    loadData(variables.getValue(), event.detail, true);
    clickMap(event);
}, supportsPassiveOption ? { passive: true } : false);
epsmb.addEventListener('removeItem', function (event) {
    variables.getValue(true).forEach(function (e) {
        var name = e + "_" + event.detail.value;
        chart.series.removeIndex(chart.series.indexOf(series[name])).dispose();
    });
    clickMap(event);
}, supportsPassiveOption ? { passive: true } : false);
function getList(name) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4, fetch("/ambiental/" + name + "list")
                        .then(function (r) { return r.json(); })
                        .catch(function (e) { return console.error(e); })];
                case 1: return [2, _a.sent()];
            }
        });
    });
}
var psmb = new Choices(epsmb, choiceOps);
psmb.setChoices(function () { return __awaiter(_this, void 0, void 0, function () { return __generator(this, function (_a) {
    switch (_a.label) {
        case 0: return [4, getList('psmb')];
        case 1: return [2, _a.sent()];
    }
}); }); });
var evariable = document.getElementById('variable');
evariable.addEventListener('addItem', function (event) {
    return loadData(psmb.getValue(), event.detail, false);
}, supportsPassiveOption ? { passive: true } : false);
evariable.addEventListener('removeItem', function (event) {
    psmb.getValue(true).forEach(function (e) {
        var name = event.detail.value + "_" + e;
        chart.series.removeIndex(chart.series.indexOf(series[name])).dispose();
    });
}, supportsPassiveOption ? { passive: true } : false);
choiceOps.placeholderValue = 'Seleccione variables';
var variables = new Choices(evariable, choiceOps);
variables.setChoices(function () { return __awaiter(_this, void 0, void 0, function () { return __generator(this, function (_a) {
    switch (_a.label) {
        case 0: return [4, getList('variable')];
        case 1: return [2, _a.sent()];
    }
}); }); });
function getParam(a, x) {
    switch (a) {
        case '11':
            return "&t=" + x.value.slice(1, 2);
        case '12':
            return "&l=" + x.value;
        case '13':
        case '16':
            return "&s=" + x.value;
        case '15':
            return "&rs=" + x.value;
        default:
            return '';
    }
    ;
}
if (semaforo) {
    etl.addEventListener('addItem', function (event) {
        if (event.detail.groupValue === 'Análisis') {
            var a = event.detail.value;
            var tls = tl.getValue();
            var psmbs = tls.filter(function (x) { return x.groupId === 2; });
            var sps = tls.filter(function (x) { return x.groupId === 3; });
            if (psmbs.length !== 0 && sps.length !== 0) {
                var id = event.detail.id;
                var nm = event.detail.groupValue;
                console.log(event.detail);
                var xs = id == 0 ?
                    { value: '', name: '' }
                    : tls.filter(function (x) { return x.groupId === id; });
                var xs = tls.filter(function (x) { return x.groupId === id; });
                if (xs.length !== 0) {
                    psmb.disable();
                    variables.disable();
                    tl.disable();
                    var sd = $('#start').val();
                    var ed = $('#end').val();
                    var promises = psmbs.map(function (psmb) {
                        return sps.map(function (sp) {
                            return xs.map(function (x) {
                                var tag = [a, psmb.value, sp.value, x.value].join('_');
                                var name = [event.detail.name, psmb.name, sp.name, x.name].join(' ');
                                var url = "/ambiental/tldata?a=" + a + "&psmb=" + psmb.value + "&sp=" + sp.value + getParam(a, x) + "&start=" + sd + "&end=" + ed;
                                return fetchData(url, tag, name);
                            });
                        });
                    });
                    Promise.all(promises).then(function (r) {
                        chart.invalidateData();
                        psmb.enable();
                        variables.enable();
                        tl.enable();
                    });
                }
                else {
                    alert(nm + " no seleccionada, seleccione requerimientos y luego an\u00E1lisis");
                }
            }
            else {
                tl.removeActiveItemsByValue(a);
                alert('PSMB y especie no seleccionados, seleccione requerimientos y luego análisis');
            }
        }
    }, supportsPassiveOption ? { passive: true } : false);
    etl.addEventListener('removeItem', function (event) {
        if (event.detail.groupValue === 'Análisis') {
            var a = event.detail.value;
            var tls = tl.getValue();
            var psmbs = tls.filter(function (x) { return x.groupId === 2; });
            var sps = tls.filter(function (x) { return x.groupId === 3; });
            if (psmbs.length !== 0 && sps.length !== 0) {
                psmb.disable();
                variables.disable();
                tl.disable();
                var id = a - 7;
                var xs = tls.filter(function (x) { return x.groupId === id; });
                if (xs.length !== 0) {
                    psmbs.forEach(function (psmb) {
                        return sps.forEach(function (sp) {
                            return xs.forEach(function (x) {
                                var name = [a, psmb.value, sp.value, x.value].join('_');
                                chart.series.removeIndex(chart.series.indexOf(series[name])).dispose();
                            });
                        });
                    });
                }
            }
        }
    }, supportsPassiveOption ? { passive: true } : false);
    tl.setChoices(function () { return __awaiter(_this, void 0, void 0, function () { return __generator(this, function (_a) {
        switch (_a.label) {
            case 0: return [4, getList('tl')];
            case 1: return [2, _a.sent()];
        }
    }); }); });
}
function Area(path) {
    return (google.maps.geometry.spherical.computeArea(path) / 10000).toFixed(2);
}
var selected = 'red';
function addListenerOnPolygon(e) {
    if ($.isEmptyObject(e)) {
        psmb.getValue(true).includes(this.zIndex.toString()) ?
            this.setOptions({ fillColor: selected, strokeColor: selected }) :
            this.setOptions({ fillColor: undefined, strokeColor: undefined });
    }
    else {
        if (psmb.getValue(true).includes(this.zIndex.toString())) {
            this.setOptions({ fillColor: undefined, strokeColor: undefined });
            psmb.removeActiveItemsByValue(this.zIndex.toString());
        }
        else {
            this.setOptions({ fillColor: selected, strokeColor: selected });
            psmb.setChoiceByValue(this.zIndex.toString());
        }
    }
}
;
function flatten(items) {
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
}
function getBounds(positions) {
    var bounds = new google.maps.LatLngBounds();
    flatten(positions).forEach(function (p) { return bounds.extend(p); });
    return bounds;
}
var map = new google.maps.Map(document.getElementById('map'), {
    mapTypeId: 'terrain'
});
var infowindow = new google.maps.InfoWindow({
    maxWidth: 150
});
var polygons = {};
window.onload = function initMap() {
    return __awaiter(this, void 0, void 0, function () {
        var bnds;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    bnds = new google.maps.LatLngBounds();
                    return [4, fetch('/ambiental/mapdata')
                            .then(function (r) { return r.json(); })
                            .then(function (data) {
                            return data.map(function (dato) {
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
                                    title: dato.name + " " + dato.id
                                });
                                marker.addListener('click', (function (marker) {
                                    return function () {
                                        var content = "<h4>" + dato.name + "</h4><table><tr><th>C\u00F3digo</th><td align=\"right\">" + dato.id + "</td></tr>";
                                        if (dato.comuna !== null)
                                            content +=
                                                "<tr><th>Comuna</th><td align=\"right\">" + dato.comuna + "</td></tr>";
                                        if (dato.provincia !== null)
                                            content +=
                                                "<tr><th>Provincia</th><td align=\"right\">" + dato.provincia + "</td></tr>";
                                        content +=
                                            "<tr><th>Regi\u00F3n</th><td align=\"right\">" + dato.region + "</td>\n</tr><tr><th>\u00C1rea (ha)</th>\n<td align=\"right\">" + Area(consessionPolygon.getPath().getArray()) + "</td>\n</tr>\n<tr><th>Fuentes</th><td></td></tr>\n<tr><td>Sernapesca</td>\n<td align=\"right\">\n<a target=\"_blank\" href=\"https://www.sernapesca.cl\">\n<img src=\"../images/ico/sernapesca.svg\" height=\"30\" /></a></td></tr>\n<tr><td>PER Mitilidos</td>\n<td align=\"right\">\n<a target=\"_blank\" href=\"https://www.mejillondechile.cl\">\n<img src=\"../images/ico/mejillondechile.min.png\" height=\"30\" /></a></td></tr>\n<tr><td>Subpesca</td>\n<td align=\"right\">\n<a target=\"_blank\" href=\"https://www.subpesca.cl\">\n<img src=\"../images/ico/subpesca.png\" height=\"30\" /></a></td></tr>";
                                        infowindow.setContent(content);
                                        infowindow.open(map, marker);
                                    };
                                })(marker));
                                return marker;
                            });
                        }).then(function (m) {
                            return new MarkerClusterer(map, m, { imagePath: '/images/markers/m' });
                        }).then(function (_) {
                            map.fitBounds(bnds);
                            map.setCenter(bnds.getCenter());
                            loadDates();
                            variables.setChoiceByValue('t');
                            psmb.setChoiceByValue('1');
                        })];
                case 1:
                    _a.sent();
                    return [2];
            }
        });
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
$('.input-daterange').datepicker({
    inputs: $('.actual_range'),
    format: 'yyyy-mm-dd',
    language: 'es',
    startDate: $('#start').val().toString(),
    endDate: $('#end').val().toString()
}).on('changeDate', function (_) {
    var vars = variables.getValue(true);
    variables.removeActiveItems();
    chart.data = [];
    loadDates();
    vars.forEach(function (v) { return variables.setChoiceByValue(v); });
});
//# sourceMappingURL=graph.js.map