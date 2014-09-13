/* INIT */
jQuery(document).ready(function(){

	$('#f_search').bind("keypress", function(e) {
             if (e.keyCode == 13) {
                 doSearch();
                 return false;}
         });
	
	$("#bt_search").click(function(e) {
            	doSearch();
		return false;
         });
});

/* SEARCH */
function doSearch(){
if ($('#f_search').val() != '')
	window.location = "?mode=search&q=" + jQuery('#f_search').val();
}

/* FORUM */
var mForum = function() {
	return {
		NewTopic : function(s_forumId, s_title, s_body) {		
			$.post("/umbraco/api/Forum/NewTopic/?forumId=" + s_forumId, {title: s_title, body: s_body},
			function(data){		   
			   window.location = jQuery("value", data).text();
			});
		},
		NewComment : function(s_topicId, i_items, s_body) {
		    $.post("/umbraco/api/Forum/NewComment/?topicId=" + s_topicId + "&itemsPerPage=" + i_items, { body: s_body },
			function(data){
		           var forceReload = false;
		           forceReload = (window.location.href.indexOf("#") > -1);
			   window.location = jQuery("value", data).text();
		
			   if(forceReload)
				window.location.reload()
			});
		}
	};
}();