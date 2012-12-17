var s_currentLookUp = '';

var uForum = function() {
  return {
    ForumEditor : function(id){
      tinyMCE.init({
      // General options
      mode : "exact",
      elements : id,
      content_css : "/css/fonts.css",
      auto_resize : true,
      theme : "advanced",
      remove_linebreaks : false,
      relative_urls: false,
      plugins: "insertimage",
      theme_advanced_buttons1_add : "insertimage",
      theme_advanced_buttons1: "bold,strikethrough,|,bullist,numlist,|,link,unlink,formatselect,insertimage,code"
      });
    },
    NewTopic : function(s_forumId, s_title, s_body) {    
      $.post("/base/uForum/NewTopic/" + s_forumId + ".aspx", {title: s_title, body: s_body},
      function(data){       
         window.location = jQuery("value", data).text();
      });
    },
    EditTopic : function(s_topidId, s_title, s_body) {    
      $.post("/base/uForum/EditTopic/" + s_topidId + ".aspx", {title: s_title, body: s_body},
      function(data){       
         window.location = jQuery("value", data).text();
      });
    },
    NewComment : function(s_topicId, i_items, s_body) {
      $.post("/base/uForum/NewComment/" + s_topicId + "/" + i_items +".aspx", {body: s_body},
      function(data){
               var forceReload = false;
               forceReload = (window.location.href.indexOf("#") > -1);
         window.location = jQuery("value", data).text();
    
         if(forceReload)
        window.location.reload()
      });
    },
    EditComment : function(s_commentId, i_items, s_body) {
      $.post("/base/uForum/EditComment/" + s_commentId + "/" + i_items +".aspx", {body: s_body},
      function(data){
               var forceReload = false;
               forceReload = (window.location.href.indexOf("#") > -1);
         window.location = jQuery("value", data).text();
    
         if(forceReload)
        window.location.reload()
      });
    },
    lookUp : function() { 
      var s_q= jQuery("#title").val();
      s_q += " " + tinyMCE.get('topicBody').getContent();
      
      if(s_q.length > 10 && s_q != s_currentLookUp ){
        s_currentLookUp = s_q;
        $.post("/base/uSearch/FindSimiliarItems/forumTopics/20.aspx",{q: s_q},
        function(data){
        var html = "<ul class='summary'>";
        var found = false;
        jQuery.each( jQuery("result", data), function(index, value) { 
            var title = jQuery(value).find("Title").text();
            var topicId = jQuery(value).find("__NodeId").text();
          html += "<li><a href='#' rel='" + topicId + "' class='similarTopicLink'>" + title + "</a></li>";
          found = true; 
        });
        html += "</ul>"
          
        if(found){
        jQuery("#suggestedTopics").html(html);
        jQuery("#topicsBox").show();

        jQuery(".similarTopicLink").click(function()
          { 
                        var id = jQuery(this).attr('rel');
            $.post("/base/uForum/TopicUrl/" + id + ".aspx",function(data){
              window.open(jQuery("value", data).text());
          
              });
          });
        }

            //jQuery("#wikiContent").html( diffString(_c, jQuery("node/data [alias = 'bodyText']", data).text()) 
        //jQuery("#wikiContent").html( jQuery("node/data [alias = 'bodyText']", data).text() );
        
        }, "xml");      
      }else{
        //alert(s_q.lenght);
      }
    }   
  };
}();