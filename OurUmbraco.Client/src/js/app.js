//underscore templating
_.templateSettings = {
    interpolate: /\{\{(.+?)\}\}/g,
    evaluate: /\{\[([\s\S]+?)\]\}/g
};


jQuery(document).ready(function () {

    // Quick menu
    var bodyVar = $('body'),
        searchAll = $('.search-all');

    $(".user, .close").click(function (e) {
        e.preventDefault();
        e.stopPropagation();
        bodyVar.toggleClass("quickmenu");
    });

    $('.wrapper').on('click', function () {
        bodyVar.removeClass('quickmenu');
    });

    // Mobile navigation
    $("#toggle a").click(function (e) {
        e.preventDefault();
        $(".cross").toggleClass("open");
        $("nav").toggleClass("open");
        $("body").toggleClass("navopen");
    });

    $('.pm-nuget').on('click', '.nuget', function () {
        $(this).focus();
        $(this).select();
    });

    // Tab
    $('.tabs li').click(function () {
        var tab_id = $(this).attr('data-tab');

        $('.tabs li').removeClass('current');
        $('.tab-content').removeClass('current');

        $(this).addClass('current');
        $("#" + tab_id).addClass('current');
    });

    // Click effect
    var ink, d, x, y;
    $(".button, .inked").click(function (e) {

        if ($(this).find(".ink").length === 0) {
            $(this).prepend("<div class='ink'></div>");
        }

        ink = $(this).find(".ink");
        ink.removeClass("animate");

        if (!ink.height() && !ink.width()) {
            d = Math.max($(this).outerWidth(), $(this).outerHeight());
            ink.css({ height: d, width: d });
        }

        x = e.pageX - $(this).offset().left - ink.width() / 2;
        y = e.pageY - $(this).offset().top - ink.height() / 2;

        ink.css({ top: y + 'px', left: x + 'px' }).addClass("animate");
    });
});

var classOnScrollObject = function (selector, className, scrollDistance) {
    this.item = $(selector);
    this.itemClass = className;
    this.scrollDistance = scrollDistance;
    this.classApplied = false;
    this.scrollContainer = $(window);
    this.fromTop = this.scrollContainer.scrollTop();
};

classOnScrollObject.prototype.applyClass = function () {
    this.fromTop = this.scrollContainer.scrollTop();

    if (this.fromTop > this.scrollDistance && this.classApplied === false) {
        this.classApplied = true;
        this.item.addClass(this.itemClass);
    } else if (this.fromTop < this.scrollDistance && this.classApplied === true) {
        this.classApplied = false;
        this.item.removeClass(this.itemClass);
    }
};

function classOnScroll(selector, className, scrollDistance) {
    var newClassOnScroll = new classOnScrollObject(selector, className, scrollDistance);

    newClassOnScroll.applyClass();

    newClassOnScroll.scrollContainer.scroll(function () {
        newClassOnScroll.applyClass();
    });

}
