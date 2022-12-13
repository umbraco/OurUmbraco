angular.module('umbraco').controller('SkybrudUmbracoRedirects.Dashboard.Controller', function ($scope, $http, $q, $timeout, dialogService, notificationsService, localizationService, skybrudRedirectsService) {

    $scope.redirects = [];
    $scope.mode = 'list';

    $scope.rootNodes = [
        { name: 'All sites', value: '' }
    ];

    localizationService.localize('redirects_allSites').then(function (value) {
        $scope.rootNodes[0].name = value;
    });

    $scope.types = [
        { name: 'All types', value: 'all' },
        { name: 'Content', value: 'content' },
        { name: 'Media', value: 'media' },
        { name: 'URL', value: 'url' }
    ];

    angular.forEach($scope.types, function (type) {
        localizationService.localize('redirects_media_' + type.value).then(function (value) {
            type.name = value;
        });
    });

    $scope.filters = {
        rootNode: $scope.rootNodes[0],
        type: $scope.types[0],
        text: ''
    };

    $scope.activeFilters = 0;

    $scope.loading = false;

    // Opens a dialog for adding a new redirect. When a callback received, the list is updated.
    // Since we don't have any sorting, we can assume the added redirect will be shown last
    $scope.addRedirect = function () {
        skybrudRedirectsService.addRedirect({
            callback: function () {
                $scope.updateList(1);
            }
        });
    };

    // Opens a dialog for adding a new redirect. When a callback received, the list is updated.
    $scope.editRedirect = function (redirect) {
        skybrudRedirectsService.editRedirect(redirect, function () {
            $scope.updateList();
        });
    };

    $scope.deleteRedirect = function (redirect) {
        var url = redirect.url + (redirect.queryString ? '?' + redirect.queryString : '');
        if (!confirm('Are you sure you want do delete the redirect at "' + url + '" ?')) return;
        skybrudRedirectsService.deleteRedirect(redirect, function () {
            $scope.updateList();
        });
    };
    
    // Initial pagination options
    $scope.pagination = {
        text: '',
        page: 1,
        pages: 0,
        limit: 11,
        offset: 0,
        pagination: []
    };

    // Loads the previous page
    $scope.prev = function () {
        if ($scope.pagination.page > 1) $scope.updateList($scope.pagination.page - 1);
    };

    // Loads the next pages
    $scope.next = function () {
        if ($scope.pagination.page < $scope.pagination.pages) $scope.updateList($scope.pagination.page + 1);
    };

    // Updates the list based on current arguments
    $scope.updateList = function (page) {

        $scope.loading = true;

        // If a page is specified, we load that page
        page = (page ? page : $scope.pagination.page);

        // Declare the arguments (making up the query string) for the call to the API
        var args = {
            limit: $scope.pagination.limit,
            page: page
        };

        $scope.activeFilters = 0;

        // Any filters?
        if ($scope.filters.rootNode && $scope.filters.rootNode.id > 0) {
            args.rootNodeId = $scope.filters.rootNode.id;
            $scope.activeFilters++;
        }

        // Any filters?
        if ($scope.filters.type.value != 'all') {
            args.type = $scope.filters.type.value;
            $scope.activeFilters++;
        }

        if ($scope.filters.text) {
            args.text = $scope.filters.text;
            $scope.activeFilters++;
        }
        
        // Declare the HTTP options
        var http = $http({
            method: 'GET',
            url: '/umbraco/backoffice/api/Redirects/GetRedirects',
            params: args
        });

        // Show the loader for at least 200 ms
        var timer = $timeout(function () { }, 200);

        // Wait for both the AJAX call and the timeout
        $q.all([http, timer]).then(function (array) {

            $scope.loading = false;
            $scope.redirects = array[0].data.items;

            // Update our pagination model
            $scope.pagination = array[0].data.pagination;
            $scope.pagination.pagination = [];

            var from = Math.max(1, $scope.pagination.page - 7);
            var to = Math.min($scope.pagination.pages, $scope.pagination.page + 7);

            for (var i = from; i <= to; i++) {
                $scope.pagination.pagination.push({
                    page: i,
                    active: $scope.pagination.page == i
                });
            }

            var tokens = [
                $scope.pagination.from,
                $scope.pagination.to,
                $scope.pagination.total,
                $scope.pagination.page,
                $scope.pagination.pages
            ];

            localizationService.localize('redirects_pagination', tokens).then(function (value) {
                $scope.pagination.text = value;
            });

        }, function () {
            notificationsService.error('Unable to load redirects', 'The list of redirects could not be loaded.');
            $scope.loading.list = false;
        });

    };

    skybrudRedirectsService.getRootNodes().success(function (r) {
        angular.forEach(r.data, function (rootNode) {
            $scope.rootNodes.push(rootNode);
        });
    });

    $scope.updateList();

    $scope.$watch('filters', function () {
        $scope.updateList();
    }, true);

});