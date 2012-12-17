<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
	version="1.0" 
	xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
	xmlns:msxml="urn:schemas-microsoft-com:xslt"
	xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:MemberLocator="urn:MemberLocator" 
	exclude-result-prefixes="umbracoTags.library msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers MemberLocator ">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>
<xsl:variable name="tags" select="umbracoTags.library:getAllTagsInGroup('project')"/>
<xsl:variable name="max" select="Exslt.ExsltMath:max($tags//tag/@nodesTagged)"/>
<xsl:variable name="sum" select="sum($tags//tag/@nodesTagged)"/>
<xsl:variable name="currentTag" select="umbraco.library:RequestQueryString('tag')"/>


<xsl:template match="/">


<div id="tagCloud">
<xsl:for-each select="$tags//tag">
<xsl:sort select="." order="ascending"/>

<xsl:if test="number(@nodesTagged) &gt; 1">
<xsl:call-template name="tagHtml">
	<xsl:with-param name="tag" select="."/>
</xsl:call-template>
</xsl:if>

</xsl:for-each>
</div>

</xsl:template>


<xsl:template name="tagHtml">
<xsl:param name="tag"/>
<xsl:variable name="weight"> <xsl:value-of select="($tag/@nodesTagged div $max) * 100"/></xsl:variable>
<xsl:variable name="cssClass">
<xsl:choose>
<xsl:when test="$weight &gt;= 99">weight1</xsl:when>
<xsl:when test="$weight &gt;= 70">weight2</xsl:when>
<xsl:when test="$weight &gt;= 40">weight3</xsl:when>
<xsl:when test="$weight &gt;= 20">weight4</xsl:when>
<xsl:when test="$weight &gt;= 3">weight5</xsl:when>
<xsl:otherwise>weight0</xsl:otherwise>
</xsl:choose>
</xsl:variable>

<a href="/projects/tag/{$tag}#projectList" class="{$cssClass}">
<xsl:if test="$tag = $currentTag"><xsl:attribute name="class"><xsl:value-of select="$cssClass"/> current</xsl:attribute></xsl:if>
<xsl:value-of select="$tag"/></a>&nbsp; 
</xsl:template>

</xsl:stylesheet>