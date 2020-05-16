var polygons: any = {};
window.onload = async function initMap() {
    function Area(path: google.maps.LatLng[] | google.maps.MVCArray<google.maps.LatLng>) {
        return (google.maps.geometry.spherical.computeArea(path) / 10000).toFixed(2);
    }
    function flatten(items: any[]) {
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
    function getBounds(positions: any[]) {
        var bounds = new google.maps.LatLngBounds();
        flatten(positions).forEach(p => bounds.extend(p));
        return bounds;
    }
    var infowindow = new google.maps.InfoWindow({
        maxWidth: 500
    });
    var map = new google.maps.Map(document.getElementById('map'), {
        mapTypeId: 'terrain'
    });
    var bnds: google.maps.LatLngBounds = new google.maps.LatLngBounds();
    var type = document.getElementById('type');
    await fetch(`/centres/getmap?type=${type}`)
        .then(r => r.json())
        .then(data => data.map((dato: any) => {
            var head = `<h4>${dato.id} ${dato.name}</h4><table><tr><th>Código</th><td align=""right"">${dato.id}</td></tr><tr><th>Compañía</th><td align=""right"">${dato.businessName}</td></tr><tr><th>RUT</th><td align=""right"">${dato.rut}</td></tr><tr><th>Localidad</th><td align=""right"">${dato.name}</td></tr><tr><th>Comuna</th><td align=""right"">${dato.comuna}</td></tr><tr><th>Provincia</th><td align=""right"">${dato.provincia}</td></tr><tr><th>Región</th><td align=""right"">${dato.region}</td></tr></tr><tr><th>Área (Ha)</th><td align=""right"">`;
            var tail = `</td></tr><tr><a href=""/Centres/Details/${dato.id}"">Detalles</a></tr><tr><th>Fuentes</th><td></td></tr><tr><td>Sernapesca</td><td align=""right""><a target=""_blank"" href=""https://www.sernapesca.cl""><img src=""../images/ico/sernapesca.svg"" height=""20"" /></a></td></tr><tr><td>PER Mitilidos</td><td align=""right""><a target=""_blank"" href=""https://www.mejillondechile.cl""><img src=""../images/ico/mejillondechile.min.png"" height=""20"" /></a></td></tr><tr><td>Subpesca</td><td align=""right""><a target=""_blank"" href=""https://www.subpesca.cl""><img src=""../images/ico/subpesca.png"" height=""20"" /></a></td></tr>`;
            var id = dato.id;
            var consessionPolygon = new google.maps.Polygon({
                paths: dato.position,
                strokeColor: '#FF0000',
                strokeOpacity: 0.8,
                strokeWeight: 2,
                fillColor: '#FF0000',
                fillOpacity: 0.35
            });
            consessionPolygon.setMap(map);
            polygons[id] = consessionPolygon;
            var center = getBounds(dato.position).getCenter();
            bnds.extend(center);
            var marker = new google.maps.Marker({
                position: center,
                title: `Código de Concesión : ${id}`
                //label: String(array[i][1])
            });
            marker.addListener('click', (marker =>
                () => {
                    infowindow.setContent(head + Area(consessionPolygon.getPath().getArray()) + tail);
                    infowindow.open(map, marker);
                    map.setCenter(marker.getPosition());
                })(marker));
            return marker;
        })).then(m => new MarkerClusterer(map, m, { imagePath: '/images/markers/m' })
    ).then(_ => {
        map.fitBounds(bnds);
        map.setCenter(bnds.getCenter());
    });
}