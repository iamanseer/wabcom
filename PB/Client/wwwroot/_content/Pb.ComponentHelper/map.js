var map, marker, circle;
var mapLat = 0, mapLong = 0;
var radius = 0;


function Mapinit(lat = "", lon = "",DotNet) {
    if (lat == "" || lon == null) {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition((position) => {
                mapLat = position.coords.latitude;
                mapLong = position.coords.longitude;
                loadMap();
                $("#setLocation").click();
                DotNet.invokeMethodAsync("SendLocation", position.coords.latitude, position.coords.longitude);
            })
        }
        else {
            loadMap();
        }
    }
    else {
        mapLat = parseFloat(lat);
        mapLong = parseFloat(lon);
        loadMap();
    }
}


function loadMap() {

    map = new google.maps.Map(document.getElementById("map"), {
        zoom: 15,
        center: {
            lat: mapLat,
            lng: mapLong,
        },
    });

    // Add a marker at the center of the map.
    addMarker();

    google.maps.event.addListener(marker, 'dragend', function (evt) {
        mapLat = evt.latLng.lat();
        mapLong = evt.latLng.lng();
        loadMap();
        $("#setLocation").click();
    });
}


function UpdateLocation(wrapper) {
    return wrapper.invokeMethod("UpdateLocation", `${mapLat}`, `${mapLong}`);
}



function addMarker() {
    marker = new google.maps.Marker({
        position: {
            lat: mapLat,
            lng: mapLong,
        },
        map: map,
        draggable: true
    });

    if (radius != 0) {
        circle = null;
        circle = new google.maps.Circle({
            strokeColor: "#FF0000",
            strokeOpacity: 0.8,
            strokeWeight: 2,
            fillColor: "#FF0000",
            fillOpacity: 0.35,
            map: map,
            center: {
                lat: mapLat,
                lng: mapLong,
            },
            radius: parseFloat(radius),
        });
    }
}

function setRadius(rad) {
    radius = rad;
    loadMap();
};