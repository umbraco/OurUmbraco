var community = function () {
    return {
        /* FORUM */
        markCommentAsSolution: function (id) {
            $.get("/umbraco/api/Powers/Action/?alias=TopicSolved&pageId=" + id);
        },

        highFiveComment: function (id) {
            $.get("/umbraco/api/Powers/Action/?alias=LikeComment&pageId=" + id);
        },

        deleteComment: function (id, thisComment) {

            $.ajax({
                url: "/umbraco/api/Forum/Comment/" + id,
                type: 'DELETE'
            });            
        },

        deleteThread: function (id) {
            $.ajax({
                url: "/umbraco/api/Topic/Delete/" + id,
                type: 'DELETE',
            })
            .done(function () {
                window.location = "/forum";
            });
        },

        getCommentMarkdown: function (id) {
            return $.get("/umbraco/api/Forum/CommentMarkDown/" + id).pipe(function (p) {
                return p;
            });
            
        },

        getThreadMarkdown: function (id) {
            return $.get("/umbraco/api/Forum/TopicMarkDown/" + id).pipe(function (p) {
                return p;
            });

        },
        follow: function (id,controller) {
            $.get("/umbraco/api/Notifications/SubscribeToForum"+controller+"/?id=" + id);
        },

        unfollow: function (id,controller) {
            $.get("/umbraco/api/Notifications/UnSubscribeFromForum"+controller+"/?id=" + id);
        },

        markAsSpam: function (id,controller) {
            $.post("/umbraco/api/Forum/"+controller+"AsSpam/" + id);
        },

        markAsHam: function (id,controller) {
            $.post("/umbraco/api/Forum/"+controller+"AsHam/" + id);
        },

        getCategoryUrl: function (id) {
            return $.get("/umbraco/api/PublicForum/CategoryUrl/" + id).pipe(function (p) {
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
        e.preventDefault();
        if (deepLinking === false) {
            body.addClass("active copy-prompt");
            getLink.val(window.location.hostname + window.location.pathname + $(this).attr("data-id"));
            getLink.focus().select();
            deepLinking = true;
        } else {
            body.removeClass("active copy-prompt");
            deepLinking = false;
        }
    });

    getLink.keydown(function (e) {
        if ((e.metaKey || e.ctrlKey) && e.keyCode === 67) {
            body.removeClass("active copy-prompt");

            thankyou.style.opacity = 1;
            setTimeout(function () {
                thankyou.style.opacity = 0;
            }, 900);
            deepLinking = false;
        }
    });

    $('.overlay').on('click', function () {
        body.removeClass('active copy-prompt');
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
        var $thisComment = $(this).closest(".comment");
        // $(this).closest(".comment").fadeOut(function () { $(this).closest(".comment").remove(); });

        terminateConfirm("comment", id, $thisComment);
    });

    /*Delete thread
    ==========================*/
    $(".delete-thread").on("click", function (e) {
        e.preventDefault();

        var data = $(this).data();
        var id = parseInt(data.id);

        terminateConfirm("thread", id);
    });

    // Ask for confirmation
    function terminateConfirm(typeOfPost, id, thisComment) {
        var $confirm = $('#confirm-wrapper');
        var $confirmType = $('#confirm-wrapper .type-of')
        var $body = $('body');

        $body.addClass('active confirm-prompt');

        $confirmType.html(typeOfPost);

        $('#confirm-wrapper .green').on('click', function () {

            terminatePost(typeOfPost, id, thisComment);
            $body.removeClass('active confirm-prompt');
        });

        $('#confirm-wrapper .red').on('click', function () {

            $body.removeClass('active confirm-prompt');
        });
    }

    // Terminate upon confirmation
    function terminatePost(typeOfPost, id, thisComment) {
        switch (typeOfPost) {
            case "comment":
                thisComment.closest(".comment").fadeOut(function () { thisComment.closest(".comment").remove(); });
                community.deleteComment(id, thisComment);
                break;
            case "thread":
                community.deleteThread(id);
                break;
            default:
                alert('Something went wrong')
        }
    }


    //follow thread

    //unfollow thread
    $(".follow").on("click", function (e) {
        e.preventDefault();
        var data = $(this).data();
        var id = parseInt(data.id);
        var controller = data.controller;
        if ($(this).hasClass("following")) {

            community.unfollow(id,controller);
            $(this).removeClass("following");
            $(this).addClass("transparent");
            $("span", $(this)).text("Follow");
        }
        else
        {
            community.follow(id,controller);
            $(this).addClass("following");
            $(this).removeClass("transparent");
            $("span", $(this)).text("Following");
        }
    });

    //Category filter
    $('.sorting select').on('change', function () {
        var id = $(this).val();
        community.getCategoryUrl(id).done(function (data) {
            window.location.replace(data);
           
        });;
    });

    //mark as spam

    $(".comments").on("click", "a.mark-as-spam", function (e) {
        e.preventDefault();
        var data = $(this).data();
        var id = parseInt(data.id);
        var controller = data.controller;

        community.markAsSpam(id, controller);

        $(this).removeClass("mark-as-spam");
        $(this).addClass("mark-as-ham");

        $("span", $(this)).text("Mark as ham");
    });

    //mark as spa
    $(".comments").on("click", "a.mark-as-ham", function (e) {
        e.preventDefault();
        var data = $(this).data();
        var id = parseInt(data.id);
        var controller = data.controller;

        community.markAsHam(id, controller);

        $(this).removeClass("mark-as-ham");
        $(this).addClass("mark-as-spam");

        $("span", $(this)).text("Mark as spam");

    });


    /* PROFILE */

    //upload avatar
    $(".profile-settings-forms").on("click", ".avatar-image", function(e)
    {
        var $dialog = $("#update-avatar-dialog");
        var $loader = $('.span', $dialog);
        var $file = $("input[type=file]", $dialog);
        var $cancel = $("input[type=button]", $dialog);
       
        var uploadStart = function () {
            $loader.show();
        };

        var uploadComplete = function (response) {
            
            $loader.hide();

            if (response.success) {                
                $("img", $(".profile-settings-forms")).attr("src", response.imagePath);
                $("#Avatar", $(".profile-settings-forms")).val(response.imagePath);
                $dialog.hide();
            } else {
                alert(response.message);
                $file.val('');
            }
        };

        $file.unbind('change').ajaxfileupload({
            action: $file.attr('data-action'),
            onStart: uploadStart,
            onComplete: uploadComplete
        });

        $cancel.click(function () {
            $dialog.hide();
            callback(null);
        });

        $dialog.show();
    });

    /* FORMS */

    //make sure surrounding element get's warning class
    $("form").submit(function () {
        
        if ($(this).valid()) {
            $(this).find('div.profile-input').each(function () {
                if ($(this).find('.input-validation-error').length == 0) {
                    $(this).removeClass('warning');
                }
            });
        }
        else {
            $(this).find('div.profile-input').each(function () {
                if ($(this).find('.input-validation-error').length > 0) {
                    $(this).addClass('warning');
                }
            });
        }
    });
});
