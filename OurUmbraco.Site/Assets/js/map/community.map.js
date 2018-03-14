function initMap() {

    var map = new google.maps.Map(document.getElementById('map'),
        {
            zoom: 5,
            center: { lat: 51.4472452, lng: 2.8123371 }
        });

    // Try HTML5 geolocation.
    if (navigator.geolocation) {
        navigator.geolocation.getCurrentPosition(function (position) {            
            var pos = {
                lat: position.coords.latitude,
                lng: position.coords.longitude
            };

            //Use the lat & lon of the users current location
            //To set the center of the map and zoom in closer
            map.setCenter(pos);
            map.setZoom(8);

        }, function () {
            //If we ever need todo handle error handling - or user denined permission
        });
    }

    google.maps.event.addListener(map, 'idle', () => {
        const sw = map.getBounds().getSouthWest();
        const ne = map.getBounds().getNorthEast();
        
        var url = "/umbraco/api/mapapi/GetAllMemberLocations?swLat=" + sw.lat() + "&swLon=" + sw.lng() + "&neLat=" + ne.lat() + "&neLon=" + ne.lng();

        //Map Marker mustache HTML template
        var template = $('#map-marker-template').html();

        $.getJSON(url, function (data) {

            var markers = data.map(function (item) {
                var latlng = new google.maps.LatLng(item.Lat, item.Lon);
                var marker = new google.maps.Marker({
                    position: latlng
                });

                var infowindow = new google.maps.InfoWindow({
                    content: "<span>Loading</span>"
                });

                //Only render the HTML content - when you click the marker
                var html = Mustache.render(template, item);

                marker.addListener('click', function () {
                    infowindow.setContent(html);
                    infowindow.open(map, marker);
                });

                return marker;
            });

            // Add a marker clusterer to manage the markers.
            var markerCluster = new MarkerClusterer(map,
                markers,
                {
                    imagePath: 'https://developers.google.com/maps/documentation/javascript/examples/markerclusterer/m'
                });

            var zoomLevel = map.getZoom();
            if (zoomLevel > 15) {

                var innerTemplate = $('#member-item-template').html();
                var outerTemplate = $('#members-template').html();

                var multiHtml = Mustache.render(outerTemplate,
                    data,
                    {
                        member: innerTemplate
                    });

                $("#member-list").html(multiHtml);
            } else {
                //Ensure the member list is cleared out - when we zoom out we need to clear out our results
                $("#member-list").html("");
            }

        });

    });
};