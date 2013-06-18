var completeStates = ['fixed', 'fixed awaiting retest', 'incomplete', 'obsolete', 'won\'t fix', 'Duplicate', 'can\'t reproduce', 'duplicate', 'closed'];
var inProgressStates = ['in progress'];
var issueTypes = ['bug','exception','performance problem','auto-reported exception','cosmetics','usability problem','meta issue','task'];
var featureTypes = ['feature (planned)','feature (request)'];
var featureRequestTypes = ['feature (request)'];
// a global month names array
var gsMonthNames = ['January','February','March','April','May','June','July','August','September','October','November','December'];
// a global day names array
var gsDayNames = ['Sunday','Monday','Tuesday','Wednesday','Thursday','Friday','Saturday'];

// the date format prototype
Date.prototype.format = function(f)
{
    if (!this.valueOf())
        return '&nbsp;';

    var d = this;

    return f.replace(/(yyyy|mmmm|mmm|mm|dddd|ddd|dd|hh|nn|ss|a\/p)/gi,
        function($1)
        {
            switch ($1.toLowerCase())
            {
            case 'yyyy': return d.getFullYear();
            case 'mmmm': return gsMonthNames[d.getMonth()];
            case 'mmm':  return gsMonthNames[d.getMonth()].substr(0, 3);
            case 'mm':   return (d.getMonth() + 1).zf(2);
            case 'dddd': return gsDayNames[d.getDay()];
            case 'ddd':  return gsDayNames[d.getDay()].substr(0, 3);
            case 'dd':   return d.getDate().zf(2);
            case 'hh':   return ((h = d.getHours() % 12) ? h : 12).zf(2);
            case 'nn':   return d.getMinutes().zf(2);
            case 'ss':   return d.getSeconds().zf(2);
            case 'a/p':  return d.getHours() < 12 ? 'a' : 'p';
            }
        }
    );
}

// Declare mapping options
var versionMappingOptions = {
    'create': function (o) {

        // Peform the default map
        var version = ko.mapping.fromJS(o.data);

        // Create a web safe ID for a version
        version.id = ko.computed(function () {
            return 'v' + this.version().replace(/[^a-zA-Z0-9]+/g, '');
        }, version);


        version.issueIssues = ko.computed(function () {
            return ko.utils.arrayFilter(this.issues(), function (issue) {
                return $.inArray(issue.type().toLowerCase(), issueTypes) > -1;
            });
        }, version);

        version.featureIssues = ko.computed(function () {
            return ko.utils.arrayFilter(this.issues(), function (issue) {
                return $.inArray(issue.type().toLowerCase(), featureTypes) > -1 || ($.inArray(issue.type().toLowerCase(),featureRequestTypes) > -1 && $.inArray(issue.state().toLowerCase(), completeStates) > -1);
            });
        }, version);

        version.breakingIssues = ko.computed(function (){
            return ko.utils.arrayFilter(this.issues(), function(issue){
                return issue.breaking();
            });           
        }, version);
        

        // Create filtered lists of issues
        version.completedIssues = ko.computed(function () {
            return ko.utils.arrayFilter(this.issues(), function (issue) {
                return $.inArray(issue.state().toLowerCase(), completeStates) > -1;
            });
        }, version);
        version.inProgressIssues = ko.computed(function () {
            return ko.utils.arrayFilter(this.issues(), function (issue) {
                return $.inArray(issue.state().toLowerCase(), inProgressStates) > -1;
            });
        }, version);
        version.notStartedIssues = ko.computed(function () {
            return ko.utils.arrayFilter(this.issues(), function (issue) {
                return $.inArray(issue.state().toLowerCase(), completeStates) == -1 && $.inArray(issue.state().toLowerCase(), inProgressStates) == -1;
            });
        }, version);

        // Calculate percentages
        version.percentComplete = ko.computed(function () {
            var totalIssuesCount = this.issues().length;
            if (totalIssuesCount == 0)

                return 0;
            var completedIssuesCount = this.completedIssues().length;
            return Math.round((100 / totalIssuesCount) * completedIssuesCount);
        }, version);
        version.percentInProgress = ko.computed(function () {
            var totalIssuesCount = this.issues().length;
            if (totalIssuesCount == 0)
                return 0;
            var inProgressIssuesCount = this.inProgressIssues().length;
            return Math.round((100 / totalIssuesCount) * inProgressIssuesCount);
        }, version);

        // Create helper method for toggling views
        version.toggle = function () {
            this.open(!this.open());
        };
        return version;
    }
};

// Declare view model
var viewModel = {
    versions: ko.observableArray([])
};

viewModel.currentReleases = ko.computed(function(){
    return ko.utils.arrayFilter(viewModel.versions(), function (ver) {
                return ver.currentRelease();
            });
    },viewModel);


viewModel.inProgressReleases = ko.computed(function(){
    return ko.utils.arrayFilter(viewModel.versions(), function (ver) {
                return ver.inProgressRelease();
            });
    },viewModel);

viewModel.futureReleases = ko.computed(function(){
    return ko.utils.arrayFilter(viewModel.versions(), function (ver) {
                return ver.plannedRelease();
            });
    },viewModel);

viewModel.releasedReleases = ko.computed(function(){
    return ko.utils.arrayFilter(viewModel.versions(), function (ver) {
                return ver.released();
            });
    },viewModel);

viewModel.comingReleases = ko.computed(function(){
    return ko.utils.arrayFilter(viewModel.versions(), function (ver) {
                return (!ver.released() && !ver.isPatch());
            });
    },viewModel);

viewModel.patchReleases = ko.computed(function(){
    return ko.utils.arrayFilter(viewModel.versions(), function (ver) {
                return (!ver.released() && ver.isPatch());
            });
    },viewModel);

viewModel.historicalReleases = ko.computed(function(){
    return ko.utils.arrayFilter(viewModel.versions(), function (ver) {
                return (ver.released() && !ver.latestRelease());
            });
    },viewModel);


// Declare loader function
loadData = function (versionId) {
    $.getJSON("/api/aggregate/" + versionId, function (data) {

        // Parse result
        ko.mapping.fromJS(data, versionMappingOptions, viewModel.versions);

        // Reload data
        //setTimeout(loadData, 60000);

        $('.progress span').each(function(i,e){
                var progressItem = $(this);
                if(progressItem.attr('title') > 0){
                    progressItem.countTo({
                        from: 0,
                        to: progressItem.attr('title'),
                        speed: 1200,
                        refreshInterval: 50,
                    });
                }
            });

    });
};

// Declare loader function
loadAllData = function () {
    $.getJSON("/api/GetAllFromFile/", function (data) {

        // Parse result
        ko.mapping.fromJS(data, versionMappingOptions, viewModel.versions);

        // Reload data
        //setTimeout(loadData, 60000);

        $('.progress span').each(function(i,e){
                var progressItem = $(this);
                if(progressItem.attr('title') > 0){
                    progressItem.countTo({
                        from: 0,
                        to: progressItem.attr('title'),
                        speed: 1200,
                        refreshInterval: 50,
                    });
                }
            });

    });
};

// Initialize

// Knockout extentions
ko.observable.fn.prettyDate = function () {
    return humaneDate(new Date(this()));
};

ko.bindingHandlers.slideVisible = {
    init: function (element, valueAccessor) {
        // Initially set the element to be instantly visible/hidden depending on the value
        var value = valueAccessor();
        $(element).toggle(ko.utils.unwrapObservable(value)); // Use "unwrapObservable" so we can handle values that may or may not be observable
    },
    update: function (element, valueAccessor) {
        // Whenever the value subsequently changes, slowly fade the element in or out
        var value = valueAccessor();
        ko.utils.unwrapObservable(value) ? $(element).slideDown() : $(element).slideUp();
    }
};


(function($) {
    $.fn.countTo = function(options) {
        // merge the default plugin settings with the custom options
        options = $.extend({}, $.fn.countTo.defaults, options || {});

        // how many times to update the value, and how much to increment the value on each update
        var loops = Math.ceil(options.speed / options.refreshInterval),
            increment = (options.to - options.from) / loops;

        return $(this).each(function() {
            var self = this,
                loopCount = 0,
                value = options.from,
                interval = setInterval(updateTimer, options.refreshInterval);

            function updateTimer() {
                value += increment;
                loopCount++;
                $(self).html(value.toFixed(options.decimals));

                if (typeof(options.onUpdate) == 'function') {
                    options.onUpdate.call(self, value);
                }

                if (loopCount >= loops) {
                    clearInterval(interval);
                    value = options.to;

                    if (typeof(options.onComplete) == 'function') {
                        options.onComplete.call(self, value);
                    }
                }
            }
        });
    };

    $.fn.countTo.defaults = {
        from: 0,  // the number the element should start at
        to: 100,  // the number the element should end at
        speed: 1000,  // how long it should take to count between the target numbers
        refreshInterval: 100,  // how often the element should be updated
        decimals: 0,  // the number of decimal places to show
        onUpdate: null,  // callback method for every time the element is updated,
        onComplete: null,  // callback method for when the element finishes updating
    };
}(jQuery));