<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt"
  xmlns:our.library="urn:our.library" xmlns:umbracoTags.library="urn:umbracoTags.library" xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" xmlns:uPowers="urn:uPowers" xmlns:MemberLocator="urn:MemberLocator" 
  exclude-result-prefixes="umbracoTags.library our.library msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh uPowers MemberLocator ">


<xsl:output method="xml" omit-xml-declaration="yes"/>

<xsl:param name="currentPage"/>
<xsl:variable name="tags" select="umbracoTags.library:getAllTagsInGroup('project')"/>
<xsl:variable name="max" select="Exslt.ExsltMath:max($tags//tag/@nodesTagged)"/>
<xsl:variable name="sum" select="sum($tags//tag/@nodesTagged)"/>
<xsl:variable name="currentTag" select="umbraco.library:RequestQueryString('tag')"/>

<xsl:variable name="recordsPerPage" select="25"/>
<xsl:variable name="pageNumber" >
<xsl:choose>
<xsl:when test="umbraco.library:RequestQueryString('p') &lt;= 0 or string(umbraco.library:RequestQueryString('p')) = '' or string(umbraco.library:RequestQueryString('page')) = 'NaN'">0</xsl:when>
<xsl:otherwise>
<xsl:value-of select="umbraco.library:RequestQueryString('p')"/>
</xsl:otherwise>
</xsl:choose>
</xsl:variable>

<xsl:template match="/">

<xsl:choose>
<xsl:when test="umbraco.library:RequestQueryString('tag') != '' and umbraco.library:RequestQueryString('tag') != 'all'">


<a name="projectList">&nbsp;</a>
<div class="spotlight">
<h2>Projects tagged '<xsl:value-of select="umbraco.library:RequestQueryString('tag')"/>' <a href="/projects/tag/all">(all projects)</a></h2>

<xsl:call-template name="projectList">
  <xsl:with-param name="projects" select="umbracoTags.library:getContentsWithTags(umbraco.library:RequestQueryString('tag'))//Project"/>
</xsl:call-template>



</div>
</xsl:when>

<xsl:when test="umbraco.library:RequestQueryString('tag') = 'all'">
<div class="spotlight">
<h2>All Projects (sorted by name)</h2>

<xsl:call-template name="projectList">
  <xsl:with-param name="projects" select="$currentPage/descendant::Project"/>
</xsl:call-template>

</div>
</xsl:when>

<xsl:otherwise>
<xsl:call-template name="projectList">
  <xsl:with-param name="projects" select="$currentPage/descendant::Project"/>
</xsl:call-template>  
</xsl:otherwise>
</xsl:choose>
</xsl:template>


<xsl:template name="projectList">
<xsl:param name="projects" />



<xsl:variable name="numberOfRecords" select="count($projects/* [@isDoc])"/>

<ul class="summary projectsTagged">
<xsl:for-each select="$projects">
<xsl:sort select="@nodeName" order="ascending"/>

<xsl:if test="position() &gt; $recordsPerPage * number($pageNumber) and
position() &lt;= number($recordsPerPage * number($pageNumber) +
$recordsPerPage )">

  <xsl:call-template name="projectHtml">
  <xsl:with-param name="project" select="."/>
  </xsl:call-template>

</xsl:if>

</xsl:for-each>
</ul>


<xsl:if test="$numberOfRecords &gt; $recordsPerPage">
<strong>Pages: </strong>
<ul id="projectPager" class="pager">

<xsl:call-template name="paging.loop">
  <xsl:with-param name="i">0</xsl:with-param>
  <xsl:with-param name="count" select="ceiling($numberOfRecords div $recordsPerPage)"></xsl:with-param>
</xsl:call-template>

</ul>
</xsl:if>

</xsl:template>

<xsl:template name="paging.loop">
                <xsl:param name="i"/>
                <xsl:param name="count"/>
                
                <xsl:if test="$i &lt; $count">

        <li>
        <xsl:if test="$pageNumber = $i ">
                      <xsl:attribute name="class">
                                    <xsl:text>current</xsl:text>
                                  </xsl:attribute>
                                </xsl:if>
                                <a href="?p={$i}#projectList">
                                        <xsl:value-of select="$i + 1" />
                                </a>
        </li>

                        <xsl:call-template name="paging.loop">
                                <xsl:with-param name="i" select="$i + 1" />
                                <xsl:with-param name="count" select="$count">
                                </xsl:with-param>
                        </xsl:call-template>
                </xsl:if>
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

<a href="/projects/tag/{$tag}" class="{$cssClass}">
<xsl:if test="$tag = $currentTag"><xsl:attribute name="class"><xsl:value-of select="$cssClass"/> current</xsl:attribute></xsl:if>

<xsl:value-of select="$tag"/></a>&nbsp; 
</xsl:template>


<xsl:template name="projectHtml">
<xsl:param name="project"/>
  <li>

  <xsl:choose>
  <xsl:when test="string-length($project/defaultScreenshotPath) &gt; 0">
    <div style="float:left;padding-top:15px;width:75px;height:80px;">
      <div style="padding-left:16px;">
        <img style="width:40px;height:40px" src="/umbraco/imagegen.ashx?image={$project/defaultScreenshotPath}&amp;pad=true&amp;width=40&amp;height=40;"  alt="{$project/@nodeName}"/>
      </div>
    </div>
  </xsl:when>
  <xsl:otherwise>
    <div style="float:left;padding-top:10px;width:75px;padding-bottom:40px;overflow:hidden;">
      <div style="padding-left:18px;">
      <img src="/css/img/package.png" alt="no screenshots uploaded" />
      </div>
    </div>
  </xsl:otherwise>
  </xsl:choose>

  <h5><a href="{umbraco.library:NiceUrl($project/@id)}"><xsl:value-of select="$project/@nodeName"/></a></h5>
  <xsl:if test="string-length($project/description) &gt; 0">
  <p style="min-height:30px;">
    <xsl:choose>
      <xsl:when test="string-length(our.library:StripHTML($project/description)) &gt; 320">
        <xsl:value-of select="substring(our.library:StripHTML($project/description),0,320)" disable-output-escaping="yes"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="our.library:StripHTML($project/description)" disable-output-escaping="yes"/>
      </xsl:otherwise>
    </xsl:choose>
     ... <a href="{umbraco.library:NiceUrl($project/@id)}" style="display:inline;">more</a>
  </p>
  </xsl:if>
  <small><strong>Status: </strong> <xsl:value-of select="$project/status"/>, <strong> Version: </strong> <xsl:value-of select="$project/version"/>,  <strong> By: </strong> <xsl:if test="string-length($project/owner) &gt; 0"><xsl:value-of select="umbraco.library:GetMemberName($project/owner)"/></xsl:if> </small>
  <br style="clear:both" />
  </li>
</xsl:template>

</xsl:stylesheet>