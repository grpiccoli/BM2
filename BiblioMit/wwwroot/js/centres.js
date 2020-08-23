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
var type = document.getElementById('type').value;
var isresearch = type === '5';
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
var epsmb = document.getElementById('area');
var ecompany = document.getElementById('company');
choiceOps.placeholderValue = esp ? 'Seleccione comunas' : 'Select communes';
var psmb = new Choices(epsmb, choiceOps);
choiceOps.placeholderValue = esp ? 'Seleccione compañías/instituciones' : 'Select companies/institutions';
var company = new Choices(ecompany, choiceOps);
var map = new google.maps.Map(document.getElementById('map'), {
    mapTypeId: 'terrain'
});
var infowindow = new google.maps.InfoWindow({
    maxWidth: 500
});
var tableInfo = {};
var polygons = {};
var markers = {};
var companies = {};
var psmbs = {};
var bnds = new google.maps.LatLngBounds();
var showInfo = function (_e) {
    var id = this.zIndex;
    var head = "<h4>" + tableInfo[id].code + " " + tableInfo[id].name + "</h4>\n<table><tr><th>C\u00F3digo</th><td align=\"\"right\"\">" + tableInfo[id].code + "</td></tr>\n<tr><th>Compa\u00F1\u00EDa</th><td align=\"\"right\"\">" + tableInfo[id].businessName + "</td></tr>\n<tr><th>RUT</th><td align=\"\"right\"\">" + tableInfo[id].rut + "</td></tr>\n<tr><th>Localidad</th><td align=\"\"right\"\">" + tableInfo[id].name + "</td></tr>\n<tr><th>Comuna</th><td align=\"\"right\"\">" + tableInfo[id].comuna + "</td></tr>\n<tr><th>Provincia</th><td align=\"\"right\"\">" + tableInfo[id].provincia + "</td></tr>\n<tr><th>Regi\u00F3n</th><td align=\"\"right\"\">" + tableInfo[id].region + "</td></tr></tr>\n<tr><th>\u00C1rea (Ha)</th><td align=\"\"right\"\">";
    var tail = "</td></tr><tr><a href=\"\"/Centres/Details/" + id + "\"\">Detalles</a></tr>\n<tr><th>Fuentes</th><td></td></tr><tr><td>Sernapesca</td>\n<td align=\"\"right\"\"><a target=\"\"_blank\"\" href=\"\"https://www.sernapesca.cl\"\"><img src=\"\"../images/ico/sernapesca.svg\"\" height=\"\"20\"\" /></a></td></tr>\n<tr><td>PER Mitilidos</td>\n<td align=\"\"right\"\"><a target=\"\"_blank\"\" href=\"\"https://www.mejillondechile.cl\"\"><img src=\"\"../images/ico/mejillondechile.min.png\"\" height=\"\"20\"\" /></a></td></tr>\n<tr><td>Subpesca</td>\n<td align=\"\"right\"\"><a target=\"\"_blank\"\" href=\"\"https://www.subpesca.cl\"\"><img src=\"\"../images/ico/subpesca.png\"\" height=\"\"20\"\" /></a></td></tr>";
    infowindow.setContent(head + Area(polygons[id].getPath().getArray()) + tail);
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
    if (!isresearch) {
        var consessionPolygon = new google.maps.Polygon({
            paths: dato.position,
            zIndex: dato.id,
            strokeColor: '#FF0000',
            strokeOpacity: 0.8,
            strokeWeight: 2,
            fillColor: '#FF0000',
            fillOpacity: 0.35
        });
        consessionPolygon.setMap(map);
        consessionPolygon.addListener('click', addListenerOnPolygon);
        polygons[dato.id] = consessionPolygon;
    }
    var center = getBounds(dato.position).getCenter();
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
        code: dato.code,
        rut: dato.rut,
        bsnssName: dato.bsnssName
    };
    if (!(dato.rut in companies))
        companies[dato.rut] = {};
    companies[dato.rut].push(dato.id);
    if (!(dato.comunaid in psmbs))
        psmbs[dato.comunaid] = {};
    if (!(dato.provinciaid in psmbs))
        psmbs[dato.provinciaid] = {};
    if (!(dato.regionid in psmbs))
        psmbs[dato.regionid] = {};
    companies[dato.rut].push(dato.id);
    marker.addListener('click', showInfo);
    markers[dato.id] = marker;
    return marker;
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
var filter = function () {
    markers.forEach(function (m) {
        m.setMap(null);
    });
    company.getValue(false).forEach(function (c) {
        companies[c].forEach(function (s) {
            markers[s].setMap(map);
        });
    });
    psmb.getValue(false).forEach(function (c) {
        psmbs[c].forEach(function (s) {
            markers[s].setMap(map);
        });
    });
};
epsmb.addEventListener('addItem', function (_) { return filter(); }, passive);
epsmb.addEventListener('removeItem', function (_) { return filter(); }, passive);
ecompany.addEventListener('addItem', function (_) { return filter(); }, passive);
ecompany.addEventListener('removeItem', function (_) { return filter(); }, passive);
var clickMap = function (e) {
    if (e !== undefined && polygons[e.detail.value] !== undefined)
        google.maps.event.trigger(polygons[e.detail.value], 'click', {});
};
var getList = function (name) {
    return __awaiter(this, void 0, void 0, function () {
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0: return [4, fetch("/ambiental/" + name + "list")
                        .then(function (r) { return r.json(); })
                        .catch(function (e) { return console.error(e, name); })];
                case 1: return [2, _a.sent()];
            }
        });
    });
};
var init = function () {
    return __awaiter(this, void 0, void 0, function () {
        var name, name2, mappromise, comunalist, provincialist, regionlist, psmblist, companylist, psmbchoices, companychoices;
        return __generator(this, function (_a) {
            name = isresearch ? 'research' : 'farm';
            name2 = isresearch ? 'institution' : 'company';
            mappromise = fetch("/ambiental/" + name + "data")
                .then(function (r) { return r.json(); })
                .then(function (data) { return data.map(processMapData); })
                .then(function (m) {
                map.fitBounds(bnds);
                new MarkerClusterer(map, m, { imagePath: '/images/markers/m' });
            });
            comunalist = getList('comuna' + name);
            provincialist = getList('provincia' + name);
            regionlist = getList('region');
            psmblist = getList(name);
            companylist = getList(name2);
            psmbchoices = Promise.all([comunalist, provincialist, regionlist]).then(function (r) {
                psmb.setChoices(r[0].concat(r[1]).concat([r[2]]));
                return true;
            });
            companychoices = Promise.all([psmblist, companylist]).then(function (r) {
                company.setChoices(r[0].concat(r[1]));
                return true;
            });
            Promise.all([psmbchoices, companychoices, mappromise]).then(function (_) {
                loaderStop();
            });
            return [2];
        });
    });
};
init();
//# sourceMappingURL=centres.js.map