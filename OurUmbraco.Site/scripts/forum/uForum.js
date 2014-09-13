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
            $.post("/umbraco/api/Forum/NewTopic/?forumId=" + forumId, { title: title, body: body,tags:tags },
            function (data) {
                window.location = jQuery("value", data).text();
            });
        },
        EditTopic: function (topicId, title, body,tags) {
            $.post("/umbraco/api/Forum/EditTopic/?topicId=" + topicId, { title: title, body: body,tags:tags },
            function (data) {
                window.location = jQuery("value", data).text();
            });
        },
        NewComment: function (topicId, items, body) {
            $.post("/umbraco/api/Forum/NewComment/?topicId=" + topicId + "&itemsPerPage=" + items, { body: body },
            function (data) {
                var forceReload = (window.location.href.indexOf("#") > -1);
                window.location = jQuery("value", data).text();

                if (forceReload) {
                    window.location.reload();
                }
            });
        },
        EditComment: function (commentId, items, body) {
            $.post("/umbraco/api/Forum/EditComment/?commentId=" + commentId + "&itemsPerPage=" + items, { body: body },
            function (data) {
                var forceReload = (window.location.href.indexOf("#") > -1);
                window.location = jQuery("value", data).text();

                if (forceReload) {
                    window.location.reload();
                }
            });
        },
        lookUp: function (useMarkdown) {
            var query = jQuery("#title").val();
            if (useMarkdown) {
                query += " " + Markdown.App.getPreviewContent();
            } else {
                query += " " + tinyMCE.get('topicBody').getContent();
            }

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
                        html += "<li><a href='/umbraco/api/Forum/TopicUrl/?topicId=" + topicId + "' target='_blank' class='similarTopicLink'>" + title + "</a></li>";
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