    var cloudmade;
    var map;
    var infowindow;
    var pinSound= new Audio("/images/mapR/mapr.mp3");    

    var recent; 
    var i;

    
    function loadMap(){
    var lat = 51.514;
    var lng = -0.137;
    
      
    if (google.loader.ClientLocation) {
       lat = google.loader.ClientLocation.latitude;
       lng = google.loader.ClientLocation.longitude;
    } 

    var myOptions = {
          zoom: 4,
          center: new google.maps.LatLng(lat, lng),
          mapTypeId: google.maps.MapTypeId.ROADMAP
        };
        map = new google.maps.Map(document.getElementById('map_canvas'),
            myOptions);  
  }
  
 
  function renderPin(act, showBubble){
    
    if(act.Text != null){
    var location = new google.maps.LatLng(act.Lat, act.Long);
    
    var marker = new google.maps.Marker({
      map:map,
      icon: "/images/mapr/" + act.Type + ".png",
      draggable:false,
      title: act.Text,
      animation: google.maps.Animation.DROP,
      position: location
    });
    
    var html = "<div class='makr'><span class='t'>" + prettyDate(act.TimeStamp) + "</span><img width='32' height='32' src='" + act.Icon + "'>" + 
        "<div><small>" + act.MemberName + ": </small><h3><a target='_blank' href='" + act.Url + "'>" + act.Text + "</a></h3>";
    
    html += "</div></div>";
    
    var v = new InfoBubble({
          content: html,
          shadowStyle: 0,
          padding: 10,
          backgroundColor: '#ffffff',
          borderRadius: 4,
          arrowSize: 10,
          borderWidth: 1,
          borderColor: '#666',
          hideCloseButton: true,
          arrowPosition: 30,
          backgroundClassName: 'phoney',
          arrowStyle: 0
        });  
    
    google.maps.event.addListener(marker, 'click', function() {
      if(infowindow) infowindow.close();
      
      infowindow = v;
      infowindow.open(map,marker);
    });
    
      
    if(showBubble){    
      if(infowindow) infowindow.close();
      
      infowindow = v;
      infowindow.open(map,marker);
      
      if(!jQuery("#noSound").is(':checked'))
          pinSound.play();
    }
    
      
    /*  
    var v = new google.maps.InfoWindow({
        content: html
    }); 
      
    google.maps.event.addListener(marker, 'click', function() {
      if(infowindow) infowindow.close();
      
      infowindow = v;
      infowindow.open(map,marker);
    });
    
    
    if(showBubble){    
      if(infowindow) infowindow.close();
      
      infowindow = v;
      infowindow.open(map,marker);
    }*/
      
      
  }
}
    
    
function delayedRender(){
    var p = recent.pop();
    
    if(p != null)
      renderPin(p, true);
    else
      window.clearInterval(i); 
    }  

function prettyDate(time){
  
  var date = new Date(parseInt(time.replace(/(^.*\()|([+-].*$)/g, ''))),
  diff = (((new Date()).getTime() - date.getTime()) / 1000),
  day_diff = Math.floor(diff / 86400);
  
  /*var date = new Date((time || "").replace(/-/g,"/").replace(/[TZ]/g," ")),
    diff = (((new Date()).getTime() - date.getTime()) / 1000),
    day_diff = Math.floor(diff / 86400);
    */  
  if ( isNaN(day_diff) || day_diff < 0 || day_diff >= 31 )
    return;
      
  return day_diff == 0 && (
      diff < 60 && "Now" ||
      diff < 120 && "1 min" ||
      diff < 3600 && Math.floor( diff / 60 ) + " mins" ||
      diff < 7200 && "1 hour" ||
      diff < 86400 && Math.floor( diff / 3600 ) + " hours") ||
    day_diff == 1 && "Yesterday" ||
    day_diff < 7 && day_diff + " days" ||
    day_diff < 31 && Math.ceil( day_diff / 7 ) + " weeks";
}

$(function () {

    //the hub
    var actsHub = $.connection.activities;
    var actsUL = $("#acts");
      
    function init() {
        return actsHub.getRecentActivities().done(function (acts) {
             recent = acts;
             i = self.setInterval("delayedRender()",10000);   
            });
    }
    
  
    function renderActivity(act, showBubble) {
        renderPin(act, showBubble);
    }

    // Add client-side hub methods that the server will call
    actsHub.newActivity = function (activity) {
        renderActivity(activity, true);
    };


    actsHub.feedOpened = function () {
        init();
    };

    actsHub.feedClosed = function () {
    };

    actsHub.feedReset = function () {
        init();
    };

    // Start the connection
    $.connection.hub.start(function () {
        
        init().done(function () {
            actsHub.getFeedState()
                .done(function (state) {
                    if (state === 'Open') {
                        actsHub.feedOpened();
                    } else {
                        actsHub.feedClosed();
                    }
                });
        });
    });
});