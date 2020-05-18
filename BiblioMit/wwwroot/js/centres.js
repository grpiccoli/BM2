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
var polygons = {};
var table = [];
var map = new google.maps.Map(document.getElementById('map'), {
    mapTypeId: 'terrain'
});
var Area = function (path) {
    return (google.maps.geometry.spherical.computeArea(path) / 10000).toFixed(2);
};
var showInfo = function (_e) {
    var id = this.zIndex;
    var head = "<h4>" + table[id].code + " " + table[id].name + "</h4><table><tr><th>C\u00F3digo</th><td align=\"\"right\"\">" + table[id].code + "</td></tr><tr><th>Compa\u00F1\u00EDa</th><td align=\"\"right\"\">" + table[id].businessName + "</td></tr><tr><th>RUT</th><td align=\"\"right\"\">" + table[id].rut + "</td></tr><tr><th>Localidad</th><td align=\"\"right\"\">" + table[id].name + "</td></tr><tr><th>Comuna</th><td align=\"\"right\"\">" + table[id].comuna + "</td></tr><tr><th>Provincia</th><td align=\"\"right\"\">" + table[id].provincia + "</td></tr><tr><th>Regi\u00F3n</th><td align=\"\"right\"\">" + table[id].region + "</td></tr></tr><tr><th>\u00C1rea (Ha)</th><td align=\"\"right\"\">";
    var tail = "</td></tr><tr><a href=\"\"/Centres/Details/" + id + "\"\">Detalles</a></tr><tr><th>Fuentes</th><td></td></tr><tr><td>Sernapesca</td><td align=\"\"right\"\"><a target=\"\"_blank\"\" href=\"\"https://www.sernapesca.cl\"\"><img src=\"\"../images/ico/sernapesca.svg\"\" height=\"\"20\"\" /></a></td></tr><tr><td>PER Mitilidos</td><td align=\"\"right\"\"><a target=\"\"_blank\"\" href=\"\"https://www.mejillondechile.cl\"\"><img src=\"\"../images/ico/mejillondechile.min.png\"\" height=\"\"20\"\" /></a></td></tr><tr><td>Subpesca</td><td align=\"\"right\"\"><a target=\"\"_blank\"\" href=\"\"https://www.subpesca.cl\"\"><img src=\"\"../images/ico/subpesca.png\"\" height=\"\"20\"\" /></a></td></tr>";
    infowindow.setContent(head + Area(polygons[id].getPath().getArray()) + tail);
    infowindow.open(map, this);
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
var infowindow = new google.maps.InfoWindow({
    maxWidth: 500
});
window.onload = function initMap() {
    return __awaiter(this, void 0, void 0, function () {
        var bnds, type;
        return __generator(this, function (_a) {
            switch (_a.label) {
                case 0:
                    bnds = new google.maps.LatLngBounds();
                    type = document.getElementById('type').value;
                    return [4, fetch("/centres/getmap?type=" + type)
                            .then(function (r) { return r.json(); })
                            .then(function (data) { return data.map(function (dato) {
                            var consessionPolygon = new google.maps.Polygon({
                                paths: dato.position,
                                strokeColor: '#FF0000',
                                strokeOpacity: 0.8,
                                strokeWeight: 2,
                                fillColor: '#FF0000',
                                fillOpacity: 0.35
                            });
                            consessionPolygon.setMap(map);
                            polygons[dato.id] = consessionPolygon;
                            var center = getBounds(dato.position).getCenter();
                            bnds.extend(center);
                            var marker = new google.maps.Marker({
                                position: center,
                                title: dato.code + " " + dato.name
                            });
                            table[dato.id] = {
                                name: dato.name,
                                comuna: dato.comuna,
                                provincia: dato.provincia,
                                code: dato.code,
                                rut: dato.rut,
                                bsnssName: dato.bsnssName
                            };
                            marker.addListener('click', showInfo);
                            return marker;
                        }); }).then(function (m) { return new MarkerClusterer(map, m, { imagePath: '/images/markers/m' }); }).then(function (_) {
                            map.fitBounds(bnds);
                            map.setCenter(bnds.getCenter());
                        })];
                case 1:
                    _a.sent();
                    return [2];
            }
        });
    });
};
//# sourceMappingURL=centres.js.map