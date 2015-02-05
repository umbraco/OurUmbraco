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

    //Copy link
    var deepLinking = false;
    var getLink = $(".getLink");
    var body = $("body");
    var thankYou = $("#thankYou");

    $(".comments").on("click", "a.copy-link", function (e) {
        e.preventDefault

        if (deepLinking === false) {
            body.addClass("active");
            getLink.val(window.location.hostname + window.location.pathname + $(this).attr("data-id"));
            getLink.focus().select();
            deepLinking = true;
        } else {
            body.removeClass("active");
            deepLinking = false;
        }
    });

    getLink.keydown(function (e) {
        if ((e.metaKey || e.ctrlKey) && e.keyCode === 67) {
            body.removeClass("active");

            thankyou.style.opacity = 1;
            thankyou.style.top = "10%";
            setTimeout(function () {
                thankyou.style.opacity = 0;
                setTimeout(function () {
                    thankyou.style.top = "50%";
                }, 600);
            }, 600);
            deepLinking = false;
        }
    });

    $('.overlay').on('click', function () {
        body.removeClass('active');
        deepLinking = false;
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
