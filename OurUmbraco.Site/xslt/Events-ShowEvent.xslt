<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:uEvents="urn:uEvents" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers uEvents MemberLocator umbracoTags.library our.library ">


<xsl:output method="html" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">

<xsl:variable name="attendees" select="umbraco.library:GetRelatedNodesAsXml($currentPage/@id)"/>
<xsl:variable name="attending" select="$attendees//relation [@typeName = 'event']"/>
<xsl:variable name="waiting" select="$attendees//relation [@typeName = 'waitinglist']"/>

<h2><xsl:value-of select="count($attending)"/> have signed up, <xsl:value-of select="number($currentPage/capacity) - count($attending)"/> seats left</h2>

<ul class="avatarList">
<xsl:for-each select="$attending">
  <li><a href="/member/{./node/@id}" title="{./node/@nodeName}"><img src="/media/avatar/{./node/@id}.jpg" alt="{./node/@nodeName}"/></a></li>
</xsl:for-each>
</ul>

<xsl:if test="count($waiting) &gt; 0">
<div class="divider"></div>
<h2><xsl:value-of select="count($waiting)"/> on event waitinglist</h2>
<ul class="avatarList">
<xsl:for-each select="$waiting">
  <li><a href="/member/{./node/@id}" title="{./node/@nodeName}"><img src="/media/avatar/{./node/@id}.jpg" alt="{./node/@nodeName}"/></a></li>
</xsl:for-each>
</ul>
</xsl:if>

<xsl:if test="umbraco.library:RequestQueryString('debug') = 'yes'">
<textarea cols="15" rows="20">
  <xsl:copy-of select="$attending"/>
</textarea>
</xsl:if>

</xsl:template>

</xsl:stylesheet>