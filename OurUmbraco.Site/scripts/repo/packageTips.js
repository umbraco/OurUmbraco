jQuery(document).ready(function() {
  
  jQuery("#loginToSeeFavs").click(function(event){
      event.preventDefault();
    
      jQuery("#repoForm").toggle();
  });
  
  jQuery("a.btn").live('click', function(event) {
    
    event.preventDefault();
    
    var _name = jQuery(this).attr("title");
    var _guid = jQuery(this).attr("rel");
    var url = 'http://' + _callback + '&guid=' + _guid;
    
    if(confirm("Are you sure you want to install: '" + _name + "'?")){
      document.location.href = url;   
    }
    
    
    /*
        if(confirm('Are you sure you wish to download:\n\n' + _name + '\n\n'){
           var url = 'http://' + _callback + '&guid=' + _guid;
           alert("going to: " + url);
    
           document.location.href = url;
        }
      */

    
  });
  
  
jQuery("li .deliPackage").live('mouseenter', function(event) { 
  
  $(this).qtip(
          {
            content: {
               text: function(api) {
                   var href = $(this).find(".hiLite a").attr("href");
                   console.log($(this).find(".hiLite a").attr);
                   var rel = $(this).find(".hiLite a").attr("rel");
                   var title = $(this).find(".hiLite a").attr("title");
                 
                   var html = "<div class='customTip'>";
                       html += $(this).find(".brief").html();    
                       html += $(this).find(".hiLite").html();
                       html += "<div class='popularity'>" + $(this).find(".popularity").html() + "</div>"; 
                       html += "<a href='" + href + "'>view details</a>";     
                       html += "</div>";
                       return html;
                   } 
              },
            position: {
               my: "bottom center", 
               at: "top center",
               viewport: $(window),
               adjust: {
                  method: "flip"
               } 
              },
             show: {
              event: event.type, // Use the same show event as the one that triggered the event handler
              ready: true,
              solo: true
             },
            hide: {
              event: false,
              inactive: 3000
             },
            style: {
                classes: 'ui-tooltip-light ui-tooltip-shadow',
                tip: {
                   corner: true,
                   width: 40,
                  height: 20
                     }
             }
          }, event);
});

});