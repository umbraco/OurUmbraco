! function (name, definition) {
    if (typeof module != 'undefined' && module.exports) module.exports = definition();
    else if (typeof define == 'function') define(definition);
    else this[name] = definition();
}('tabs', function () {

    return function tabs(container) {
        var tabs = container.querySelectorAll('.flexi-tabs__tab');
        var panes = container.querySelectorAll('.flexi-tabs__tab-panel');

        each(tabs, function (i, tab) {      
            tab.addEventListener('click', function (e) {
                activate(tabs, i, 'flexi-tabs__tab_selected');
                activatepanes(panes, i, 'flexi-tabs__tab-panel_hidden');
            });
        });

        function activate(tabs, index, activeCssClass) {
            each(tabs, function (i, tab) {
                if (i != index) {
                    removeClass(tab, activeCssClass);
                } else {
                    addClass(tab, activeCssClass);
                }
            });
        }
        function activatepanes(panes, index, activeCssClass) {
            each(panes, function (i, pane) {
                if (i != index) {
                    addClass(pane, activeCssClass);
                } else {
                    removeClass(pane, activeCssClass);
                }
            });
        }
    };


    function each(elements, fn) {
        for (var i = elements.length - 1; i >= 0; i--) {
            fn(i, elements[i]);
        }
    }

     function addClass(el, cls) {
        if (!el.classList.contains(cls)) {
            el.classList.add(cls);
        }
    }

    function removeClass(el, cls) {
        if (el.classList.contains(cls)) {
            el.classList.remove(cls);
        }
    }
});

function initTabs() {
    var container = document.querySelector('.flexi-tabs');
    if (container !== null) {
        tabs(container);
    }
}

initTabs();