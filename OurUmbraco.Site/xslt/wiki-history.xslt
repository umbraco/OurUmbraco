<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki ">


<xsl:output method="html" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">

<xsl:if test="umbraco.library:IsLoggedOn()">


<xsl:value-of select="umbraco.library:RegisterJavaScriptFile('uWiki', '/scripts/wiki/uWiki.js')"/>
<xsl:value-of select="umbraco.library:RegisterJavaScriptFile('jsDiff', '/scripts/libs/jsDiff.js')"/>
<input type="hidden" id="s_version"/>

<script type="text/javascript">
jQuery(document).ready(function(){

  jQuery(".wikiVersion").bind("mouseenter",function(){
    jQuery(this).siblings("div.versionInfo").addClass("peak");
  }).bind("mouseleave",function(){
    jQuery(this).siblings("div.versionInfo").removeClass("peak");
  }).click(function(){
    uWiki.PreviewOldVersion( <xsl:value-of select="$currentPage/@id"/>, jQuery(this).attr('rel') );
    jQuery("#s_version").val(jQuery(this).attr('rel'));

    jQuery("#history div").removeClass("show");
    jQuery(this).siblings("div.versionInfo").addClass("show");
  
    return false;  
  });  

  jQuery(".bt_rollback").click(function(){ uWiki.Rollback(<xsl:value-of select="$currentPage/@id"/>, jQuery(this).attr('rel') ); return false; });  
});
</script>


<p style="padding: 10px;">
Select the version you would like to change back to: &nbsp; 
<span style="display: none;" id="span_rollback">
<br/>
<button id="bt_rollback">Revert to this version</button> <em> or </em> 
</span> <a href="{umbraco.library:NiceUrl($currentPage/@id)}">cancel</a>
</p>

<!-- total timespan of this page -->
<xsl:variable name="ts" select="Exslt.ExsltDatesAndTimes:seconds(Exslt.ExsltDatesAndTimes:difference($currentPage/@createDate, $currentPage/@updateDate))"/>

<ul id="history">


<xsl:for-each select="uWiki:PageHistory($currentPage/@id)//* [@isDoc]">
<xsl:sort select="@updateDate" order="ascending"/>

<xsl:variable name="itemTS" select="(number(Exslt.ExsltDatesAndTimes:seconds(Exslt.ExsltDatesAndTimes:difference($currentPage/@createDate, ./@updateDate))) div $ts) * 100"/>

<li style="left: {$itemTS}%;">

<a rel="{./@version}" href="#" class="wikiVersion">
<xsl:if test="./@version = $currentPage/@version"><xsl:attribute name="class">wikiVersion current</xsl:attribute></xsl:if>meh
</a>

<div class="versionInfo rounded">
<xsl:variable name="mem" select="./data [@alias = 'author']"/>
<a href="/member{$mem}" class="author"><img src="/media/avatar/{$mem}.jpg" /></a>
<h4>Author: <a href="/member{$mem}"><xsl:value-of select="umbraco.library:GetMemberName($mem)"/></a></h4>
<small>Edited <xsl:value-of select="uForum:TimeDiff(./@updateDate)"/></small>
<a href="#" rel="{./@version}" class="bt_rollback">Restore this version</a>
</div>

</li>

</xsl:for-each>

</ul>
<small>First</small>
<small style="float: right">Latest</small>


</xsl:if>
</xsl:template>

</xsl:stylesheet>