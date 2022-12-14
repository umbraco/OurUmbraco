angular.module('umbraco.services').factory('skybrudRedirectsService', function ($http, dialogService, notificationsService) {

    var service = {

        parseUmbracoLink: function (e) {
            return {
                id: e.id || 0,
                name: e.name || '',
                url: e.url,
                target: e.target || '_self',
                mode: (e.id ? (e.isMedia || e.mode == 'media' ? 'media' : 'content') : 'url')
            };
        },

        addLink: function (callback, closeAllDialogs) {
            closeAllDialogs = closeAllDialogs !== false;
            if (closeAllDialogs) dialogService.closeAll();
            dialogService.linkPicker({
                callback: function (e) {
                    if (!e.id && !e.url && !confirm('The selected link appears to be empty. Do you want to continue anyways?')) return;
                    if (callback) callback(service.parseUmbracoLink(e));
                    if (closeAllDialogs) dialogService.closeAll();
                }
            });
        },

        editLink: function (link, callback, closeAllDialogs) {
            closeAllDialogs = closeAllDialogs !== false;
            if (closeAllDialogs) dialogService.closeAll();
            if (link.mode == 'media') {
                dialogService.linkPicker({
                    currentTarget: {
                        name: link.name,
                        url: link.url,
                        target: link.target
                    },
                    callback: function (e) {
                        if (!e.id && !e.url && !confirm('The selected link appears to be empty. Do you want to continue anyways?')) return;
                        if (service.parseUmbracoLink(e).id == 0) {
                            e.id = link.id;
                            e.isMedia = true;
                        }
                        if (callback) callback(service.parseUmbracoLink(e));
                        if (closeAllDialogs) dialogService.closeAll();
                    }
                });
            } else {
                dialogService.linkPicker({
                    currentTarget: {
                        id: link.id,
                        name: link.name,
                        url: link.url,
                        target: link.target
                    },
                    callback: function (e) {
                        if (!e.id && !e.url && !confirm('The selected link appears to be empty. Do you want to continue anyways?')) return;
                        if (callback) callback(service.parseUmbracoLink(e));
                        if (closeAllDialogs) dialogService.closeAll();
                    }
                });
            }
        },

        addRedirect: function (options) {

            if (!options) options = {};
            if (typeof (options) == 'function') options = { callback: options };

            var d = dialogService.open({
                template: '/App_Plugins/Skybrud.Umbraco.Redirects/Views/Dialogs/Add.html',
                show: true,
                options: options,
                callback: function (value) {
                    if (options.callback) options.callback(value);
                }
            });

            // Make the dialog 20px wider than default so it can be seen bhind the linkpicker dialog
            d.element[0].style = 'display: flex; width: 460px !important; margin-left: -460px';

        },
        
        editRedirect: function (redirect, options) {

            if (!options) options = {};
            if (typeof (options) == 'function') options = { callback: options };
            
            var d = dialogService.open({
                template: '/App_Plugins/Skybrud.Umbraco.Redirects/Views/Dialogs/Edit.html',
                show: true,
                redirect: redirect,
                options: options,
                callback: function (value) {
                    if (options.callback) options.callback(value);
                }
            });

            // Make the dialog 20px wider than default so it can be seen bhind the linkpicker dialog
            d.element[0].style = 'display: flex; width: 460px !important; margin-left: -460px';

        },

        deleteRedirect: function (redirect, callback) {
            $http({
                method: 'GET',
                url: '/umbraco/backoffice/api/Redirects/DeleteRedirect',
                params: {
                    redirectId: redirect.uniqueId
                }
            }).success(function () {
                notificationsService.success('Redirect deleted', 'Your redirect was successfully deleted.');
                if (callback) callback(redirect);
            }).error(function (res) {
                notificationsService.error('Deleting redirect failed', res && res.meta ? res.meta.error : 'The server was unable to delete your redirect.');
            });
        },

        isValidUrl: function(url, isRegex) {

            // Make sure we have a string and trim all leading and trailing whitespace
            url = $.trim(url + '');

            // For now a valid URL should start with a forward slash
            return isRegex || url.indexOf('/') === 0;

        }

    };

    service.getRootNodes = function() {
        return $http.get('/umbraco/backoffice/api/Redirects/GetRootNodes');
    };

    return service;

});