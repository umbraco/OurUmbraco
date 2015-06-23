// temporary sidebar fuctionality
	$('.level-1 li').on('click', function (e) {
		e.preventDefault();
		var bla = $(this).parent();
		$('.sidebar-content nav li').removeClass('active');
		$(this).parent('li').toggleClass('active');
		$(this).parent().siblings('li').removeClass('open');	
		$(this).parent('li').toggleClass('open');
	});