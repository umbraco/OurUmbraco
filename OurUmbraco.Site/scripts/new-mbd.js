// Umbraco Projects JS by Mark Boulton Design

$(function () {

    applyTips();
    setupSearchField();
    setSearchSectionLabels();

    $('.projectWidgets .popnav').click(function () {
        loadListings($(this).attr('id').substring(3), 'popular', 8);
        $('.projectWidgets .popnav').removeClass('on');
        $(this).addClass("on");
        return false;
    });

    $('.projectWidgets .newnav').click(function () {
        loadListings($(this).attr('id').substring(3), 'newest', 4);
        $('.projectWidgets  .newnav').removeClass('on');
        $(this).addClass("on");
        return false;
    });


});

tip = function (e, t) {
	
	if (!$(t).find('.tip').length) {
		
		$('.tip').remove();
		
		var d = '<div class="tip">' + $(t).html() + '<a href="' + $(t).find('h3 a').attr('href') + '" class="download">Download</a></div>';
		
		if (e.preventDefault) e.preventDefault();
		else event.returnValue = false;
		
		$(t).append($(d).css({ top: '-185px', opacity: '0' }).animate({ top: ['-205px', 'easeOutBounce'], opacity: '1' }, 550, function () {
			
			$(document).unbind('mouseover').bind('mouseover', function(e) {
				
				var t = e.target;
				
				if (!$(t).parents('.tip').length && !$(t).parents('.deliPromoBox li').length && !$(t).children('.tip').length && $(t).attr('class') != 'tip') {
				
					$('.tip').remove();
					$(document).unbind('mouseover');
				}		
				
			});
			
		}));
	
	}

};

applyTips = function () {
    $('.deliPromoBox li').not('.promoOptions li').not('.deliPaging li').bind('mouseover', function (e) { tip(e, this); });

    $('#sectionspan').click(function () {

        if ($('#sections:hidden').length) {

            $('#sections').show();

            setTimeout(function () {

                $(document).unbind('click').bind('click', function (e) {

                    var t = e.target;

                    if ((!$(t).parents('#sections').length && !$(t).attr('id') != 'sections')) {

                        $('#sections').hide();
                        $(document).unbind('click');
                        setSearchSectionLabels();

                    }

                });

            }, 10);
        }
        else {
            $('#sections').hide();
            setSearchSectionLabels();
        }

    });
};

setSearchSectionLabels = function () {

    var sectionLabel = '';
    var sectionCount = 0;
    $('#sections input:checked').each(function () {
        var checkId = $(this).attr('id');
        sectionLabel += $('label[for=' + checkId + ']').text() + ',';
        sectionCount++;
    });
    if (sectionCount == 0 || sectionCount == $('#sections input').length) {
        sectionLabel = 'All';
    } else {
        sectionLabel = sectionLabel.substring(0, sectionLabel.length - 1);
        var secs = sectionLabel.split(',');
        sectionLabel = "";
        if (secs.length > 1) {
            for (var i = 0; i < secs.length; i++) {
                sectionLabel += secs[i].substring(0, 3) + ", ";
            }
            sectionLabel = sectionLabel.substring(0, sectionLabel.length - 2);
        } else {
            sectionLabel = secs[0];
        }
    }

    $('#sectionspan label').text(sectionLabel);
    $('label.sectiontab').text(sectionLabel);
}


loadListings = function (listingtype, listing, count) {
    $("#" + listing + "-projects").fadeOut(function () {
        $("#" + listing + "-loading").fadeIn();
        $.getJSON("/base/deli/Get" + listing.capitalize() + "Listings/" + listingtype + "/0/" + count + "", function (data) {
            if (data.length > 0) {
                $("#" + listing + "-projects").empty();
                $("#listing-result").tmpl(data).appendTo("#" + listing + "-projects");
            }
            $("#" + listing + "-loading").fadeOut(function () {
                applyTips();
                $("#" + listing + "-projects").fadeIn();
            });
        });
    });
}

String.prototype.capitalize = function () {
    return this.replace(/(^|\s)([a-z])/g, function (m, p1, p2) { return p1 + p2.toUpperCase(); });
};

setupSearchField = function () {
    
    //onload if the search field has a value because of previous search hide the content of the searchfield
    if ($('#searchField').val().length > 0) {
        $('#searchlabel').hide();
    }

    $('#searchField').focus(function () {
        $('#searchlabel').fadeOut();
    }).blur(function () {
        if ($(this).val().length == 0) {
            $('#searchlabel').fadeIn();
        }
    }).bind("keypress", function (e) {
        if (e.keyCode == 13) {
            doSearch();
            return false;
        }
    });

    $("#searchbutton").click(function (e) {
        doSearch();
        return false;
    });


}