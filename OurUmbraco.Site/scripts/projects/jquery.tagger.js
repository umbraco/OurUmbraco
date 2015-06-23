(function($) {

    $.fn.addTag = function(v,projectid) {
        var r = v.split(',');
        for (var i in r) {
            //n = r[i].replace(/([^a-zA-Z0-9\s\-\_])|^\s|\s$/g, '');
	    n = r[i];
            if (n == '') return false;
            var fn = $(this).data('name');
            var i = $('<input type="hidden" />').attr('name', fn).val(n);
            var t = $('<li />').text(n).addClass('tagName')
				.click(function() {
				    // remove
				    
                                    if(projectid != null){RemoveTagFromProject(projectid,$(this).text());}
				    var hidden = $(this).data('hidden');
				    $(hidden).remove();
				    $(this).remove();
				})
				.data('hidden', i);
            var l = $(this).data('list');
            $(l).append(t).append(i);
        }
    };

})(jQuery);

function enableTagger(projectid)
{
    $('.tagger').each(function(i) {
        $(this).data('name', $(this).attr('name'));
        $(this).removeAttr('name');
        var b = $('<button type="button">Add</button>').addClass('tagAdd')
			.click(function() {
			    var tagger = $(this).data('tagger');
			    $(tagger).addTag($(tagger).val(),projectid);

			    if(projectid != null)
				{
			    		AddTagToProject(projectid,$(tagger).val());
				}
	                    $(tagger).val('');
			    $(tagger).stop();
			})
			.data('tagger', this);
        var l = $('<ul />').addClass('tagList');
        $(this).data('list', l);
        $(this).after(l).after(b);
    })
	.bind('keypress', function(e) {
	    if (13 == e.keyCode) {
	        //console.log(e.keyCode);
	        $(this).addTag($(this).val());
		
	    	if(projectid != null)
		{
			AddTagToProject(projectid,$(this).val());
		}
	        $(this).val('');
	        $(this).stop();
	        return false;
	    }
	});
}


