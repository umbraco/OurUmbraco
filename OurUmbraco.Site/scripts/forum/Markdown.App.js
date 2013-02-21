Markdown.App = (function ($) {
    var converter,
        editor,
        preview,
        editorOptions = {
            strings: {
                linkdescription: "Insert link text here"
            }
        };

    return {
        htmlEncode: function (value) {
            return $('<div/>').text(value).html();
        },
        htmlDecode: function (value) {
            return $('<div/>').html(value).text();
        },
        getConverter: function () {
            if (!converter) {
                converter = Markdown.getSanitizingConverter();
                
                converter.hooks.chain("preConversion", function (text) {
                    // For (old) html content, we need to make sure that Markdown doesn't parse code blocks as html
                    text = text.replace(/<pre([\s\S]*?)<\/pre>/gim, function (match, p1) {
                        var subtext = p1.substring(1).replace(/</gim, "&lt;");
                        return "<pre>" + subtext + "</pre>";
                    });
                    
                    return text;
                });

                converter.hooks.chain("postConversion", function (html) {                    
                    html = $("<div>" + html + "</div>");
                    var pres = html.find("pre").addClass("prettyprint");
                    
                    pres.each(function() {
                        var pre = $(this);
                        if (pre.children("code").length === 0) {
                            pre.wrapInner("<code/>");
                        }
                    });

                    if (pres.length > 0) {
                        setTimeout(function () {
                            prettyPrint();
                        }, 1000);
                    }

                    return html.html();
                });
            }

            return converter;
        },
        getEditor: function () {
            if (!editor) {
                editor = new Markdown.Editor(this.getConverter(), "", editorOptions);

                editor.hooks.set("insertImageDialog", function (callback) {
                    window.forumInsertImageCallback = callback;
                    window.open("/insertimage", "Insert image", "width=550,height=360");
                    return true; // tell the editor that we'll take care of getting the image url
                });
            }

            return editor;
        },
        getPreviewContent: function () {
            if (!preview) {
                preview = $("#wmd-preview");
            }

            return preview.html();
        }
    };
})(jQuery);