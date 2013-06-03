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
                
                // Stuff to do after the conversion from markdown to html as used in the preview window
                // See Plugin hooks here: https://code.google.com/p/pagedown/wiki/PageDown
                converter.hooks.chain("postConversion", function (html) {
                    // Just wrapping the incoming html in jQuery won't work, not sure why tbh...
                    html = $("<div>" + html + "</div>");
                    var pres = html.find("pre").addClass("prettyprint"); // Find all pre-tags and mark them for prettyprint

                    if (pres.length > 0) {
                        // For whatever reason, we need a small delay before re-applying prettyprint. 
                        // We should probably throttle this, unless prettyprint has throttling build in.
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
                    window.forumInsertImageCallback = callback; // We need a global place to get to the markdown callback from the tinymce insert image window
                    window.open("/insertimage", "Insert image", "width=550,height=360"); // Open and reuse the tinymce insert image window
                    return true; // tell the editor that we'll take care of getting the image url ourselves (through tinymce)
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