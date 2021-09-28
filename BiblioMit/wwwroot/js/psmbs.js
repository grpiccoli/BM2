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
    choiceOps.maxItemText = (maxItemCount) => `Máximo ${maxItemCount} valores`;
}
var epsmb = document.getElementById('psmb');
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
var markerCluster;
var data = document.getElementById("map-holder").dataset;
var bnds = new google.maps.LatLngBounds();
var showInfo = function (_e) {
    var id = this.zIndex;
    var area = polygons[id] != null ? Area(polygons[id].getPath().getArray()) : "";
    var head = `<h4>${tableInfo[id].code} ${tableInfo[id].name}</h4>
<table><tr><th>${data.code}</th><td align=""right"">${tableInfo[id].code}</td></tr>
<tr><th>${data.company}</th><td align=""right"">${tableInfo[id].businessName}</td></tr>
<tr><th>RUT</th><td align=""right"">${tableInfo[id].rut}</td></tr>
<tr><th>${data.locality}</th><td align=""right"">${tableInfo[id].name}</td></tr>
<tr><th>${data.commune}</th><td align=""right"">${tableInfo[id].comuna}</td></tr>
<tr><th>${data.province}</th><td align=""right"">${tableInfo[id].provincia}</td></tr>
<tr><th>${data.region}</th><td align=""right"">${tableInfo[id].region}</td></tr></tr>
<tr><th>${data.area}</th><td align=""right"">`;
    var tail = `</td></tr><tr><a href=""/Centres/Details/${id}"">${data.details}</a></tr>
<tr><th>${data.sources}</th><td></td></tr><tr><td>Sernapesca</td>
<td align=""right""><a target=""_blank"" href=""https://www.sernapesca.cl""><img src=""../images/ico/sernapesca.svg"" height=""20"" /></a></td></tr>
<tr><td>PER Mitilidos</td>
<td align=""right""><a target=""_blank"" href=""https://www.mejillondechile.cl""><img src=""../images/ico/mejillondechile.min.png"" height=""20"" /></a></td></tr>
<tr><td>Subpesca</td>
<td align=""right""><a target=""_blank"" href=""https://www.subpesca.cl""><img src=""../images/ico/subpesca.png"" height=""20"" /></a></td></tr>`;
    infowindow.setContent(head + area + tail);
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
        title: `${dato.name} ${dato.id}`,
        zIndex: dato.id
    });
    tableInfo[dato.id] = {
        name: dato.name,
        comuna: dato.comuna,
        provincia: dato.provincia,
        code: dato.code,
        rut: dato.rut,
        businessName: dato.businessName
    };
    if (!(dato.rut in companies))
        companies[dato.rut] = [];
    companies[dato.rut].push(dato.id);
    if (!(dato.comunaId in psmbs)) {
        psmbs[dato.comunaId] = [];
    }
    psmbs[dato.comunaId].push(dato.id);
    var provincia = Math.floor(dato.comunaId / 100);
    if (!(provincia in psmbs)) {
        psmbs[provincia] = [];
    }
    psmbs[provincia].push(dato.id);
    var region = Math.floor(provincia / 10);
    if (!(region in psmbs)) {
        psmbs[region] = [];
    }
    psmbs[region].push(dato.id);
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
    var selCompanies = company.getValue(false);
    var selPsmb = psmb.getValue(false);
    var anyCompany = selCompanies.length > 0;
    var anyPsmb = selPsmb.length > 0;
    if (anyCompany || anyPsmb) {
        markerCluster.clearMarkers();
        Object.values(markers).forEach((m) => {
            m.setMap(null);
        });
        var toShow = [];
        if (anyCompany)
            selCompanies.forEach((c) => {
                if (companies[c.value])
                    companies[c.value].forEach((s) => {
                        markers[s].setMap(map);
                        toShow.push(markers[s]);
                    });
            });
        if (anyPsmb)
            selPsmb.forEach((c) => {
                if (psmbs[c.value])
                    psmbs[c.value].forEach((s) => {
                        markers[s].setMap(map);
                        toShow.push(markers[s]);
                    });
            });
        console.log(toShow);
        markerCluster.addMarkers(toShow);
    }
    else {
        Object.values(markers).forEach((m) => {
            m.setMap(map);
        });
        markerCluster.addMarkers(Object.values(markers));
    }
};
epsmb.addEventListener('addItem', (_) => filter(), passive);
epsmb.addEventListener('removeItem', (_) => filter(), passive);
ecompany.addEventListener('addItem', (_) => filter(), passive);
ecompany.addEventListener('removeItem', (_) => filter(), passive);
var clickMap = function (e) {
    if (e !== undefined && polygons[e.detail.value] !== undefined)
        google.maps.event.trigger(polygons[e.detail.value], 'click', {});
};
var getList = function (name) {
    return __awaiter(this, void 0, void 0, function* () {
        return yield fetch(`/api/static/json/${lang}/${name}list.json`)
            .then(r => r.json())
            .catch(e => console.error(e, name));
    });
};
var init = function () {
    return __awaiter(this, void 0, void 0, function* () {
        var name = isresearch ? 'research' : 'farm';
        var name2 = isresearch ? 'institution' : 'company';
        var mappromise = fetch(`/api/static/json/${lang}/${name}data.json`)
            .then(r => r.json())
            .then(data => data.map(processMapData))
            .then(m => {
            map.fitBounds(bnds);
            markerCluster = new MarkerClusterer(map, m, { imagePath: '/images/markers/m' });
        });
        var comunalist = getList('comuna' + name);
        var provincialist = getList('provincia' + name);
        var regionlist = getList('region');
        var psmblist = getList(name);
        var companylist = getList(name2);
        var psmbchoices = Promise.all([comunalist, provincialist, regionlist]).then(r => {
            psmb.setChoices(r[0].concat(r[1]).concat([r[2]]));
            return true;
        });
        var companychoices = Promise.all([psmblist, companylist]).then(r => {
            company.setChoices(r[0].concat(r[1]));
            return true;
        });
        Promise.all([psmbchoices, companychoices, mappromise]).then(_ => {
            loaderStop();
        });
    });
};
init();
//# sourceMappingURL=psmbs.js.map