angular.module("umbraco").controller("communityStatsController", function ($scope, $http, notificationsService) {
    var vm = this;
    
    $scope.getOurMemberStatistics = function () {
        vm.ourMemberStats = [];
        var ourMemberStatsUrl = "backoffice/API/OurMemberStatistics/GetOurMemberStatistics/";
        notificationsService.success("Downloading member data, hold on...");

        $http.get(ourMemberStatsUrl)
            .success(function (data) {
                vm.ourMemberStats = data;
                notificationsService.success("✔ Member data retrieved.");
            })
            .error(function () {
                notificationsService.error("❌ Problem retrieving Our Member statistics data");
            });
    };


    var date = new Date();
    var defaultStartDate =  new Date(date.getFullYear(), date.getMonth(), 1);

    vm.startDate = moment(defaultStartDate).subtract(12, "months").format("YYYY-MM-DD");
    vm.endDate = moment(defaultStartDate).format("YYYY-MM-DD");
    
    $scope.getOurForumTopicStatistics = function (startDate, endDate) {
        vm.ourForumStats = [];
        var ourForumStatsUrl = "backoffice/CommunityData/GetForumTopicStatistics/?startDate=" + startDate + "&endDate=" + endDate;
        $http.get(ourForumStatsUrl)
            .success(function (data) {
                window.open(ourForumStatsUrl, '_blank', '');          
                notificationsService.success("✔ Forum data retrieved.");
            })
            .error(function () {
                notificationsService.error("❌ Problem retrieving Our Forum statistics data");
            });
    };    
    
    $scope.getOurForumCommentStatistics = function (startDate, endDate) {
        vm.ourForumStats = [];        
        var ourForumStatsUrl = "backoffice/CommunityData/GetForumCommentStatistics/?startDate=" + startDate + "&endDate=" + endDate;
        $http.get(ourForumStatsUrl)
            .success(function (data) {
                window.open(ourForumStatsUrl, '_blank', '');
                notificationsService.success("✔ Forum data retrieved.");
            })
            .error(function () {
                notificationsService.error("❌ Problem retrieving Our Forum statistics data");
            });
    };
});
