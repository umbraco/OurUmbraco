<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh ">


<xsl:output method="html" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>

<xsl:template match="/">

<xsl:variable name="p">
<xsl:choose>
<xsl:when test="string(number( umbraco.library:RequestQueryString('p') )) != 'NaN'">
<xsl:value-of select="umbraco.library:RequestQueryString('p')"/>
</xsl:when>
<xsl:otherwise>0</xsl:otherwise>
</xsl:choose>
</xsl:variable>

<xsl:variable name="q" select="umbraco.library:RequestQueryString('q')"/>
<xsl:variable name="c" select="umbraco.library:RequestQueryString('content')"/>

<div id="search">
<xsl:if test="umbraco.library:RequestQueryString('reindex') = 'now'">
<xsl:value-of select="uSearh:Reindex(false())"/>
</xsl:if>

  
<xsl:if test="umbraco.library:RequestQueryString('reindex') = 'video'">
<xsl:value-of select="uSearh:ReIndexVideo()"/>
</xsl:if>

  
<xsl:if test="$q != ''">

<xsl:variable name="results" select="uSearh:LuceneInContentType($q, $c, $p, 255, 20)"/>

<xsl:variable name="all" select="$results/search/results/result"/>

<xsl:variable name="forumTopics" select="$results/search/results/result [contentType = 'forumTopics']"/>
<xsl:variable name="forumComments" select="$results/search/results/result [contentType = 'forumComments']"/>
<xsl:variable name="wikiPages" select="$results/search/results/result [contentType = 'wiki']"/>
<xsl:variable name="projects" select="$results/search/results/result [contentType = 'project']"/>

<p class="searchRemark"><xsl:value-of select="$results/search/results" /> items was found while searching, you can narrow down your results by unchecking categories below the search field</p>

<div id="results">
<xsl:for-each select="$all">
  <xsl:choose>
  <xsl:when test="contentType = 'forumTopics'">
    <xsl:call-template name="forumTopics"><xsl:with-param name="q" select="$q"/><xsl:with-param name="r" select="."/></xsl:call-template>
  </xsl:when>
  <xsl:when test="contentType = 'forumComments'">
    <xsl:call-template name="forumComments"><xsl:with-param name="q" select="$q"/><xsl:with-param name="r" select="."/></xsl:call-template>
  </xsl:when>
  <xsl:when test="contentType = 'wiki'">
    <xsl:call-template name="wiki"><xsl:with-param name="q" select="$q"/><xsl:with-param name="r" select="."/></xsl:call-template>
  </xsl:when>
  <xsl:when test="contentType = 'project'">
    <xsl:call-template name="project"><xsl:with-param name="q" select="$q"/><xsl:with-param name="r" select="."/></xsl:call-template>
  </xsl:when>
  <xsl:when test="contentType = 'documentation'">
    <xsl:call-template name="wiki"><xsl:with-param name="q" select="$q"/><xsl:with-param name="r" select="."/></xsl:call-template>
  </xsl:when>  
  </xsl:choose>
</xsl:for-each>
</div>

<xsl:variable name="pages" select="uSearh:LucenePager($p, $results/search/totalPages)"/>
<xsl:if test="count($pages//page) &gt; 1">
<strong>Pages: </strong><ul id="searchPager" class="pager">
<xsl:for-each select="$pages//page">
  <li>
    <xsl:if test="@current = 'true'">
      <xsl:attribute name="class">current</xsl:attribute>
    </xsl:if>

    <a href="?q={$q}&amp;p={@index}&amp;content={$c}"><xsl:value-of select="@index+1"/></a>
  </li>
</xsl:for-each>
</ul>
</xsl:if>


<script type="text/javascript">
jQuery(document).ready(function(){
  jQuery("#results").tabs();
});
</script>

</xsl:if>
</div>

</xsl:template>


<xsl:template name="forumTopics">
<xsl:param name="r" />
<xsl:param name="q" />

<xsl:variable name="t" select="uForum.raw:Topic($r/id)/topics/topic" />
<xsl:if test="string(number($t/id)) != 'NaN'">


<div class="result forumTopic">
<xsl:if test="$t/answer &gt; 0"><xsl:attribute name="class">result forumTopic solution</xsl:attribute></xsl:if>
<h3>
<a href="{uForum:NiceTopicUrl($t/id)}">
<xsl:value-of select="umbraco.library:Replace($t/title, $q, concat('&lt;em&gt;', $q ,'&lt;/em&gt;') )" disable-output-escaping="yes"/>
</a>
</h3>
<small class="meta">Posted <xsl:value-of select="umbraco.library:ShortDate($t/created)"/>, By 
<a href="/member/{$t/memberId}" style="display: inline"><xsl:value-of select="umbraco.library:GetMemberName($t/memberId)"/></a>, 
<xsl:value-of select="$t/replies"/> replies, <xsl:value-of select="$t/score"/> karma points</small>
<p>
<xsl:value-of select="umbraco.library:Replace(umbraco.library:TruncateString($t/content, 250, '...'), $q, concat('&lt;em&gt;', $q ,'&lt;/em&gt;') )" disable-output-escaping="yes"/>
</p>
<cite>https://our.umbraco.com/<xsl:value-of select="uForum:NiceTopicUrl($t/id)"/></cite>
</div>
</xsl:if>
</xsl:template>


<xsl:template name="forumComments">
<xsl:param name="r" />
<xsl:param name="q" />

<xsl:if test="$r/id &gt; 0">
<xsl:variable name="c" select="uForum:Comment($r/id)"/>

<xsl:if test="$c/@id &gt; 0 and string(number($c/@id)) != 'NaN'">
<xsl:variable name="t" select="uForum.raw:Topic($c/@topicId)/topics/topic" />
<div class="result forumComment">
<h3>
<a href="{uForum:NiceCommentUrl($c/@topicId, $r/id, 10)}">
RE: <xsl:value-of select="umbraco.library:Replace($t/title, $q, concat('&lt;em&gt;', $q ,'&lt;/em&gt;') )" disable-output-escaping="yes"/>
</a>
</h3>
<small class="meta">Posted <xsl:value-of select="umbraco.library:ShortDate($c/@created)"/>, By <a href="/member/{$r/author}" style="display: inline"><xsl:value-of select="umbraco.library:GetMemberName($r/author)"/></a></small>
<p>
<xsl:value-of select="umbraco.library:Replace(umbraco.library:TruncateString($r/content, 250, '...'), $q, concat('&lt;em&gt;', $q ,'&lt;/em&gt;') )" disable-output-escaping="yes"/>
</p>
<cite>https://our.umbraco.com/<xsl:value-of select="uForum:NiceTopicUrl($t/id)"/></cite>
</div>

</xsl:if>

</xsl:if>

</xsl:template>


<xsl:template name="wiki">
<xsl:param name="r" />
<xsl:param name="q" />
<div class="result wiki">
<h3>
<a href="{umbraco.library:NiceUrl($r/id)}">
<xsl:value-of select="umbraco.library:Replace($r/name, $q, concat('&lt;em&gt;', $q ,'&lt;/em&gt;') )" disable-output-escaping="yes"/>
</a>
</h3>
<p>
<xsl:value-of select="umbraco.library:Replace( umbraco.library:TruncateString($r/content, 250, '...'), $q, concat('&lt;em&gt;', $q ,'&lt;/em&gt;') )" disable-output-escaping="yes"/>
</p>
<cite>https://our.umbraco.com<xsl:value-of select="umbraco.library:NiceUrl($r/id)"/></cite>
</div>
</xsl:template>

<xsl:template name="documentation">
<xsl:param name="r" />
<xsl:param name="q" />
  
<div class="result wiki">
<h3>
<a href="{umbraco.library:NiceUrl($r/id)}">
<xsl:value-of select="umbraco.library:Replace($r/name, $q, concat('&lt;em&gt;', $q ,'&lt;/em&gt;') )" disable-output-escaping="yes"/>
</a>
</h3>
<p>
<xsl:value-of select="umbraco.library:Replace( umbraco.library:TruncateString($r/content, 250, '...'), $q, concat('&lt;em&gt;', $q ,'&lt;/em&gt;') )" disable-output-escaping="yes"/>
</p>
<cite>https://our.umbraco.com<xsl:value-of select="umbraco.library:NiceUrl($r/id)"/></cite>
  <p><xsl:copy-of select="$r"/></p>
</div>

</xsl:template>    
    
<xsl:template name="project">
<xsl:param name="r" />
<xsl:param name="q" />
<div class="result project">
<h3>
<a href="{umbraco.library:NiceUrl($r/id)}">
<xsl:value-of select="umbraco.library:Replace($r/name, $q, concat('&lt;em&gt;', $q ,'&lt;/em&gt;') )" disable-output-escaping="yes"/>
</a>
</h3>
<p>
<xsl:value-of select="umbraco.library:Replace( umbraco.library:TruncateString($r/content, 250, '...'), $q, concat('&lt;em&gt;', $q ,'&lt;/em&gt;') )" disable-output-escaping="yes"/>
</p>
<cite>https://our.umbraco.com<xsl:value-of select="umbraco.library:NiceUrl($r/id)"/></cite>
</div>
</xsl:template>

</xsl:stylesheet>