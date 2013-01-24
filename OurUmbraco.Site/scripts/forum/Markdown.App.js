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
        getConverter: function () {
            if (!converter) {
                converter = Markdown.getSanitizingConverter();
                converter.hooks.chain("postConversion", function (html) {
                    html = $("<div>" + html + "</div>");
                    var pres = html.find("pre").addClass("prettyprint");
                    if (pres.length > 0) {
                        setTimeout(function () {
                            prettyPrint();
                        }, 2000);
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