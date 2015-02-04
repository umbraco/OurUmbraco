var community = function () {
    return {
        /* FORUM */
        markCommentAsSolution: function (id) {
            $.get("/umbraco/api/Powers/Action/?alias=TopicSolved&pageId=" + id);
        },

        highFiveComment: function (id) {
            $.get("/umbraco/api/Powers/Action/?alias=LikeComment&pageId=" + id);
        },

        deleteComment: function (id) {

            $.ajax({
                url: "/umbraco/api/Forum/Comment/" + id,
                type: 'DELETE',
                success: function (result) {
                    // Do something with the result
                }
            });
            
        },

        deleteThread: function (id) {
            $.ajax({
                url: "/umbraco/api/Forum/Thread/" + id,
                type: 'DELETE',
                success: function (result) {
                    // Do something with the result
                }
            });
        },

        getCommentMarkdown: function (id) {
            return $.get("/umbraco/api/Forum/CommentMarkDown/" + id).pipe(function (p) {
                return p;
            });
            
        }
    };
}();


$(function () {

    /*FORUM*/

    //Mark as solution
    $(".comments").on("click","a.solved",function (e) {
        e.preventDefault();
        var data = $(this).data();
        var id = parseInt(data.id);
        community.markCommentAsSolution(id);
        $(this).closest(".comment").addClass('solution');
        $(".comment a.solved").remove();

    });

    //High five
    $(".comment .highfive a").on("click",function (e) {
        e.preventDefault();
        var data = $(this).data();
        var id = parseInt(data.id);
        community.highFiveComment(id);
        $(this).empty();
        var cont = $(this).parent();
        cont.append("You Rock!");
        var count = parseInt($(".highfive-count", cont).html());
        count++;
        $(".highfive-count", cont).html(count);

    });

    //Delete comment
    $(".comments").on("click", "a.delete-reply", function (e) {
        e.preventDefault();
        var data = $(this).data();
        var id = parseInt(data.id);
        community.deleteComment(id);
        $(this).closest(".comment").fadeOut( function () { $(this).closest(".comment").remove(); });
        
    });

    //Delete thread
    $(".delete-thread").on("click", function (e) {
        e.preventDefault();
        var data = $(this).data();
        var id = parseInt(data.id);
    });
});
