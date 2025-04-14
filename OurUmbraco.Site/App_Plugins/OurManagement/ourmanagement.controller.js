angular.module("umbraco").controller("ourManagementController", function ($scope, $http, notificationsService) {
    var vm = this;
    vm.docsLoading = false;

    $scope.downloadDocumentation = function () {
        vm.docsLoading = false;
        vm.docsMessage = "Downloading documentation, hold on...";
        notificationsService.success(vm.docsMessage);

        var downloadUrl = "backoffice/API/Docs/DownloadDocs";
        $http.get(downloadUrl)
            .success(function () {
                vm.docsLoading = false;
                vm.docsMessage = "✔ Documentation all downloaded and indexed.";
                notificationsService.success(vm.docsMessage);
            })
            .error(function () {
                vm.docsLoading = false;
                vm.docsMessage = "❌ Problem with the documentation download/indexing.";
                notificationsService.error(vm.docsMessage);
            });
    };

    $scope.findMember = function (searchTerm) {
        var getMemberSearchUrl = "backoffice/API/Members/SearchMembers/?searchTerm=" + searchTerm;
        $http.get(getMemberSearchUrl)
            .success(function (data) {
                if (data.length === 0) {
                    notificationsService.error("❌ No members found that match the search term(s).");
                }
                if (data.length === 1) {
                    $scope.selectMember(data[0].Key, data[0].Name, data[0].Id, data[0].Email);
                } else {
                    vm.memberSearchResults = data;
                }
            })
            .error(function () {
                vm.memberError = "❌ Problem with getting the member search results.";
                notificationsService.error(vm.memberError);
            });
    };

    $scope.selectMember = function (memberKey, memberName, memberId, memberEmail) {
        vm.memberSearchResults = undefined;
        vm.memberId = memberId;
        vm.memberBackofficeUrl = "/umbraco/#/member/member/edit/" + memberKey;
        vm.memberName = memberName;
        vm.memberEmail = memberEmail;
    };
    
    $scope.addContribBadgeToMember = function (memberId) {
        var getMemberUrl = "backoffice/API/Members/AddContribBadgeToMember/?memberId=" + memberId;
        $http.get(getMemberUrl)
            .success(function (data) {
                if (data === "true") {
                    vm.addBadgeMessage = "✔ " + vm.memberName + " now has a contrib badge.";
                    notificationsService.success(vm.addBadgeMessage);
                } else {
                    notificationsService.error("❌ Problem with adding contrib badge to the member.");
                }
            })
            .error(function () {
                vm.memberError = "❌ Problem with getting the member.";
                notificationsService.error(vm.memberError);
            });
    };

    $scope.addKarmaToMember = function (memberId) {
        var getKarmaGrantingUrl = "backoffice/API/Members/GrantKarmaForPackageUpload/?memberId=" + memberId;
        $http.get(getKarmaGrantingUrl)
            .success(function (data) {
                if (data === "true") {
                    vm.addBadgeMessage = "✔ " + vm.memberName + " now has enough karma to create a package.";
                    notificationsService.success(vm.addBadgeMessage);
                } else {
                    notificationsService.error("❌ Problem with adding karma to the member.");
                }
            })
            .error(function () {
                vm.memberError = "❌ Problem with getting the member.";
                notificationsService.error(vm.memberError);
            });
    };

    $scope.getInvalidDocsArticles = function () {
        var getInvalidDocsUrl = "backoffice/API/Docs/GetInvalidDocsArticles";
        $http.get(getInvalidDocsUrl)
            .success(function (data) {
                if (data.length === 0) {
                    vm.invalidDocsArticles = "No articles found with invalid YAML";
                }
                else {
                    vm.invalidDocsArticles = data;
                }
            })
            .error(function () {
                vm.docsError = "❌ Problem with getting the invalid docs article results.";
                notificationsService.error(vm.memberError);
            });
    };
});
