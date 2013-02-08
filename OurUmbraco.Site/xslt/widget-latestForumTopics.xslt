<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh ">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">
<xsl:variable name="maxitems" select="/macro/maxitems"/>
<xsl:variable name="topics" select="uForum.raw:LatestTopics($maxitems, 0)"/>


<ul class="forumTopics summary">
<xsl:for-each select="$topics/topics/topic">
<li class="entry-content">

  <img src="http://our.umbraco.org/media/avatar/{latestReplyAuthor}.jpg" alt="Topic author image"/>


<xsl:choose>
<xsl:when test="latestComment &gt; 0">
<a href="{uForum:NiceCommentUrl(id, latestComment, 10)}">
RE: <xsl:value-of select="umbraco.library:TruncateString(title, 40, '...')" /></a>
</xsl:when>
<xsl:otherwise>
<a class="entry-content" href="http://our.umbraco.org/{uForum:NiceTopicUrl(id)}">
<xsl:value-of select="umbraco.library:TruncateString(title, 40, '...')" /></a>
</xsl:otherwise>
</xsl:choose>
<small>Posted in <xsl:value-of select="umbraco.library:GetXmlNodeById(parentId)/@nodeName" /> by <xsl:value-of select="umbraco.library:GetMemberName(latestReplyAuthor)"/> </small>
</li>
</xsl:for-each>
</ul>

</xsl:template>


</xsl:stylesheet>