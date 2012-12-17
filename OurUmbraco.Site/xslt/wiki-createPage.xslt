<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uWiki="urn:uWiki" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uWiki ">


<xsl:output method="html" omit-xml-declaration="yes"/>
<xsl:variable name="topic" select="umbraco.library:ContextKey('topic')"/>
<xsl:variable name="isAdmin" select="uForum:IsInGroup('admin') or uForum:IsInGroup('wiki editor')"/>  
    
<xsl:param name="currentPage"/>

<xsl:template match="/">
  
  <xsl:variable name="isUmbracoHelp" select="$currentPage/@level > 3 and $currentPage/ancestor-or-self::* [@isDoc and @level = 3]/@nodeName = 'Umbraco Help'" />
  
<xsl:if test="umbraco.library:IsLoggedOn()">

<xsl:value-of select="umbraco.library:RegisterJavaScriptFile('tinyMce', '/scripts/tiny_mce/tiny_mce_src.js')"/> </xsl:if>
<xsl:value-of select="umbraco.library:RegisterJavaScriptFile('uWiki', '/scripts/wiki/uWiki.js?v=22')"/>

<div id="wikiContent" style="width: 100%;">
  <p>You can start adding content to this page right away, by clicking the button below</p>
</div>

<xsl:if test="$isAdmin">
  <div id="wikiKeywordsContainer" style="display:none">
  <h3>Keywords for finding related content</h3>
  <small>Enter keywords for aggregating related projects, forum posts and videos, seperate keywords with commas</small>
    <input id="wikiKeywords" class="title" style="width:970px" value="{$currentPage/data [@alias = 'keywords']}" />
  </div>
</xsl:if>  
  
<div id="wikiButtons">
<div id="viewMode">
<button id="bt_create">Start adding content</button>
</div>
<div id="editMode" style="display: none;">
<button id="bt_save">Save</button> <em>or</em> <button id="bt_cancel">cancel</button>
</div>
</div>

<script type="text/javascript">
jQuery(document).ready(function(){
  jQuery("#bt_create").click(function(){ uWiki.NewEditor(<xsl:value-of select="$isUmbracoHelp and umbraco.library:Request('subPage') = ''" />); return false; });
  jQuery("#bt_save").click(function(){ 
  
        var wikiKeywords = '';
        if(jQuery('#wikiKeywords').length > 0){
          wikiKeywords = jQuery('#wikiKeywords').val();
        }
  
        <xsl:if test="$isUmbracoHelp">
          uWiki.ClearHelpRequests('<xsl:value-of select="Exslt.ExsltStrings:lowercase($currentPage/@nodeName)" />','<xsl:value-of select="$topic"/>');
        </xsl:if>
  
        uWiki.Create(<xsl:value-of select="$currentPage/@id"/>, jQuery('#wikiHeaderEditor').val() ,tinyMCE.get('wikiContent').getContent(), wikiKeywords);
        
       
  
          return false; 
  
  });
  jQuery("#bt_cancel").click(function(){ history.go(-1); return false; });
  
    <xsl:choose>
      <xsl:when test="$isUmbracoHelp">
         jQuery("#wikiHeader").html( "<xsl:value-of select="$topic"/>" );
      </xsl:when>
      <xsl:otherwise>
          jQuery("#wikiHeader").html( "The page '<xsl:value-of select="$topic"/>' was not found here" );
      </xsl:otherwise>
    </xsl:choose>
  
  <xsl:if test="umbraco.library:RequestQueryString('edit') = 'true'">
    uWiki.NewEditor();
  </xsl:if>  
});
</script>
</xsl:template>

</xsl:stylesheet>