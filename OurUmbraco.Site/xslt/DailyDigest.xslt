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

  <xsl:variable name="yesterday" select="umbraco.library:ShortDate(umbraco.library:DateAdd(umbraco.library:CurrentDate(),'d',-1))" />
  
  <xsl:variable name="projects" select="$currentPage/ancestor-or-self::node [@level = 1]/node [@nodeTypeAlias = 'Projects']//node [@nodeTypeAlias = 'Project' and umbraco.library:DateGreaterThan(@createDate,$yesterday)]" />  
  <xsl:variable name="topics" select="uForum.raw:LatestTopicsSinceDate(50, 0,umbraco.library:DateAdd(umbraco.library:CurrentDate(),'d',-1))" />
 
    
<xsl:template match="/">

  

  <xsl:if test="count($projects) &gt; 0">
  <h2>New Projects</h2>
  
    <ul>
    <xsl:for-each select="$projects">
      
      <li><a href="umbraco.library:NiceUrl(./@id)"><xsl:value-of select="./@nodeName"/></a></li>
    </xsl:for-each>
  </ul>
  </xsl:if>

  <xsl:if test="count($topics/topics/topic) &gt; 0">
  <h3>Forum topics</h3>

     <ul>
     <xsl:for-each select="$topics/topics/topic">
       <li><xsl:value-of select="."/></li>
     </xsl:for-each>
    </ul>
  </xsl:if>
</xsl:template>

</xsl:stylesheet>