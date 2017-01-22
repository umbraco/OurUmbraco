angular.module("umbraco").controller("ourManagementController", function ($scope, $http, notificationsService) {
    $scope.downloadDocumentation = function () {
        notificationsService.success("Downloading and indexing documentation, hold on");
        var downloadUrl = "/html/githubpulltrigger";
        $http.get(downloadUrl)
            .success(function () {
                notificationsService.success("Documentation all downloaded and indexed!");
            })
            .error(function () {
                notificationsService.error("Problem with the documentation download/indexing");
            });
    };

    $scope.downloadYoutrackData = function () {
        notificationsService.success("Downloading YouTrack data, hold on");
        var youtrackUrl = "backoffice/API/YouTrackApi/GetData";
        $http.get(youtrackUrl)
            .success(function () {
                notificationsService.success("YouTrack data all downloaded!");
            })
            .error(function () {
                notificationsService.error("Problem with the YouTrack data downloading");
            });
    };
});
