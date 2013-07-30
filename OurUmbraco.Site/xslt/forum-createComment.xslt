<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
  <!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum"
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum ">
  <xsl:output method="html" omit-xml-declaration="yes"/>
  <xsl:param name="currentPage"/>
  <xsl:variable name="body" select="umbraco.library:Request('commentBody')"/>
  <xsl:variable name="topicID" select="number(umbraco.library:ContextKey('topicID'))"/>
  <xsl:variable name="commentID" select="umbraco.library:RequestQueryString('id')"/>
  <xsl:variable name="tags" select="umbraco.library:Request('tags')"/>

  <xsl:variable name="maxitems">10</xsl:variable>
  <xsl:variable name="_body">
    <xsl:if test="$commentID != ''">
      <xsl:value-of select="uForum:Comment($commentID)//body"/>
    </xsl:if>
  </xsl:variable>
  <xsl:template match="/">
    <xsl:choose>
      <xsl:when test="$body != ''">
        <div class='success'>
          <h4>your reply has been created</h4>
          <p>Refresh the page to view it in the list</p>
        </div>
      </xsl:when>
      <xsl:otherwise>
        <xsl:choose>
          <xsl:when test="umbraco.library:IsLoggedOn()">
            <xsl:value-of select="umbraco.library:RegisterJavaScriptFile('tinyMce', '/scripts/tiny_mce_update/tiny_mce_src.js')"/>
            <xsl:value-of select="umbraco.library:RegisterJavaScriptFile('uForum', '/scripts/forum/uForum.js?v=6')"/>

            <xsl:if test="uForum:UseMarkdownEditor()">
              <xsl:value-of select="umbraco.library:RegisterStyleSheetFile('Markdown.Styles', '/css/forum/pagedown.css')"/>

              <xsl:value-of select="umbraco.library:RegisterJavaScriptFile('Markdown.Converter', '/scripts/forum/Markdown.Converter.js')"/>
              <xsl:value-of select="umbraco.library:RegisterJavaScriptFile('Markdown.Sanitizer', '/scripts/forum/Markdown.Sanitizer.js')"/>
              <xsl:value-of select="umbraco.library:RegisterJavaScriptFile('Markdown.Editor', '/scripts/forum/Markdown.Editor.js')"/>
            </xsl:if>

            <script type="text/javascript">
              uForum.ForumEditor("commentBody");

              jQuery(document).ready(function(){
              <xsl:choose>
                <xsl:when test="uForum:UseMarkdownEditor()">
                  jQuery("form").submit( function(e) {
                    e.preventDefault();

                    var topicId = '<xsl:value-of select="$topicID" />';
                    var body = $("#wmd-input").val(); // Always save the raw markdown input, otherwise, we screw up editing
                    var comment = '<xsl:value-of select="$commentID" />';

                    var url = "";

                    if(comment != ''){
                      url = uForum.EditComment(comment , <xsl:value-of select="$maxitems" />,  body);
                    } else {
                      url = uForum.NewComment(topicId, <xsl:value-of select="$maxitems" />,  body);
                    }

                    jQuery("#commentSuccess").show();
                    jQuery("#topicForm").hide();
                  });
                </xsl:when>
                <xsl:otherwise>
                  jQuery("form").submit( function(){
                    var topicId = '<xsl:value-of select="$topicID" />';
                    var body = tinyMCE.get('commentBody').getContent();

                    var comment = '<xsl:value-of select="$commentID" />';

                    var url = "";

                    if(comment != ''){
                      url = uForum.EditComment(comment , <xsl:value-of select="$maxitems" />,  body);
                    } else {
                      url = uForum.NewComment(topicId, <xsl:value-of select="$maxitems" />,  body);
                    }

                    jQuery("#commentSuccess").show();
                    jQuery("#topicForm").hide();

                    return false;
                  });
                </xsl:otherwise>
              </xsl:choose>
              });
            </script>
            <div id="topicForm">
              <fieldset>
                <xsl:choose>
                  <xsl:when test="uForum:UseMarkdownEditor()">
                    <div class="success">
                      <h4 style="text-align: center; margin: 2px;">
                        You're using the (experimental) Markdown editor. <br/><a href="/SwitchForumEditor?EditorChoice=Rte">Switch back to the old Rich Text Editor?</a>
                      </h4>
                    </div>
                    <p class="tinymce-container">
                      <textarea style="width: 100%; height: 300px" id="commentBody">
                        <xsl:value-of disable-output-escaping="yes" select="$_body"/>
                      </textarea>
                    </p>
                    <div class="wmd-container">
                      <div class="wmd-panel">
                        <div id="wmd-button-bar"></div>
                        <textarea class="wmd-input comment" id="wmd-input">
                          <xsl:value-of disable-output-escaping="yes" select="$_body"/>
                        </textarea>
                      </div>
                      <div id="wmd-preview" class="wmd-panel wmd-preview comment"></div>
                      <script type="text/javascript">
                        (function () {
                          var editor = Markdown.App.getEditor();
                          editor.run();
                        })();
                      </script>
                    </div>

                  </xsl:when>
                  <xsl:otherwise>
                    <div class="success">
                      <h4 style="text-align: center; margin: 2px;">
                        We have an (experimental) Markdown editor for you to test: <br/><a href="/SwitchForumEditor?EditorChoice=Markdown">Click here to switch to the Markdown Editor.</a>
                      </h4>
                    </div>
                    <p>
                      <textarea style="width: 100%; height: 300px" id="commentBody">
                        <xsl:value-of disable-output-escaping="yes" select="$_body"/>
                      </textarea>
                    </p>

                  </xsl:otherwise>
                </xsl:choose>
              </fieldset>

              <div class="buttons">
                <input type="submit" value="submit" id="btCreateTopic"/>
              </div>
            </div>
            <br />
            <div id="commentSuccess" style="display: none" class='success'>
              <h4 style="text-align: center;">Posting your reply</h4>
            </div>
          </xsl:when>
          <xsl:otherwise>
            <div class="notice">
              <h4 style="text-align: center;">
                Please <a href="/member/login?redirectUrl={umbraco.library:UrlEncode(uForum:NiceTopicUrl($topicID))}">login</a> or <a href="/member/signup">Sign up</a> To post replies
              </h4>
            </div>
          </xsl:otherwise>
        </xsl:choose>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>
</xsl:stylesheet>