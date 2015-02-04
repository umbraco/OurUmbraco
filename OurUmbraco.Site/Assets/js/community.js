var community = function () {
    return {
        /* FORUM */
        markCommentAsSolution: function (id) {
            $.get("/umbraco/api/Powers/Action/?alias=TopicSolved&pageId=" + id);
        },

        highFiveComment: function (id) {
            $.get("/umbraco/api/Powers/Action/?alias=LikeComment&pageId=" + id);
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

    $(".comment .highfive a").click(function (e) {
        e.preventDefault();
        var data = $(this).data();
        var id = parseInt(data.id);
        community.highFiveComment(id);
        $(this).empty();
        var img = $("<img>"); 
        img.attr("src", "/assets/images/yourock.svg");
        var cont = $(this).parent();
        cont.append(img);
        cont.append("You Rock");
        var count = parseInt($(".highfive-count", cont).html());
        count++;
        $(".highfive-count", cont).html(count);

    });
});
