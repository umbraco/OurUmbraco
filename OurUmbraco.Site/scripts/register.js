// Largely copied from the example at: https://developers.google.com/maps/documentation/javascript/examples/places-searchbox
function initAutocomplete() {
    var markers = [];
    var lat = document.getElementById("Latitude").value;
    var lon = document.getElementById("Longitude").value;

    if (lat === "") { lat = 0; }
    if (lon === "") { lon = 0; }

    var zoom = 1;
    if (lat !== 0 && lon !== 0) { zoom = 10; }

    var latlng = new google.maps.LatLng(lat, lon);
    var myOptions = {
        zoom: zoom,
        center: latlng,
        zoomControl: false,
        mapTypeControl: false,
        streetViewControl: false,
        scaleControl: false
    };

    var map = new google.maps.Map(document.getElementById('map'), myOptions);

    var input = document.getElementById('pac-input');

    if (lat !== 0 && lon !== 0) {
        var location = document.getElementById("Location").value;
        markers.push(new google.maps.Marker({
            map: map,
            position: latlng,
            title: document.getElementById("Location").value
        }));

        input.value = location;
    }

    // Create the search box and link it to the UI element.
    var searchBox = new google.maps.places.SearchBox(input);
    map.controls[google.maps.ControlPosition.TOP_LEFT].push(input);

    // Don't submit the form on pressing enter
    input.onkeypress = function (e) {
        var key = e.charCode || e.keyCode || 0;
        if (key === 13) {
            e.preventDefault();
        }
    };

    $('#pac-input').css('display', 'block');

    // Bias the SearchBox results towards current map's viewport.
    map.addListener('bounds_changed',
        function () {
            searchBox.setBounds(map.getBounds());
        });

    // Listen for the event fired when the user selects a prediction and retrieve
    // more details for that place.
    searchBox.addListener('places_changed',
        function () {
            var places = searchBox.getPlaces();

            if (places.length === 0) {
                return;
            }

            // Clear out the old markers.
            markers.forEach(function (marker) {
                marker.setMap(null);
            });
            markers = [];

            // For each place, get the icon, name and location.
            var bounds = new google.maps.LatLngBounds();
            places.forEach(function (place) {
                if (!place.geometry) {
                    console.log("Returned place contains no geometry");
                    return;
                }
                var icon = {
                    url: place.icon,
                    size: new google.maps.Size(71, 71),
                    origin: new google.maps.Point(0, 0),
                    anchor: new google.maps.Point(17, 34),
                    scaledSize: new google.maps.Size(25, 25)
                };

                // Create a marker for each place.
                markers.push(new google.maps.Marker({
                    map: map,
                    icon: icon,
                    title: place.name,
                    position: place.geometry.location
                }));

                document.getElementById("Location").value = place.formatted_address;
                document.getElementById("Latitude").value = place.geometry.location.lat();
                document.getElementById("Longitude").value = place.geometry.location.lng();

                if (place.geometry.viewport) {
                    // Only geocodes have viewport.
                    bounds.union(place.geometry.viewport);
                } else {
                    bounds.extend(place.geometry.location);
                }
            });
            map.fitBounds(bounds);
        });
}