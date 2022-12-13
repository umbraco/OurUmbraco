angular.module('umbraco').controller('SkybrudUmbracoRedirects.PropertyEditor.Controller', function ($scope, $routeParams, $http, $q, $timeout, dialogService, notificationsService, skybrudRedirectsService) {

    $scope.route = $routeParams;
    $scope.redirects = [];

    $scope.mode = $routeParams.create ? 'create' : 'list';
    $scope.type = $scope.route.section;

    $scope.loading = false;

    $scope.showTitle = $scope.model.config !== '1';

    // If we're neither in the content or media section, we stop further execution (eg. property editor preview)
    if ($scope.type != 'content' && $scope.type != 'media') return;

    $scope.addRedirect = function () {
        if ($scope.type == 'content') {
            skybrudRedirectsService.addRedirect({
                content: $scope.$parent.$parent.$parent.content,
                hideRootNodeOption: $scope.model.config.hideRootNodeOption,
                callback: function () {
                    $scope.updateList();
                }
            });
        } else if ($scope.type == 'media') {
            skybrudRedirectsService.addRedirect({
                hideRootNodeOption: $scope.model.config.hideRootNodeOption,
                media: $scope.$parent.$parent.$parent.content,
                callback: function () {
                    $scope.updateList();
                }
            });
        }
    };

    $scope.editRedirect = function (redirect) {
        skybrudRedirectsService.editRedirect(redirect, {
            hideRootNodeOption: $scope.model.config.hideRootNodeOption,
            callback: function () {
                $scope.updateList();
            }
        });
    };

    $scope.deleteRedirect = function (redirect) {
        var url = redirect.url + (redirect.queryString ? '?' + redirect.queryString : '');
        if (!confirm('Are you sure you want do delete the redirect at "' + url + '" ?')) return;
        skybrudRedirectsService.deleteRedirect(redirect, function () {
            $scope.updateList();
        });
    };

    $scope.updateList = function () {

        $scope.loading = true;

        // Make the call to the redirects API
        var http = $http({
            method: 'GET',
            url: '/umbraco/backoffice/api/Redirects/GetRedirectsFor' + $scope.type,
            params: {
                contentId: $routeParams.id
            }
        });

        // Show the loader for at least 200 ms
        var timer = $timeout(function () { }, 200);

        // Wait for both the AJAX call and the timeout
        $q.all([http, timer]).then(function (array) {
            $scope.content = array[0].data.data.content;
            $scope.redirects = array[0].data.data.redirects;
            $scope.loading = false;
        }, function () {
            notificationsService.error('Unable to load redirects', 'The list of redirects could not be loaded.');
            $scope.loading = false;
        });

    };

    if ($scope.mode == 'list') {
        $scope.updateList();
    }

});