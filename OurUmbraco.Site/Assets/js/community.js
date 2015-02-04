var community = function () {
    return {
        /* FORUM */
        markCommentAsSolution: function (commentId) {
            $.get("/umbraco/api/Powers/Action/?alias=TopicSolved&pageId=" + commentId);
        }
    };
}();


$(function () {

    /*FORUM*/
    $(".comment a.solved").click(function (e) {
        e.preventDefault();
        var data = $(this).data();
        var id = parseInt(data.id);
        community.markCommentAsSolution(id);
        $(this).closest(".comment").addClass('solution');
        $(".comment a.solved").remove();

    });
});
