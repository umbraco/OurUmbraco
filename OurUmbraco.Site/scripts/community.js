/* INIT */
jQuery(document).ready(function(){

	$("#forum td.body").each(function(){
		var str = $(this).html();

		str = str.replace(RegExp("(\\w{100})(\\w)", "g"), 
			function(all,text,char){ return text + "<wbr>&shy;" + char; }
			);
 
		$(this).html(str);
      	});
	

//	$('#search').bind("keypress", function(e) {
//             if (e.keyCode == 13) {
//                 doSearch();
//                 return false;}
//         });
	
//	$('#search').focus(function(){
//		if( !$("#searchBarOptions").is(":visible"))
//			$("#searchBarOptions").fadeIn(1000);
//	}).blur(function(){
//		if( $(this).val() == '')
//			$("#searchBarOptions").fadeOut(200);
//	});

//	$("#searchbutton").click(function(e) {
//            	doSearch();
//		return false;
//         });


	$("#forum a.forumToggleComment").click(function(e) {
		var rel = jQuery(this).attr("rel");
            	jQuery('#collapsed' + rel).hide();
		jQuery('#' + rel).show().addClass("highlighted");
		
		return false;
         });

});

/* SEARCH */
function doSearch(){
    if ($('#searchField').val() != '')
	var types = "";
	
	$.each( $("input[@name='contentType[]']:checked"), function() {
  		types += $(this).val() + ",";
	});

window.location = "/search?q=" + encodeURIComponent(jQuery('#searchField').val()) + "&content=" + types;
}



function killButton(button, label){
	var b = $(button);
	b.after("<em class='yellow'>" + label + "</em>");
	b.remove();
	return false;
} 


var topNotification = function() {
	return {
		ShowMessage : function(message){
			$("#topNotificationContainer").children().remove();
			$("#topNotificationContainer").append("<div class='topNotification'><span class='notifyClose'>x</span>" +message+ "</div>");
			
			$(".notifyClose").click(function(){
				topNotification.HideMessage();
			});			

			$("#topNotificationContainer").fadeIn('slow');
		},
		HideMessage: function(){
			$("#topNotificationContainer").fadeOut('slow');
		}
	};
}();



