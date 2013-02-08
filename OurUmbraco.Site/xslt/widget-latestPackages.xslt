<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE xsl:stylesheet [ <!ENTITY nbsp "&#x00A0;"> ]>
<xsl:stylesheet 
  version="1.0" 
  xmlns:xsl="http://www.w3.org/1999/XSL/Transform" 
  xmlns:msxml="urn:schemas-microsoft-com:xslt" 
  xmlns:umbraco.library="urn:umbraco.library" xmlns:Exslt.ExsltCommon="urn:Exslt.ExsltCommon" xmlns:Exslt.ExsltDatesAndTimes="urn:Exslt.ExsltDatesAndTimes" xmlns:Exslt.ExsltMath="urn:Exslt.ExsltMath" xmlns:Exslt.ExsltRegularExpressions="urn:Exslt.ExsltRegularExpressions" xmlns:Exslt.ExsltStrings="urn:Exslt.ExsltStrings" xmlns:Exslt.ExsltSets="urn:Exslt.ExsltSets" xmlns:uForum="urn:uForum" xmlns:uForum.raw="urn:uForum.raw" xmlns:uWiki="urn:uWiki" xmlns:uSearh="urn:uSearh" 
  exclude-result-prefixes="msxml umbraco.library Exslt.ExsltCommon Exslt.ExsltDatesAndTimes Exslt.ExsltMath Exslt.ExsltRegularExpressions Exslt.ExsltStrings Exslt.ExsltSets uForum uForum.raw uWiki uSearh ">

<xsl:output method="xml" omit-xml-declaration="yes" />

<xsl:param name="currentPage"/>

<!-- Don't change this, but add a 'contentPicker' element to -->
<!-- your macro with an alias named 'source' -->
<xsl:variable name="source" select="/macro/source"/>
<xsl:variable name="maxitems" select="/macro/maxitems"/>
    
<xsl:template match="/">

<!-- The fun starts here -->
<ul class="projects summary">
<xsl:for-each select="umbraco.library:GetXmlNodeById($source)/descendant::Project [projectLive = '1']">
<xsl:sort select="@createDate" order="descending"/>
<xsl:if test="position() &lt;= $maxitems">
  <li><a href="http://our.umbraco.org/{umbraco.library:NiceUrl(@id)}"><xsl:value-of select="@nodeName"/></a>
  <small>Version: <xsl:value-of select="version"/>,  by <xsl:value-of select="umbraco.library:GetMemberName(owner)"/> </small>
  </li>
</xsl:if>
</xsl:for-each>
</ul>

</xsl:template>

</xsl:stylesheet>