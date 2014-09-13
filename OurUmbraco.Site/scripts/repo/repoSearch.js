
          
      $(document).ready(function() {
        
        $("#search-field").focus(function() {
          if ($("#search-field").val() == "Search...") {
            $("#search-field").val("");
          }
        });

        $("#search-field").blur(function() {
          if ($("#search-field").val() == "") {
            $("#search-field").val("Search...");
          }
        });       
        
      });
  $(function() {
    $("#search-field").typeWatch({ highlight: true, wait: 200, captureLength: -1, callback: function() {
      var query = $("#search-field").val();
      if (query.length > 1) {
        $("#search-field").addClass("search-loading");
        // get data
        $.getJSON("/umbraco/api/Search/FindProjects/?query=" + query + "&parent=0&wildcard=true", function(data){
          // toggle UI
          if (data.length > 0) {
            $("#search-no-results").hide();            
            $("#repo-content").hide();
            $("#search-result-holder").empty();
            $("#search-result-holder").show();
        
            // apply the jquery templates
            $( "#search-result" ).tmpl( data).appendTo( "#search-result-holder"); 
          } else {
            $("#search-result-holder").hide();
            $("#repo-content").show();
            $("#search-query").text(query);
            $("#search-no-results").fadeIn('slow');            
          }

          $("#search-field").removeClass("search-loading");
        });
        
      } else {
        $("#search-no-results").hide();            
        $("#search-result-holder").hide();
        $("#repo-content").show();
      }
    }});
  });