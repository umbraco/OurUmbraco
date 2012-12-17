<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:uEvents="urn:uEvents" xmlns:MemberLocator="urn:MemberLocator" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:our.library="urn:our.library" xmlns:Notifications="urn:Notifications" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers uEvents MemberLocator umbracoTags.library our.library Notifications ">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>
<xsl:variable name="keywords" select="/macro/keywords" />
    
<xsl:template match="/">

<xsl:if test="$keywords != '' and $keywords != 'undefined'">  
<xsl:variable name="results" select="uSearh:LuceneInContentType($keywords, 'wiki,forumTopics', 0, 255, 100)"/>
<xsl:variable name="all" select="$results/search/results/result"/>

<xsl:variable name="forumTopics" select="$results/search/results/result [contentType = 'forumTopics']"/>
<xsl:variable name="wikiPages" select="$results/search/results/result [contentType = 'wiki']"/>
<xsl:variable name="projects" select="$results/search/results/result [contentType = 'project']"/>

  
<div class="spotlight">  
  <h4>Related pages</h4>
  <ul class="wiki summary">
    <xsl:for-each select="$wikiPages">
        <xsl:if test="position() &lt; 10">
        <xsl:call-template name="wiki">
          <xsl:with-param name="r" select="." />
        </xsl:call-template>
        </xsl:if>  
    </xsl:for-each>
  </ul>
</div>  
  
  
<div class="spotlight">  
  <h4>Related forum topics</h4>
  <ul class="forum summary">
  <xsl:for-each select="$forumTopics">
    <xsl:if test="position() &lt; 10">
        <xsl:call-template name="forum">
          <xsl:with-param name="r" select="." />
        </xsl:call-template>
    </xsl:if>
    </xsl:for-each>
  </ul>
</div>

  
<div class="spotlight">  
  <h4>Related projects</h4>
  <ul class="projects summary">
  <xsl:for-each select="$projects">
    <xsl:if test="position() &lt; 10">
        <xsl:call-template name="project">
          <xsl:with-param name="r" select="." />
        </xsl:call-template>
    </xsl:if>
    </xsl:for-each>
  </ul>
</div>
  
</xsl:if>
  
</xsl:template>

              
    
<xsl:template name="forum">
<xsl:param name="r" />

<xsl:variable name="t" select="uForum.raw:Topic($r/id)/topics/topic" />
<xsl:if test="string(number($t/id)) != 'NaN'">

<li class="entry-content">
<img src="/media/avatar/{$t/latestReplyAuthor}.jpg" alt="Topic author image"/>

<xsl:choose>
<xsl:when test="latestComment &gt; 0">
  <a href="{uForum:NiceCommentUrl($t/id, $t/latestComment, 10)}">
    RE: <xsl:value-of select="$t/title" /></a>
</xsl:when>
<xsl:otherwise>
  <a class="entry-content" href="{uForum:NiceTopicUrl($t/id)}">
    <xsl:value-of select="$t/title" /></a>
</xsl:otherwise>
</xsl:choose>
  <small>Posted in <xsl:value-of select="umbraco.library:GetXmlNodeById($t/parentId)/@nodeName" /> by <xsl:value-of select="umbraco.library:GetMemberName($t/latestReplyAuthor)"/> </small>
</li>

</xsl:if>
</xsl:template>



<xsl:template name="wiki">
<xsl:param name="r" />
  <xsl:variable name="node" select="umbraco.library:GetXmlNodeById($r/id)" />
  <xsl:variable name="parent" select="umbraco.library:GetXmlNodeById($node/@parentID)" />  
  <li>

    <a href="{umbraco.library:NiceUrl($r/id)}">
    <xsl:value-of select="$node/@nodeName" disable-output-escaping="yes"/>
    </a> 
   <small>In <xsl:value-of select="$parent/@nodeName" />, by <xsl:value-of select="umbraco.library:GetMemberName($node/author)"/>  </small>
</li>
</xsl:template>

    
<xsl:template name="project">
<xsl:param name="r" />
  <xsl:variable name="node" select="umbraco.library:GetXmlNodeById($r/id)" />
  <xsl:variable name="parent" select="umbraco.library:GetXmlNodeById($node/@parentID)" />  
  <li>
    <a href="{umbraco.library:NiceUrl($r/id)}">
    <xsl:value-of select="$node/@nodeName" disable-output-escaping="yes"/>
    </a> 
   <small>In <xsl:value-of select="$parent/@nodeName" />, by <xsl:value-of select="umbraco.library:GetMemberName($node/author)"/>  </small>
</li>
</xsl:template>    
    
</xsl:stylesheet>