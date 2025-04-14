angular.module("umbraco").controller("communityStatsController", function ($scope, $http, notificationsService) {
    var vm = this;
    vm.packagesByUser = [];
    
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

    $scope.getOurMemberCreatedStatistics = function () {
        vm.ourMemberCreatedStats = [];
        var ourMemberCreatedStatsUrl = "backoffice/CommunityData/GetActiveMemberSignupStatistics/"
        window.open(ourMemberCreatedStatsUrl, '_blank', '');
    };
    
    var date = new Date();
    var defaultStartDate =  new Date(date.getFullYear(), date.getMonth(), 1);

    vm.startDate = moment(defaultStartDate).subtract(12, "months").format("YYYY-MM-DD");
    vm.endDate = moment(defaultStartDate).format("YYYY-MM-DD");
    
    $scope.getOurForumTopicStatistics = function (startDate, endDate) {
        vm.ourForumStats = [];
        var ourForumStatsUrl = "backoffice/CommunityData/GetForumTopicStatistics/?startDate=" + startDate + "&endDate=" + endDate;
        window.open(ourForumStatsUrl, '_blank', '');
    };    
    
    $scope.getOurForumCommentStatistics = function (startDate, endDate) {
        vm.ourForumStats = [];        
        var ourForumStatsUrl = "backoffice/CommunityData/GetForumCommentStatistics/?startDate=" + startDate + "&endDate=" + endDate;
        window.open(ourForumStatsUrl, '_blank', '');
    };
});
