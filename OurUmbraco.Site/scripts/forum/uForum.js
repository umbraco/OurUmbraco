var s_currentLookUp = '';

var uForum = function () {
    return {
        ForumEditor: function (id) {
            tinyMCE.init({
                // General options
                mode: "exact",
                elements: id,
                content_css: "/css/fonts.css",
                auto_resize: true,
                theme: "advanced",
                remove_linebreaks: false,
                relative_urls: false,
                plugins: "insertimage",
                theme_advanced_buttons1_add: "insertimage",
                theme_advanced_buttons1: "bold,strikethrough,|,bullist,numlist,|,link,unlink,formatselect,insertimage,code"
            });
        },
        NewTopic: function (forumId, title, body,tags) {
            $.post("/base/uForum/NewTopic/" + forumId + ".aspx", { title: title, body: body,tags:tags },
            function (data) {
                window.location = jQuery("value", data).text();
            });
        },
        EditTopic: function (topicId, title, body,tags) {
            $.post("/base/uForum/EditTopic/" + topicId + ".aspx", { title: title, body: body,tags:tags },
            function (data) {
                window.location = jQuery("value", data).text();
            });
        },
        NewComment: function (topicId, items, body) {
            $.post("/base/uForum/NewComment/" + topicId + "/" + items + ".aspx", { body: body },
            function (data) {
                var forceReload = (window.location.href.indexOf("#") > -1);
                window.location = jQuery("value", data).text();

                if (forceReload) {
                    window.location.reload();
                }
            });
        },
        EditComment: function (commentId, items, body) {
            $.post("/base/uForum/EditComment/" + commentId + "/" + items + ".aspx", { body: body },
            function (data) {
                var forceReload = (window.location.href.indexOf("#") > -1);
                window.location = jQuery("value", data).text();

                if (forceReload) {
                    window.location.reload();
                }
            });
        },
        lookUp: function () {
            var query = jQuery("#title").val();
            query += " " + tinyMCE.get('topicBody').getContent();

            if (query.length <= 1) {
                jQuery("#topicsBox").fadeOut("fast");
            }
            if (query.length > 10 && query != s_currentLookUp) {
                jQuery("#topicsBox").fadeIn("fast");

                s_currentLookUp = query;
                $.post("/base/uSearch/FindSimiliarItems/forumTopics/20.aspx", { q: query },
                function (data) {
                    var found = false;
                    
                    var html = "<ul class='summary'>";
                    jQuery.each(jQuery("result", data), function (index, value) {
                        var title = jQuery(value).find("Title").text();
                        var topicId = jQuery(value).find("__NodeId").text();
                        html += "<li><a href='/base/uForum/TopicUrl/" + topicId + ".aspx' target='_blank' class='similarTopicLink'>" + title + "</a></li>";
                        found = true;
                    });
                    html += "</ul>";

                    if (found) {
                        jQuery("#suggestedTopics").html(html);
                    } else {
                        jQuery("#topicsBox").fadeOut("fast");
                    }
                }, "xml");
            } else {
            }
        }
    };
}();