<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:uForum="urn:uForum"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" 
  exclude-result-prefixes="uForum msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets ">

<xsl:output method="html" omit-xml-declaration="yes"/>
<xsl:param name="currentPage"/>
<xsl:template match="/">


<xsl:if test="umbraco.library:IsLoggedOn()">
  <xsl:variable name="mem" select="umbraco.library:GetCurrentMember()/@id"/>
  <xsl:value-of select="uForum:RegisterRssFeed( concat('http://our.umbraco.org/rss/yourtopics?id=',$mem), 'Your active topics', 'yourtopics')"/>
</xsl:if>

<xsl:choose>
<xsl:when test="$currentPage/@id = 1053">

<div id="options">
    <ul>
    <xsl:if test="umbraco.library:IsLoggedOn()">  
    <li class="right"><a class="act yourtopics" href="/forum/your-topics">Topics you participated in</a></li>
    </xsl:if>
    <li class="right"><a class="act topics" href="/forum/active-topics">Active topics</a></li>  
    </ul>
</div>


<xsl:call-template name="categories"/>

</xsl:when>
<xsl:otherwise>
  <xsl:call-template name="forums">  
    <xsl:with-param name="parent" select="uForum:Forums($currentPage/@id, true())"/>  
  </xsl:call-template>
</xsl:otherwise>
</xsl:choose>

</xsl:template>

<xsl:template name="categories">
<xsl:for-each select="$currentPage/* [@isDoc and string(umbracoNaviHide) != '1']">
<xsl:if test="count(./* [@isDoc]) &gt; 0">
<h2><xsl:value-of select="@nodeName"/></h2>
  <xsl:call-template name="forums">  
    <xsl:with-param name="parent" select="uForum:Forums(./@id, true())"/>  
  </xsl:call-template>
</xsl:if>
</xsl:for-each>
</xsl:template>

<xsl:template name="forums">
<xsl:param name="parent"/>
<xsl:if test="count($parent/forum) &gt; 0">
<table class="forumList" cellspacing="0">
<tbody>
<xsl:for-each select="$parent/forum">
<xsl:sort select="@SortOrder" />
<tr>
<th>
<a href="{umbraco.library:NiceUrl(@id)}"><xsl:value-of select="./title"/></a>
<div class="forumStats"><xsl:value-of select="./@TotalTopics"/> topics, <xsl:value-of select="(./@TotalTopics + ./@TotalComments)"/> posts</div>
<div class="forumDesc"><xsl:value-of select="./description"/></div>
</th>
<td class="forumLastPost">
<xsl:if test="number(./@LatestAuthor) &gt; 0 and ./@LatestTopic &gt; 0">
<a href="{uForum:NiceTopicUrl(./@LatestTopic)}" title="{umbraco.library:FormatDateTime(@LatestPostDate, 'MMMM d, yyyy @ hh:mm')}"><xsl:value-of select="uForum:TimeDiff(./@LatestPostDate)"/></a> by <xsl:value-of select="umbraco.library:GetMemberName(./@LatestAuthor)"/> 
</xsl:if>
</td>
</tr>
</xsl:for-each>
</tbody>
</table>
</xsl:if>
</xsl:template>

</xsl:stylesheet>