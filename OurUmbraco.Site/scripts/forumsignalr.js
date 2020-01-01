function startNotifier(memberId, memberName, modelId, isMemberAdmin) {
    var lastActivity = null;

    // if it's been long since last activity, we'll remove the "working on reply" box
    function checkActivity() {
        var timeDiff = _.now() - lastActivity;
        if (timeDiff > 60000) {
            $("#reply-is-coming").fadeOut();
        } else {
            setTimeout(checkActivity, 5000);
        }
    }

    $(function () {
        var forum = $.connection.forumPostHub;

        forum.client.someoneIsTyping = function (id, memberId, name) {
            if (id !== modelId) {
                return;
            }

            // update the date
            lastActivity = _.now();

            var replyPlaceholder = $("#reply-is-coming");

            // update meta data
            replyPlaceholder.find(".author").text(name);
            $.ajax({
                url: "/umbraco/api/Avatar/GetMemberAvatar/?memberId=" + memberId,
                type: "GET"
            }).done(function (avatarData) {
                replyPlaceholder.find(".photo").attr("href", "/member/" + memberId);
                replyPlaceholder.find("img").replaceWith(avatarData);
            });

            if (!replyPlaceholder.is(":visible")) {
                replyPlaceholder.fadeIn(1000);
                checkActivity();
            }
        };

        forum.client.DeleteComment = function (threadId, commentId) {
            if (threadId !== modelId) {
                return;
            }
            var containerId = "#comment-" + commentId;
            $(containerId).fadeOut(1000, function () { $(this).remove(); });
        };

        forum.client.returnLatestComment = function (data) {
            if (data.topicId !== modelId) {
                return;
            }

            var avatarUrl = "/umbraco/api/Avatar/GetMemberAvatar/?memberId=" + data.authorId;
            $.ajax({
                url: avatarUrl,
                type: "GET"
            }).done(function (avatarData) {
                data.avatar = avatarData;

                //new comment we'll use mustache to insert it into the dom
                data.lower = function () {
                    return function (text, render) {
                        return render(text).toLowerCase();
                    }
                };

                // hide reply in progress
                $("#reply-is-coming").fadeOut();

                data.canHaveChildren = true;
                data.isLoggedIn = memberId > 0;
                data.isCommentOwner = data.authorId === memberId;
                data.canManageComment = isMemberAdmin || data.isCommentOwner;

                if (data.isSpam === false || data.isCommentOwner) {
                    var template = $("#comment-template").html();
                    var rendered = Mustache.render(template, data);

                    if (data.parent === 0) {
                        $(".comments").append(rendered);
                        $("div.replybutton").insertAfter($("#comment-" + data.id));
                    } else {
                        var allComments = $("li[data-parent='" + data.parent + "']");
                        if (allComments.length > 0) {
                            var lastComment = 0;
                            for (var i = 0; i < allComments.length; i++) {
                                lastComment = allComments[i];
                            }
                            $(lastComment).after(rendered);
                        } else {
                            $("#comment-" + data.parent).after(rendered);
                        }
                    }


                    var notify = new PNotify({
                        title: "New answer was added",
                        text: "Jump to new answer",
                        type: "success"
                    });

                    $("#comment-" + data.id).addClass("new-signalr").fadeIn(200);
                    notify.get().css("cursor", "pointer").click(function(e) {
                        $(document).scrollTop($("#comment-" + data.id).offset().top - 80);
                        $("#comment-" + data.id).hide();
                        $("#comment-" + data.id).fadeIn();
                    });
                }
            });
        };

        forum.client.returnEditedComment = function(data) {
            if (data.topicId !== modelId) {
                return;
            }
            var container = $("#comment-" + data.id + " .body");
            container.html(data.body);
            var notify = new PNotify({
                title: "Post was edited",
                text: "Jump to modified answer"
            });

            $("#comment-" + data.id).addClass("edit-signalr").fadeIn(200);
            notify.get().css("cursor", "pointer").click(function(e) {
                $(document).scrollTop($("#comment-" + data.id).offset().top - 80);
                $("#comment-" + data.id).hide();
                $("#comment-" + data.id).fadeIn();

            });
        };

        forum.client.notify = function() {
            if ($("#wmd-input").val().length > 50) {
                forum.server.someoneIsTyping(modelId, memberId, memberName);
            }
        };

        var notifyChange = _.debounce(forum.client.notify, 500, { leading: true, trailing: false });
        // Start the connection.
        $.connection.hub.start().done(function () {
            $("#wmd-input").bind("input propertychange", notifyChange);
        });
    });
}
