var community = function () {
    return {
        /* FORUM */
        markCommentAsSolution: function (commentId, topicId) {
            $.post("/umbraco/api/forum/MarkAsSolution/"+commentId + "?topicId=" + topicId);
        }
    };
}();


$(function () {

    /*FORUM*/
    $(".comment a.solved").click(function (e) {
        e.preventDefault();
        var data = $(this).data();
        var id = parseInt(data.id);
        var topicId = parseInt(data.topic);
        community.markCommentAsSolution(id, topicId);
        $(this).closest(".comment").addClass('solution');
        $(".comment a.solved").remove();

    });
});
