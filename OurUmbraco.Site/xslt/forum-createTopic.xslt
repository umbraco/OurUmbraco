<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [
    <!ENTITY nbsp "&#x00A0;">
]>
<xsl:stylesheet
  version="1.0"
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum"
  xmlns:uForum.raw="urn:uForum.raw"
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw">
    <xsl:output method="html" omit-xml-declaration="yes"/>
    <xsl:param name="currentPage"/>
    <xsl:variable name="id" select="umbraco.library:RequestQueryString('id')"/>
    <xsl:variable name="body" select="umbraco.library:Request('topicBody')"/>
    <xsl:variable name="title" select="umbraco.library:Request('title')"/>
    <xsl:variable name="successMessage">
        <h1>your topic has been created</h1>
    </xsl:variable>
    <xsl:template match="/">
        <xsl:variable name="_body">
            <xsl:if test="$id != ''">
                <xsl:value-of select="uForum.raw:Topic($id)/topics/topic/body"/>
            </xsl:if>
        </xsl:variable>
        <xsl:variable name="_title">
            <xsl:if test="$id != ''">
                <xsl:value-of select="uForum.raw:Topic($id)/topics/topic/title"/>
            </xsl:if>
        </xsl:variable>
        <xsl:choose>
            <xsl:when test="$body != '' and $title != ''">
                <xsl:value-of select="$successMessage" disable-output-escaping="yes"/>
                Here we will submit it server side...
            </xsl:when>
            <xsl:otherwise>
                <xsl:value-of select="umbraco.library:RegisterJavaScriptFile('tinyMce', '/scripts/tiny_mce/tiny_mce_src.js')"/>
                <xsl:value-of select="umbraco.library:RegisterJavaScriptFile('uForum', '/scripts/forum/uForum.js?v=11')"/>
                <script type="text/javascript">
    uForum.ForumEditor("topicBody");  

    jQuery(document).ready(function(){    
                        window.setInterval(function() {
                            uForum.lookUp();
                        }, 10000);

                        jQuery("form").submit( function(e) {
                            e.preventDefault();
                        
            jQuery("#btCreateTopic").attr("disabled", "true");
            jQuery("#topicForm").hide();
            jQuery(".success").show();

            var topicId = '<xsl:value-of select="$id"/>';
            var forumId = <xsl:value-of select="$currentPage/@id"/>;
            var title = jQuery("#title").val();
                            var body = $("#wmd-input").val(); // Always save the raw markdown input, otherwise, we screw up editing

      if(topicId !== "") {
                uForum.EditTopic(topicId, title, body );
            } else {
                uForum.NewTopic(forumId, title, body );
                            }
          

        });
                    });
                </script>
                <div class="success" style="display:none;" id="commentSuccess">
                    <h4 style="text-align: center;">Posting your topic</h4>
                </div>
                <div id="topicForm">
                    <fieldset>
                        <p>
                            <input type="text" id="title" class="title" style="width: 670px;" value="{$_title}" />
                        </p>
                        <p class="tinymce-container">
                            <textarea style="width: 680px; height: 300px" id="topicBody">
                                <xsl:value-of disable-output-escaping="yes" select="$_body"/>
                            </textarea>
                        </p>
                        <div class="wmd-container">
                            <div class="wmd-panel">
                                <div id="wmd-button-bar"></div>
                                <textarea class="wmd-input topic" id="wmd-input">
                                    <xsl:value-of disable-output-escaping="yes" select="$_body"/>
                                </textarea>
                            </div>
                            <div id="wmd-preview" class="wmd-panel wmd-preview topic"></div>
                            <script type="text/javascript">
                                (function () {
                                    Markdown.App.getEditor().run();                                    
                                })();
                            </script>
                        </div>
                        <div class="buttons">
                            <input type="submit" value="submit" id="btCreateTopic"/>
                            <xsl:if test="$id != ''">
                                &nbsp;<em> or </em> &nbsp;<a href="{uForum:NiceTopicUrl($id)}">cancel</a>
                            </xsl:if>
                        </div>
                    </fieldset>
                </div>                
            </xsl:otherwise>
        </xsl:choose>
    </xsl:template>
</xsl:stylesheet>