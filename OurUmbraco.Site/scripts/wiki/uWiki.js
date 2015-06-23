var uWiki = function() {
  return {
    Edit: function(s_baseId, s_baseVersion,readOnlyTitle) {
        
    //fetch content from the server in case someone has changed it
    $.post("/umbraco/api/Wiki/GetContentVersion/?id=" + s_baseId + "&guid=" + s_baseVersion,
      function(data){
        
          if(_currentContent == ''){
          _currentContent = jQuery("#wikiContent").html();
          _currentTitle = jQuery("#wikiHeader").html();
           }
        
       jQuery("#wikiContent").html( jQuery("bodyText", data).text() );
       jQuery("#wikiHeader").html( _currentTitle, jQuery("node", data).attr("nodeName") );
      
      tinyMCE.init({
      // General options
      mode : "exact",
      elements : "wikiContent",
      content_css : "/css/fonts.css",
      auto_resize : true,
      theme : "advanced",
      remove_linebreaks : false,
      relative_urls: false,
      plugins: "insertimage",
      theme_advanced_buttons1_add : "insertimage",
      theme_advanced_buttons1: "bold,strikethrough,|,bullist,numlist,|,link,unlink,formatselect,insertimage,code"
      });

    jQuery("#editMode").show();
    jQuery("#tab_edit").addClass("ui-tabs-selected");
    
    jQuery("#divKeywords").show();
        
    jQuery(".wiki-sidebar").hide();
        
    jQuery("#wikiHeaderEditor").val(jQuery("#wikiHeader").text() );
        
    if(!readOnlyTitle)
    {
      jQuery("#wikiHeader").hide();
      jQuery("#wikiHeaderEditor").show();
    }
    });    

    

    },
    NewEditor: function(readOnlyTitle) {
      tinyMCE.init({
      // General options
      mode : "exact",
      elements : "wikiContent",
      content_css : "/css/fonts.css",
      auto_resize : true,
      theme : "advanced",
      remove_linebreaks : false,
      relative_urls: false,
      plugins: "insertimage",
      theme_advanced_buttons1_add : "insertimage",
      theme_advanced_buttons1: "bold,strikethrough,|,bullist,numlist,|,link,unlink,formatselect,insertimage,code"
      });

    
    jQuery("#viewMode").hide();
    jQuery("#editMode").show();
    
    jQuery("#wikiHeaderEditor").val(jQuery("#wikiHeader").text());
      
    if(!readOnlyTitle)
    {
      jQuery("#wikiHeader").hide();
      jQuery("#wikiHeaderEditor").show();
    }
    
    jQuery("#wikiKeywordsContainer").show();

      

      
      
    },
    Save: function(s_pageId, s_title, s_body, s_keywords) {
        $.post("/umbraco/api/Wiki/Update/", { pageId: s_pageId, body: s_body, title: s_title, keywords: s_keywords },
      function(data){
         window.location = jQuery("value", data).text();
      });
      
      uWiki.Cancel(false);
    },
    Create: function(s_parentId, s_title, s_body,s_keywords){
      $.post("/umbraco/api/Wiki/Create/", {parentId: s_parentId, body: s_body, title: s_title, keywords: s_keywords},
      function(data){
        window.location = jQuery("value", data).text();
      });

      uWiki.Cancel(false);
    },
    Cancel: function(rollback){
      
      jQuery("#tab_edit").removeClass("ui-tabs-selected");

      tinyMCE.execCommand('mceRemoveControl', false, 'wikiContent');
      
      jQuery("#editMode").hide();
      
      jQuery(".wiki-sidebar").show();      

      jQuery("#wikiHeaderEditor").hide();
      jQuery("#wikiHeader").show();
            
      jQuery("#divKeywords").hide();
      
      if(rollback){
        jQuery("#wikiContent").html(_currentContent);
        jQuery("#wikiHeader").html(_currentTitle);
      }else{
        jQuery("#wikiHeader").html(jQuery("#wikiHeaderEditor").val());
      }
    },
    PreviewOldVersion: function(s_pageId, s_versionGuid, s_currentVersionGuid){
      $.post("/umbraco/api/Wiki/GetContentVersion/?id=" + s_pageId + "&guid=" + s_versionGuid,
      function(data){
          //jQuery("#wikiContent").html( diffString(_c, jQuery("node/data [alias = 'bodyText']", data).text()) );
          jQuery("#wikiContent").html( jQuery("node/data [alias = 'bodyText']", data).text() ).effect('highlight');
      }
      );
    },
    Rollback: function(s_pageId, s_versionGuid){
      $.post("/umbraco/api/Wiki/Rollback/?pageId=" + s_pageId + "&guid=" + s_versionGuid,
      function(data){
          window.location = jQuery("value", data).text();
      });
    },
    ClearHelpRequests: function(s_section, s_applicationPage){
      
        $.get("/umbraco/api/Wiki/ClearHelpRequests/?section=" + s_section + "&applicationPage=" + s_applicationPage);
    }
  };
}();

var _currentContent = "";
var _currentTitle = "";