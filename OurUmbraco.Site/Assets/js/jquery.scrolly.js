(function ($) {
	// Define the jscroll namespace and default settings
	$.scrolly = {
		defaults: {
			page: 1,
			loadingHtml: '<small>Loading...</small>',
			nextSelector: 'a:last',
			contentSelector: '',
			callback: false
		}
	};

	// Constructor
	var scrolly = function ($e, options) {
		console.log($e);

		// Private vars
		var _data = $e.data('scrolly'),
            _userOptions = (typeof options === 'function') ? { callback: options } : options,
            _options = $.extend({}, $.scrolly.defaults, _userOptions, _data || {}),
            _isWindow = ($e.css('overflow-y') === 'visible'),
            _$next = $e.find(_options.nextSelector).first(),
            _$window = $(window),
            _$body = $('body'),
            _$scroll = _isWindow ? _$window : $e,
            _nextHref = $.trim(_$next.attr('href') + ' ' + _options.contentSelector);

		// Initialization
		$e.data('scrolly', $.extend({}, _data, { initialized: true, waiting: false, nextHref: _nextHref }));
		_setBindings();

		// Find the next link's parent, or add one, and hide it
		function _nextWrap($next) {
			
			var $parent = $next.parent().not('.jscroll-inner,.jscroll-added').addClass('jscroll-next-parent').hide();
			if (!$parent.length) {
				$next.wrap('<div class="jscroll-next-parent" />').parent().hide();
			}
			
		}

		// Observe the scroll event for when to trigger the next load
		function _observe() {

			var $inner = $e.find('tr').last(),
                data = $e.data('scrolly'),
                borderTopWidth = parseInt($e.css('borderTopWidth')),
                borderTopWidthInt = isNaN(borderTopWidth) ? 0 : borderTopWidth,
                iContainerTop = parseInt($e.css('paddingTop')) + borderTopWidthInt,
                iTopHeight = _isWindow ? _$scroll.scrollTop() : $e.offset().top,
                innerTop = $inner.length ? $inner.offset().top : 0,
                iTotalHeight = Math.ceil(iTopHeight - innerTop + _$scroll.height() + iContainerTop);

			if (!data.waiting && iTotalHeight >= $inner.outerHeight()) {
				//data.nextHref = $.trim(data.nextHref + ' ' + _options.contentSelector);
				console.log("loading");
				_load();
			}
		}
		// Check if the href for the next set of content has been set
		function _checkNextHref(data) {
			data = data || $e.data('scrolly');
			if (!data || !data.nextHref) {
				return false;
			} else {
				_setBindings();
				return true;
			}
		}
		function _setBindings() {
			var $next = $e.find(_options.nextSelector).first();
			
			_nextWrap($next);
			if (_$body.height() <= _$window.height()) {
				_observe();
			}
			_$scroll.unbind('.scrolly').bind('scroll.scrolly', function () {
				return _observe();
			});
			
			
		}


		function _load() {
			var $inner = $e.find('tbody').last(),
                data = $e.data('scrolly');

			data.waiting = true;
			$('<div class="scrolly-loading">' + _options.loadingHtml + '</div>').insertAfter($inner);

			return $e.animate({ scrollTop: $inner.outerHeight() }, 0, function () {
				
				//needs to be replaced with ajax call and the actual content we want to fetch
				var i = 0;
				while (i < 50) {
					$inner.append("<tr><td>test</td></tr>");
					
					i++;
				}
				
				_options.page ++;
				
					
				var $next = $(this).find(_options.nextSelector).first();
				data.waiting = false;
				data.nextHref = $next.attr('href') ? $.trim($next.attr('href') + ' ' + _options.contentSelector) : false;
				_checkNextHref();
				if (_options.callback) {
					_options.callback.call(this);
				}
				
			});
		}
		return $e;

	};

	// Define the scrolly plugin method and loop
	$.fn.scrolly = function (m) {
		return this.each(function () {
			var $this = $(this),
                data = $this.data('scrolly');
			// Instantiate scrolly on this element if it hasn't been already
			if (data && data.initialized) return;
			var scrol = new scrolly($this, m);
		});
	};
})(jQuery);