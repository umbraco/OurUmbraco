angular.module('umbraco').controller('SkybrudUmbracoRedirects.AddRedirectDialog.Controller', function ($scope, $http, notificationsService, skybrudRedirectsService, localizationService) {

    // Get the Umbraco version
    var v = Umbraco.Sys.ServerVariables.application.version.split('.');
    $scope.gte74 = v[0] == 7 && v[1] >= 4;
    $scope.gte76 = v[0] == 7 && v[1] >= 6;

    $scope.options = $scope.dialogOptions.options || {};

    $scope.type = $scope.options.media ? 'media' : 'content';
    $scope.content = $scope.options.content;
    $scope.media = $scope.options.media;

    $scope.hideRootNodeOption = $scope.options.hideRootNodeOption === '1' || $scope.options.hideRootNodeOption === true;

    $scope.redirect = {
        rootNodeId: 0,
        url: '',
        link: null,
        permanent: true,
        regex: false,
        forward: false
    };

    $scope.rootNodes = [
        { id: 0, name: 'All sites' }
    ];

    $scope.rootNode = $scope.rootNodes[0];

    if ($scope.content) {
        $scope.redirect.link = {
            id: $scope.content.id,
            name: $scope.content.name,
            url: $scope.content.urls.length > 0 ? $scope.content.urls[0] : '#',
            mode: 'content'
        }
    } else if ($scope.media) {

        // $scope.media doesn't expose the URL directly, so we need to read it from the "_umb_urls" property
        var mediaUrl = null;
        angular.forEach($scope.media.tabs, function (tab) {
            angular.forEach(tab.properties, function (property) {
                if (property.alias == '_umb_urls') {
                    mediaUrl = property.value;
                }
            });
        });

        $scope.redirect.link = {
            id: $scope.media.id,
            name: $scope.media.name,
            url: mediaUrl ? mediaUrl : '#',
            mode: 'media'
        }

    }

    $scope.rootNodeChanged = function() {
        $scope.redirect.rootNodeId = $scope.rootNode.id;
    };

    $scope.addLink = function () {
        if ($scope.gte74) {
            $scope.linkPickerOverlay = {
                view: "linkpicker",
                title: $scope.labels.selectDestination,
                show: true,
                hideTarget: true,
                submit: function (model) {
                    $scope.linkPickerOverlay.show = false;
                    $scope.linkPickerOverlay = null;
                    $scope.redirect.link = skybrudRedirectsService.parseUmbracoLink(model.target);
                }
            };
        } else {
            skybrudRedirectsService.addLink(function (link) {
                $scope.redirect.link = link;
            }, false);
        }
    };

    $scope.editLink = function () {
        if ($scope.gte74) {
            $scope.linkPickerOverlay = {
                view: "linkpicker",
                show: true,
                currentTarget: $scope.redirect.link,
                hideTarget: true,
                title: $scope.labels.selectDestination,
                submit: function (model) {
                    $scope.linkPickerOverlay.show = false;
                    $scope.linkPickerOverlay = null;
                    $scope.redirect.link = skybrudRedirectsService.parseUmbracoLink(model.target);
                }
            };
        } else {
            skybrudRedirectsService.editLink($scope.redirect.link, function (link) {
                $scope.redirect.link = link;
            }, false);
        }
    };

    $scope.removeLink = function () {
        $scope.redirect.link = null;
    };

	$scope.hasValidUrl = function () {
		return skybrudRedirectsService.isValidUrl($scope.redirect.url, $scope.redirect.regex);
    };

    $scope.save = function () {

        if ($scope.loading) return;

        if (!$scope.redirect.url) {
            notificationsService.error($scope.labels.errorNoUrl.title, $scope.labels.errorNoUrl.message);
            return;
        }

        if (!skybrudRedirectsService.isValidUrl($scope.redirect.url)) {
            notificationsService.error($scope.labels.errorInvalidUrl.title, $scope.labels.errorInvalidUrl.message);
            return;
        }

        if (!$scope.redirect.link) {
            notificationsService.error($scope.labels.errorNoLink.title, $scope.labels.errorNoLink.message);
            return;
        }

        var params = {
            rootNodeId: $scope.redirect.rootNodeId,
            url: $scope.redirect.url,
            linkMode: $scope.redirect.link.mode,
            linkId: $scope.redirect.link.id,
            linkUrl: $scope.redirect.link.url,
            linkName: $scope.redirect.link.name,
            permanent: $scope.redirect.permanent,
            regex: $scope.redirect.regex,
            forward: $scope.redirect.forward
        };

        $scope.loading = true;

        $http({
            method: 'GET',
            url: '/umbraco/backoffice/api/Redirects/AddRedirect',
            params: params
        }).success(function () {
            $scope.loading = false;
            notificationsService.success($scope.labels.saveSuccessful.title, $scope.labels.saveSuccessful.message);
            $scope.submit($scope.redirect);
        }).error(function (res) {
            $scope.loading = false;
            notificationsService.error($scope.labels.errorAddFailed.title, res && res.meta ? res.meta.error : $scope.labels.errorAddFailed.message);
        });

    };

    function initLabels() {

        localizationService.localize('redirects_allSites').then(function (value) {
            $scope.rootNodes[0].name = value;
        });

        $scope.labels = {
            errorNoUrl: { title: 'No URL', message: 'You must specify the original URL.' },
            errorInvalidUrl: { title: 'Invalid URL', message: 'The specified URL is not valid.' },
            errorNoLink: { title: 'No link', message: 'You must select a destination page or link.' },
            errorAddFailed: { title: 'Saving failed', message: 'The redirect could not be saved due to an error on the server.' },
            saveSuccessful: { title: 'Redirect added', message: 'Your redirect has successfully been added.' },
            selectDestination: 'Select destination'
        };

        localizationService.localize('redirects_errorNoUrlTitle').then(function (value) { $scope.labels.errorNoUrl.title = value; });
        localizationService.localize('redirects_errorNoUrlMessage').then(function (value) { $scope.labels.errorNoUrl.message = value; });

        localizationService.localize('redirects_errorInvalidUrlTitle').then(function (value) { $scope.labels.errorInvalidUrl.title = value; });
        localizationService.localize('redirects_errorInvalidUrlMessage').then(function (value) { $scope.labels.errorInvalidUrl.message = value; });

        localizationService.localize('redirects_errorNoLinkTitle').then(function (value) { $scope.labels.errorNoLink.title = value; });
        localizationService.localize('redirects_errorNoLinkMessage').then(function (value) { $scope.labels.errorNoLink.message = value; });

        localizationService.localize('redirects_errorAddFailedTitle').then(function (value) { $scope.labels.errorAddFailed.title = value; });
        localizationService.localize('redirects_errorAddFailedMessage').then(function (value) { $scope.labels.errorAddFailed.message = value; });

        localizationService.localize('redirects_addSuccessfulTitle').then(function (value) { $scope.labels.saveSuccessful.title = value; });
        localizationService.localize('redirects_addSuccessfulMessage').then(function (value) { $scope.labels.saveSuccessful.message = value; });

        localizationService.localize('redirects_selectDestination').then(function (value) { $scope.labels.selectDestination = value; });

    }

    initLabels();

    skybrudRedirectsService.getRootNodes().success(function (r) {
        angular.forEach(r.data, function (rootNode) {
            $scope.rootNodes.push(rootNode);

            // If a property editor for content, the current root node (if present) should be pre-selected 
            if ($scope.content && (',' + $scope.content.path + ',').indexOf(',' + rootNode.id + ',') > 0) {
                $scope.rootNode = rootNode;
                $scope.rootNodeChanged();
            }

        });
    });

});