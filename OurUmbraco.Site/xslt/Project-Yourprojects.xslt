<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt" 
  xmlns:our.library="urn:our.library"
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" 
  exclude-result-prefixes="msxml our.library umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh ">

<xsl:output method="xml" omit-xml-declaration="yes" />

<xsl:param name="currentPage"/>

<!-- Don't change this, but add a 'contentPicker' element to -->
<!-- your macro with an alias named 'source' -->
<xsl:variable name="source" select="/macro/source"/>
<xsl:variable name="editor" select="/macro/editor"/>
<xsl:variable name="uploader" select="/macro/uploader"/>
<xsl:variable name="forums" select="/macro/forums"/>
<xsl:variable name="team" select="/macro/team"/>
<xsl:variable name="licenses" select="/macro/licenses"/>
    
<xsl:template match="/">

<xsl:if test="umbraco.library:IsLoggedOn()">
<xsl:variable name="mem" select="uForum:GetCurrentMember()"/>

<h3><a href="{umbraco.library:NiceUrl($editor)}">Create a new project</a></h3>




<xsl:if test="count(umbraco.library:GetXmlNodeById($source)/descendant::Project [owner = $mem/@id]) &gt;0">
<h3>Your existing projects</h3>
<ul class="yourProjects">
  <xsl:for-each select="umbraco.library:GetXmlNodeById($source)/descendant::Project [owner = $mem/@id]">
<xsl:sort select="@nodeName" />
  <li>
    <div class="projectImage">
    <img alt="no screenshots uploaded" src="/css/img/package.png"/>
    </div>
    <div class="projectActions">
    <h2><a href="{umbraco.library:NiceUrl(@id)}"><xsl:value-of select="@nodeName"/></a></h2>
    <small>Created on <xsl:value-of select="umbraco.library:ShortDate(@createDate)"/></small>
    <p><a href="{umbraco.library:NiceUrl($editor)}?id={@id}">Edit project</a></p>
    <p><a href="{umbraco.library:NiceUrl($forums)}?id={@id}">Setup project forums</a></p>
    <p><a href="{umbraco.library:NiceUrl($licenses)}?id={@id}">Manage Licenses</a></p>
    <p><a href="{umbraco.library:NiceUrl($team)}?id={@id}">Manage team</a></p>  
    </div>
     <div style="clear:both;"></div>
  </li>
</xsl:for-each>
</ul>
  
  <div style="clear:both;" />

</xsl:if>

  
  <xsl:variable name="contripr" select="our.library:ProjectsContributing($mem/@id)" />
  <xsl:if test="count($contripr//projectId) &gt; 0">
  <h3>Project where you are contributor</h3>
  
  <ul class="yourProjects">
    <xsl:for-each select="$contripr//projectId">
    <xsl:sort select="umbraco.library:GetXmlNodeById(.)/@nodeName" />
      <xsl:variable name="project" select="umbraco.library:GetXmlNodeById(.)" />
  <li>
    <div class="projectImage">
    <img alt="no screenshots uploaded" src="/css/img/package.png"/>
    </div>
    <div class="projectActions">
      <h2><a href="{umbraco.library:NiceUrl(.)}"><xsl:value-of select="$project/@nodeName"/></a></h2>
      <small>Created by <a href="/member/{$project/data [@alias = 'owner']}"><xsl:value-of select="umbraco.library:GetMemberName($project/owner)"/></a> on <xsl:value-of select="umbraco.library:ShortDate($project/@createDate)"/></small>
    <p><a href="{umbraco.library:NiceUrl($editor)}?id={.}">Edit project</a>
    
    </p>
    <p><a href="{umbraco.library:NiceUrl($uploader)}?id={.}">Manage project files</a>

    </p>
 
     
    </div>
     <div style="clear:both;"></div>
  </li>
</xsl:for-each>
</ul>
  </xsl:if>
  


  
</xsl:if>
</xsl:template>


    
</xsl:stylesheet>